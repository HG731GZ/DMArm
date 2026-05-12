using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DMArmDLL;
using System.IO;
using System.Threading;
using static System.Windows.Forms.LinkLabel;
using System.Runtime.CompilerServices;
using MathNet.Numerics.LinearAlgebra;
using ZLGCAN;

namespace DMArmAPP
{
	public partial class Form1 : Form
	{
		public USBCANFD canfd;
		public Robot_UR vrobot, rrobot;

		EmbeddedExeTool fr = null;

		public bool rrobot_visual = true, vrobot_visual = true;//控制unity中机械臂的可见性

		public UdpClass udp_visual, udp_remote;

		public byte[] udp_send_data = new byte[49];
		public byte[] udp_remote_send_data = new byte[30];

		private Thread robot_control_main_thread;
		private bool control_running = false;

		private bool gravity_flag = false, output_flag = false;

		double[] joint_zero = new double[6] { -3.09, -4.34, 1.56, 0, 0.10, 0 };

		public Form1()
		{
			rrobot = new Robot_UR();
			vrobot = new Robot_UR();
			//rrobot.reset_tool_param(new double[4] { 0.628796, 0, -0.087305, 0 });
			InitializeComponent();
			panel_unity.Controls.Clear();
			if (fr != null && fr.IsStarted)//如果重新啟動（即fr不為空），則關閉
			{
				fr.Stop();
			}
			string exepath = Directory.GetCurrentDirectory() + "\\UnityBuild-UR\\DMArmVisual.exe";
			fr = new EmbeddedExeTool(panel_unity, "");
			fr.Start(exepath);

			canfd = new USBCANFD();

			udp_unity_init();

			udp_remote = new UdpClass("192.168.1.200", 11452);
			udp_remote.bind();
		}

		private void button_open_device_Click(object sender, EventArgs e)
		{
			if (!try_udp_connect())
			{
				SystemSounds.Asterisk.Play();
				MessageBox.Show("UDP连接失败，稍后再试！", "错误");
				return;
			}
			if (!canfd.IsOpen)
			{
				if (!canfd.open_device())
				{
					SystemSounds.Asterisk.Play();
					MessageBox.Show("打开设备失败！", "错误");
					return;
				}
				if (!canfd.init_device())
				{
					SystemSounds.Asterisk.Play();
					MessageBox.Show("初始化设备失败！", "错误");
					return;
				}
				if (!canfd.start_device())
				{
					SystemSounds.Asterisk.Play();
					MessageBox.Show("启动设备失败！", "错误");
					return;
				}
				if (!canfd.get_status_all())
				{
					SystemSounds.Asterisk.Play();
					MessageBox.Show("读取电机状态失败！", "错误");
					return;
				}
				(sender as Control).Text = "关闭设备";
				button_disable_motor.Enabled = true;
				button_enable_motor.Enabled = true;
				button_start_sending.Enabled = true;
				button_set_MIT.Enabled = true;
				button_set_PV.Enabled = true;
				button_gravcomp_sw.Enabled = true;
				motor_set_zero_enable(true);
				rrobot.Angle = rrobot.motor2dh(canfd.motors);
				rrobot.set_robot();
			}
			else
			{
				if (!canfd.close_device())
				{
					SystemSounds.Asterisk.Play();
					MessageBox.Show("关闭设备失败！", "错误");
					return;
				}
				(sender as Control).Text = "开启设备";
				button_disable_motor.Enabled = false;
				button_enable_motor.Enabled = false;
				button_start_sending.Enabled = false;
				button_set_MIT.Enabled = false;
				button_set_PV.Enabled = false;
				button_gravcomp_sw.Enabled = false;
				motor_set_zero_enable(false);
				udp_send_timer.Stop();
			}
		}
		private void button_enable_motor_Click(object sender, EventArgs e)
		{
			canfd.enable_all();
			motor_lock_enable(true);
			clamp_open(0.5f);
		}

		private void button_disable_motor_Click(object sender, EventArgs e)
		{
			canfd.disable_all();
			//disable_all指令后发送线程已经关闭
			button_start_sending.Text = "开始发送";
			button_enable_motor.Enabled = true;
			button_open_device.Enabled = true;
			motor_lock_enable(false);
			motor_set_zero_enable(true);
		}

		private void button_start_sending_Click(object sender, EventArgs e)
		{
			if (canfd.IsUpdating == false)
			{
				(sender as Control).Text = "停止发送";
				canfd.start_can_thread(1);
				start_thread(ref robot_control_main_thread, robot_control, "robot_control_main_thread");

				button_enable_motor.Enabled = false;//为了防止残余指令，此时禁止使能、禁止关闭设备
				button_open_device.Enabled = false;
				motor_set_zero_enable(false);
			}
			else
			{
				canfd.stop_can();
				control_running = false;
				(sender as Control).Text = "开始发送";
				button_enable_motor.Enabled = true;
				button_open_device.Enabled = true;
				motor_set_zero_enable(true);
			}
		}

		private void button_set_MIT_Click(object sender, EventArgs e)
		{
			if (canfd.IsUpdating)
			{
				canfd.mode_switch_flag = 1;
			}
			else
			{
				canfd.set_mode_all(1);
			}
			foreach (Control control in groupBox_single_motor_set.Controls)
			{
				if (control.Name.Contains("lock"))
				{
					control.Text = "🔒";
				}
			}
		}

		private void button_set_PV_Click(object sender, EventArgs e)
		{
			if (canfd.IsUpdating)
			{
				canfd.mode_switch_flag = 2;
			}
			else
			{
				canfd.set_mode_all(2);
			}
			foreach (Control control in groupBox_single_motor_set.Controls)
			{
				if (control.Name.Contains("lock"))
				{
					control.Text = "🔒";
				}
			}
		}
		private void button_single_lock_Click(object sender, EventArgs e)
		{
			if (canfd.Mode == 1)
			{
				string temp = System.Text.RegularExpressions.Regex.Replace((sender as Button).Name, @"[^0-9]+", "");
				uint.TryParse(temp, out uint id);
				if (canfd.motors[id - 1].Mode == 1)//如果是MIT模式，就上锁
				{
					(sender as Button).Text = "🔐";
					canfd.motors[id - 1].set_mode(2);
				}
				if (canfd.motors[id - 1].Mode == 2)
				{
					(sender as Button).Text = "🔒";
					canfd.motors[id - 1].set_mode(1);
				}
			}
		}
		private void button_set_single_zero_Click(object sender, EventArgs e)
		{
			string temp = System.Text.RegularExpressions.Regex.Replace((sender as Button).Name, @"[^0-9]+", "");
			uint.TryParse(temp, out uint id);
			canfd.set_zero(id);
		}
		private void button_gravcomp_sw_Click(object sender, EventArgs e)
		{
			if (gravity_flag)
			{
				gravity_flag = false;
				(sender as Button).Text = "开启重力补偿";
			}
			else
			{
				gravity_flag = true;
				(sender as Button).Text = "关闭重力补偿";
			}
		}

        private void button_udp_connect_Click(object sender, EventArgs e)
        {
            udp_remote.connect("192.168.1.88", 8000);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			udp_visual.close();
			udp_remote.close();
			fr.Stop();
		}
		#region Winform定时器Tick事件
		private void param_updating_timer_Tick(object sender, EventArgs e)
		{
			switch (canfd.Mode)
			{
				case 1:
					{
						label_mode.Text = "当前模式：MIT模式";
						break;
					}
				case 2:
					{
						label_mode.Text = "当前模式：位置模式";
						break;
					}
				default:
					{
						label_mode.Text = "当前模式：错误";
						break;
					}
			}

			foreach (Control control in groupBox_motor_pos.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					if ((id >= 1) && (id <= 6))
					{
						control.Text = canfd.motors[id - 1].Position.ToString().PadRight(12, ' ');
					}
					else
					{
						control.Text = "";
					}
				}
			}
			foreach (Control control in groupBox_motor_vel.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					if ((id >= 1) && (id <= 6))
					{
						control.Text = canfd.motors[id - 1].Velocity.ToString().PadRight(12, ' ');
					}
					else
					{
						control.Text = "";
					}
				}
			}
			foreach (Control control in groupBox_motor_tor.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					if ((id >= 1) && (id <= 6))
					{
						control.Text = canfd.motors[id - 1].Torque.ToString().PadRight(12, ' ');
					}
					else
					{
						control.Text = "";
					}
				}
			}
			foreach (Control control in groupBox_motor_temp.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					if ((id >= 1) && (id <= 6))
					{
						control.Text = canfd.motors[id - 1].tem_mos.ToString().PadRight(4, ' ')
							+ canfd.motors[id - 1].tem_rotor.ToString().PadRight(4, ' ')
							+ canfd.motors[id - 1].ERRCODE + " " + canfd.motors[id - 1].ModeName;
						if ((canfd.motors[id - 1].ERRCODE != "使能") ||
							(canfd.motors[id - 1].tem_mos > 60) ||
							(canfd.motors[id - 1].tem_rotor > 60))
						{
							control.BackColor = Color.Pink;
						}
						else
						{
							control.BackColor = Color.LightGreen;
						}
					}
					else
					{
						control.Text = "";
					}
				}
			}
			foreach (Control control in groupBox_clamp.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					switch (id)
					{
						case 1:
							{
								control.Text = "位置：" + (canfd.tools[0].Position * 180f / Math.PI).ToString("#0.00").PadLeft(5, ' ') + "°";
								break;
							}
						case 2:
							{
								control.Text = "速度：" + (canfd.tools[0].Velocity).ToString("#0.00").PadLeft(5, ' ') + "rad/s";
								break;
							}
						case 3:
							{
								control.Text = "力矩：" + (canfd.tools[0].Torque).ToString("#0.00").PadLeft(5, ' ') + "Nm";
								break;
							}
						case 4:
							{
								control.Text = "夹持力：" + (Math.Max(0, -canfd.tools[0].Torque / 0.01f)).ToString("#0.00").PadLeft(5, ' ') + "N";
								break;
							}
						case 5:
							{
								control.Text = "温度：" + canfd.tools[0].tem_mos.ToString().PadLeft(5, ' ')
							+ canfd.tools[0].tem_rotor.ToString().PadLeft(5, ' ');
								if (Math.Max(canfd.tools[0].tem_mos, canfd.tools[0].tem_rotor) > 60)
								{
									control.BackColor = Color.Pink;
								}
								else
								{
									control.BackColor = DefaultBackColor;
								}
								break;
							}
						case 6:
							{
								control.Text = "状态：" + canfd.tools[0].ERRCODE + " " + canfd.tools[0].ModeName;
								if (canfd.tools[0].ERRCODE != "使能")
								{
									control.BackColor = Color.Pink;
								}
								else
								{
									control.BackColor = Color.LightGreen;
								}
								break;
							}
						default: break;
					}
				}
			}
			foreach (Control control in groupBox_DH.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					if ((id >= 1) && (id <= 6))
					{
						control.Text = (rrobot.Angle[id - 1] * 180d / Math.PI).ToString("####.00").PadRight(8) + "°";
					}
					else
					{
						control.Text = "";
					}
				}
            }

			foreach (Control control in groupBox_tool.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					if ((id >= 1) && (id <= 3))
					{
						control.Text = (rrobot.Position[id - 1] * 1000d).ToString("####.##").PadLeft(8, ' ') + "mm";
					}
					else if ((id >= 4) && (id <= 6))
					{
						control.Text = (rrobot.RPY[id - 4] * 180d/Math.PI).ToString("####.##").PadLeft(8, ' ') + "°";
					}
				}
			}

			//form_robot.sync2virtual(rrobot);
			label_can_param.Text = "系统刷新率：" + canfd.CanParam[6].ToString("#000") + "Hz";
		}
		private void udp_send_timer_Tick(object sender, EventArgs e)
		{
			for (int i = 0; i < 6; i++)
			{
				BitConverter.GetBytes((float)rrobot.Angle[i]).CopyTo(udp_send_data, i * 4);
				BitConverter.GetBytes((float)vrobot.Angle[i]).CopyTo(udp_send_data, i * 4 + 24);
				BitConverter.GetBytes(canfd.motors[i].Position).CopyTo(udp_remote_send_data, i * 4 + 1);
			}
			BitConverter.GetBytes(canfd.tools[0].Position).CopyTo(udp_remote_send_data, 25);
			udp_remote_send_data[0] = 0x0F;
			udp_remote_send_data[29] = 0x0C;
			udp_send_data[48] = 0;
			if (rrobot_visual)
			{
				udp_send_data[48] |= 0b0001;
			}
			if (vrobot_visual)
			{
				udp_send_data[48] |= 0b0010;
			}
			udp_visual.send(udp_send_data);
			udp_remote.send(udp_remote_send_data);
        }
        #endregion

        #region 其他自定义函数
		private void udp_unity_init()
		{
			string username = Environment.GetEnvironmentVariable("USERNAME");
			udp_visual = new UdpClass("127.0.0.1", 7009);
			udp_visual.try_bind();
			File.WriteAllText("C:\\Users\\" + username + "\\DMArmAPP.ini", "WinAPP:" + udp_visual.LocalPort.ToString() + "\n");
		}
		private bool try_udp_connect()
		{
			string username = Environment.GetEnvironmentVariable("USERNAME");
			string[] lines = File.ReadAllLines("C:\\Users\\" + username + "\\DMArmAPP.ini");
			while (lines.Length < 2)
			{
				try
				{
					lines = File.ReadAllLines("C:\\Users\\" + username + "\\DMArmAPP.ini");
				}
				catch (Exception e)
				{
				}
				Thread.Sleep(500);
				Console.WriteLine("Connecting unity...");
			}
			int unity_port_index;
			if (lines[0].Contains("Unity"))
			{
				unity_port_index = 0;
			}
			else
			{
				unity_port_index = 1;
			}

			lines[unity_port_index] = System.Text.RegularExpressions.Regex.Replace(lines[unity_port_index], @"[^0-9]+", "");
			int.TryParse(lines[unity_port_index], out int unity_port);

			udp_visual.connect("127.0.0.1", unity_port);
			
			//udp.start_recv_trd();
			udp_send_timer.Start();
			return true;
		}

		/// <summary>
		/// 启动一个线程
		/// </summary>
		/// <param name="trd"></param>
		/// <param name="action"></param>
		public static void start_thread(ref Thread trd, Action action, string name)
		{
			trd = new Thread(new ThreadStart(action));
			trd.Name = name;
			trd.IsBackground = true;
			trd.Start();
		}
		private void motor_set_zero_enable(bool enable)
		{
			foreach (Control control in groupBox_single_motor_set.Controls)
			{
				if ((control is Button) && (control.Name.Contains("set_zero")))
				{
					control.Enabled = enable;
				}
			}
		}

		private void clamp_open(float max_torque)
		{
			canfd.tools[0].PVT.position_set = 2.3f;
			canfd.tools[0].PVT.torque_lim = max_torque;
			canfd.tools[0].PVT.velocity_lim = 2f;
			canfd.tools[0].set();
		}



        private void clamp_close(float max_torque)
		{
			canfd.tools[0].PVT.position_set = -5f;
			canfd.tools[0].PVT.torque_lim = max_torque;
			canfd.tools[0].PVT.velocity_lim = 2f;
			canfd.tools[0].set();
		}

		private void motor_lock_enable(bool enable)
		{
			foreach (Control control in groupBox_single_motor_set.Controls)
			{
				if ((control is Button) && (control.Name.Contains("lock")))
				{
					control.Enabled = enable;
				}
			}
		}

		private float clamp_spring_torque(float motorangle)
		{
			double stiff = 0.02d;
			motorangle = (float)(motorangle * 180d / Math.PI);
			if (motorangle < 20f)
			{
				double tau = Math.Abs((20f - motorangle) * stiff);
				tau = Math.Min(tau, 0.2d);
				return (float)tau;
			}
			else
			{
				return 0;
			}
		}

		#endregion

		#region 控制线程
		private void robot_control()
		{
			control_running = true;

			while (control_running)
			{
				rrobot.Angle = rrobot.motor2dh(canfd.motors);
				rrobot.set_robot();
				for (int i = 0; i < canfd.motors.Length; i++)
				{
					canfd.motors[i].MIT.torque_set = (float)rrobot.Tau_G_Motor[i] * 1.1f;
					if (output_flag)
					{
						canfd.motors[i].MIT.torque_set = (float)(rrobot.Tau_G_Motor[i] + rrobot.Tau_Fh_Motor[i]);
					}
					else
					{
						canfd.motors[i].MIT.torque_set = (float)rrobot.Tau_G_Motor[i] * 1.1f;
					}
					if (gravity_flag)
					{
						canfd.motors[i].set();
					}
					else
					{
						canfd.motors[i].set_empty_command();
					}
				}
				USBCANFD.delayms(0.9);
			}
		}
		#endregion
	}
}

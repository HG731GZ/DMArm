using DMArmDLL;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using ZLGCAN;
using Control = System.Windows.Forms.Control;

namespace DMArmAPP
{
	public partial class Form1 : Form
	{
		public USBCANFD canfd;
		public Robot_UR vrobot, rrobot;

		EmbeddedExeTool fr = null;

		public UdpClass udp_visual, udp_remote;

		public byte[] udp_unity_send_data = new byte[49];

		private Thread robot_control_main_thread, teleop_udp_sending_thread;
		private bool control_running = false, follow_slave=false;
		private float pv_lim = 0.5f;//PV模式的速度限制

		private bool gravity_flag = false;
		private TextBox[]	motor_position_disp = new TextBox[6],
							motor_velocity_disp = new TextBox[6],
							motor_torque_disp = new TextBox[6],
							robot_dh_disp = new TextBox[6],
							motor_temp_disp = new TextBox[6],
							tool_states_disp = new TextBox[6],
							slave_dh_disp = new TextBox[6];
		
		public Form1()
		{
			rrobot = new Robot_UR();
			vrobot = new Robot_UR();
			InitializeComponent();
			get_ui_disp_control();
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

            udp_remote = new UdpClass("192.168.3.5",6005);
            if (udp_remote.bind())
            {
                Console.WriteLine("UDP Remote Success!");
            }

            udp_remote.start_recv_trd();
            start_thread(ref teleop_udp_sending_thread, udp_teleop, "Teleoperation Command Sending Thread");
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

		// 主从同步按钮事件
		private void button_sync_slave_Click(object sender, EventArgs e)
		{
			if (udp_remote.data_recv.data != null)
			{
				UdpClass.try_parse_protocol_frame(udp_remote.data_recv.data, out UdpProtocolFrame slave_data);
				for (int i = 0; i < 6; i++)
				{
					canfd.motors[i].PV.position_set = (float)(slave_data.q[i] / rrobot.ratio[i]);
					canfd.motors[i].PV.velocity_lim = pv_lim;
				}
				button_set_PV_Click(null, e);
			}
		}
        private void button_udp_connect_Click(object sender, EventArgs e)
        {
            string temp = System.Text.RegularExpressions.Regex.Replace(textBox_udp_port.Text, @"[^0-9]+", "");
            int.TryParse(temp, out int portNum);
            udp_remote.connect("192.168.3.14", portNum);
        }		
		private void button_follow_slave_Click(object sender, EventArgs e)
		{
            if (follow_slave)
			//停止跟随从端
			{
				follow_slave = false;
				for (int i = 0; i < 6; i++)
				{
					canfd.motors[i].set_empty_command();
				}
				(sender as Button).Text = "开始跟随从端";
			}
			else
			//开始跟随从端
			{
				follow_slave = true;
                (sender as Button).Text = "停止跟随从端";
			}
            button_set_PV_Click(null, e);
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
			if (udp_remote.data_recv.data != null)
			{
				UdpClass.try_parse_protocol_frame(udp_remote.data_recv.data, out UdpProtocolFrame slave_data);
				if (slave_data.q != null)
				{
					for (int i = 0; i < 6; i++)
					{
						slave_dh_disp[i].Text = (slave_data.q[i] * 180d / Math.PI).ToString("####.00").PadRight(8) + "°";
					}
					vrobot.Angle = slave_data.q;
				}
			}
            for (int i = 0; i < 6; i++)
			{
				motor_position_disp[i].Text = canfd.motors[i].Position.ToString().PadRight(12, ' ');
				motor_velocity_disp[i].Text = canfd.motors[i].Velocity.ToString().PadRight(12, ' ');
				motor_torque_disp[i].Text = canfd.motors[i].Torque.ToString().PadRight(12, ' ');

				motor_temp_disp[i].Text = canfd.motors[i].tem_mos.ToString().PadRight(4, ' ')
										+ canfd.motors[i].tem_rotor.ToString().PadRight(4, ' ')
										+ canfd.motors[i].ERRCODE + " " + canfd.motors[i].ModeName;
				if ((canfd.motors[i].ERRCODE != "使能") ||
					(canfd.motors[i].tem_mos > 60) ||
					(canfd.motors[i].tem_rotor > 60))
				{
					motor_temp_disp[i].BackColor = Color.Pink;
				}
				else
				{
					motor_temp_disp[i].BackColor = Color.LightGreen;
				}

				robot_dh_disp[i].Text = (rrobot.Angle[i] * 180d / Math.PI).ToString("####.00").PadRight(8) + "°";
			
            }
			tool_states_disp[0].Text = "位置：" + (canfd.tools[0].Position * 180f / Math.PI).ToString("#0.00").PadLeft(5, ' ') + "°";
			tool_states_disp[1].Text = "速度：" + (canfd.tools[0].Velocity).ToString("#0.00").PadLeft(5, ' ') + "rad/s";
			tool_states_disp[2].Text = "力矩：" + (canfd.tools[0].Torque).ToString("#0.00").PadLeft(5, ' ') + "Nm";
			tool_states_disp[3].Text = "夹持力：" + (Math.Max(0, -canfd.tools[0].Torque / 0.01f)).ToString("#0.00").PadLeft(5, ' ') + "N";
			tool_states_disp[4].Text = "温度：" + canfd.tools[0].tem_mos.ToString().PadLeft(5, ' ')	+ canfd.tools[0].tem_rotor.ToString().PadLeft(5, ' ');
			if (Math.Max(canfd.tools[0].tem_mos, canfd.tools[0].tem_rotor) > 60)
			{
				tool_states_disp[4].BackColor = Color.Pink;
			}
			else
			{
				tool_states_disp[4].BackColor = DefaultBackColor;
			}
			tool_states_disp[5].Text = "状态：" + canfd.tools[0].ERRCODE + " " + canfd.tools[0].ModeName;
			if (canfd.tools[0].ERRCODE != "使能")
			{
				tool_states_disp[5].BackColor = Color.Pink;
			}
			else
			{
				tool_states_disp[5].BackColor = Color.LightGreen;
			}
			label_can_param.Text = "系统刷新率：" + canfd.CanParam[6].ToString("#000") + "Hz";
		}

        private void udp_send_timer_Tick(object sender, EventArgs e)
		{
			for (int i = 0; i < 6; i++)
			{
				BitConverter.GetBytes((float)rrobot.Angle[i]).CopyTo(udp_unity_send_data, i * 4);
				BitConverter.GetBytes((float)vrobot.Angle[i]).CopyTo(udp_unity_send_data, i * 4 + 24); 
            }
			udp_unity_send_data[48] = 0b0011;
			udp_visual.send(udp_unity_send_data);
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
		/// 获取UI中用于显示参数的控件
		/// </summary>
		private void get_ui_disp_control()
		{
			foreach (Control control in groupBox_motor_pos.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					if ((id >= 1) && (id <= 6))
					{
						motor_position_disp[id - 1] = control as TextBox;
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
						motor_velocity_disp[id - 1] = control as TextBox;
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
						motor_torque_disp[id - 1] = control as TextBox;
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
						motor_temp_disp[id - 1] = control as TextBox;
					}
				}
			}
			foreach (Control control in groupBox_clamp.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					if ((id >= 1) && (id <= 6))
					{
						tool_states_disp[id - 1] = control as TextBox;
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
						robot_dh_disp[id - 1] = control as TextBox;
					}
				}
			}
			foreach (Control control in groupBox_slave.Controls)
			{
				if (control is TextBox)
				{
					string temp = System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", "");
					uint.TryParse(temp, out uint id);
					if ((id >= 1) && (id <= 6))
					{
						slave_dh_disp[id - 1] = control as TextBox;
					}
				}
			}
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

		#endregion

		#region 控制/UDP遥操作线程
		private void udp_teleop()
		{
			while (true)
			{
				if (follow_slave)
				{
					if (udp_remote.data_recv.data != null)
					{
						UdpClass.try_parse_protocol_frame(udp_remote.data_recv.data, out UdpProtocolFrame slave_data);
						if (canfd.Mode == 2)//只在位置模式下更改PV指令
						{
							for (int i = 0; i < 6; i++)
							{
								canfd.motors[i].PV.position_set = (float)(slave_data.q[i] / rrobot.ratio[i]);
								canfd.motors[i].PV.velocity_lim = pv_lim;
							}
						}
						else
						{
							for (int i = 0; i < 6; i++)
							{
								canfd.motors[i].PV.velocity_lim = 0;
							}
						}
					}
				}

				udp_remote.send_protocol_frame(rrobot.Angle, 1, new double[3] { Math.Max(0, Math.Min(0.99f, canfd.tools[0].Position)), 0, 0 });

				USBCANFD.delayms(5);
			}
		}
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
					if (i == 3)
					{
						canfd.motors[i].MIT.torque_set = (float)rrobot.Tau_G_Motor[i] * 1.2f;
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

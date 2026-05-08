using DMArmDLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZLGCAN;
using MathNet.Numerics.LinearAlgebra;

namespace DMArmAPP
{
	public partial class Form_Robot : Form
	{
		EmbeddedExeTool fr = null;
		Form1 form1 = Form1.form1;
		Robot rrobot = Form1.form1.rrobot;
		Robot vrobot = Form1.form1.vrobot;

		int a = 0;

		GroupBox[] groupBoxes;

		double[] start_q = new double[6], target_q = new double[6];

		public Thread go_path_thread = null;
		public Form_Robot()
		{
			InitializeComponent();
			groupBoxes = new GroupBox[2] { groupBox_angle_control, groupBox_tool_control };
			panel_unity.Controls.Clear();
			if (fr != null && fr.IsStarted)//如果重新啟動（即fr不為空），則關閉
			{             
				fr.Stop();
			}
			string exepath = Directory.GetCurrentDirectory() + "\\UnityBuild\\DMArmVisual.exe";
			fr = new EmbeddedExeTool(panel_unity, "");
			fr.Start(exepath);

			param_sync_robot(vrobot);
			timer1.Start();
		}

		private void robot_param_change(object sender, EventArgs e)
		{
			disable_valuechange_event(sender);
			robot_symc_param(vrobot, sender as Control);
			param_sync_robot(vrobot);
			enable_valuechange_event(sender);
		}
		private void timer1_Tick(object sender, EventArgs e)
		{
			a++;
			label1.Text = form1.udp_visual.LocalPort.ToString();
		}

		private void Form_robot_closing(object sender, FormClosingEventArgs e)
		{
			fr.Stop();
		}

		#region 按钮点击事件
		private void button_read_rrobot_Click(object sender, EventArgs e)
		{
			disable_valuechange_event(sender);
			param_sync_robot(rrobot);
			vrobot.Angle = rrobot.Angle;
			vrobot.set_robot();
			enable_valuechange_event(sender);
		}
		private void button_save_start_Click(object sender, EventArgs e)
		{
			robot_angle_symc_param(vrobot);
			foreach (Control control in tabPage_quint_q.Controls)
			{
				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
							out uint id);
					if (id >= 1 && id <= 6)
					{
						(control as NumericUpDown).Value = (decimal)(vrobot.Angle[id - 1] / Math.PI * 180d);
					}
				}
			}
			foreach (Control control in tabPage_quint_tool.Controls)
			{
				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
							out uint id);
					if (id >= 1 && id <= 3)
					{
						(control as NumericUpDown).Value = (decimal)(vrobot.Position[id - 1] * 1000d);
					}
					if (id >= 4 && id <= 6)
					{
						(control as NumericUpDown).Value = (decimal)(vrobot.RPY[id - 4] / Math.PI * 180d);
					}
				}
			}
		}
		private void button_save_target_Click(object sender, EventArgs e)
		{
			robot_angle_symc_param(vrobot);
			foreach (Control control in tabPage_quint_q.Controls)
			{
				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
							out uint id);
					if (id >= 7 && id <= 12)
					{
						(control as NumericUpDown).Value = (decimal)(vrobot.Angle[id - 7] / Math.PI * 180d);
					}
				}
			}
			foreach (Control control in tabPage_quint_tool.Controls)
			{
				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
							out uint id);
					if (id >= 7 && id <= 9)
					{
						(control as NumericUpDown).Value = (decimal)(vrobot.Position[id - 7] * 1000d);
					}
					if (id >= 10 && id <= 12)
					{
						(control as NumericUpDown).Value = (decimal)(vrobot.RPY[id - 10] / Math.PI * 180d);
					}
				}
			}
		}
		private void button_show_start_Click(object sender, EventArgs e)
		{
			double[] angle = new double[6];
			foreach (Control control in tabPage_quint_q.Controls)
			{
				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
							out uint id);
					if (id >= 1 && id <= 6)
					{
						angle[id - 1] = (double)(control as NumericUpDown).Value / 180d * Math.PI;
					}
				}
			}
			vrobot.Angle = angle;
			vrobot.set_robot();
			param_sync_robot(vrobot);
		}

		private void button_show_target_Click(object sender, EventArgs e)
		{
			double[] angle = new double[6];
			foreach (Control control in tabPage_quint_q.Controls)
			{
				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
							out uint id);
					if (id >= 7 && id <= 12)
					{
						angle[id - 7] = (double)(control as NumericUpDown).Value / 180d * Math.PI;
					}
				}
			}
			vrobot.Angle = angle;
			vrobot.set_robot();
			param_sync_robot(vrobot);
		}

		private void button_start_motion_Click(object sender, EventArgs e)
		{
			double[] start = new double[6], target = new double[6];
			foreach (Control control in tabPage_quint_q.Controls)
			{
				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
							out uint id);
					if (id >= 1 && id <= 6)
					{
						start[id - 1] = (double)(control as NumericUpDown).Value / 180d * Math.PI;
					}
					if (id >= 7 && id <= 12)
					{
						target[id - 7] = (double)(control as NumericUpDown).Value / 180d * Math.PI;
					}
				}
			}
			Matrix<double> path = MotionPlan.motion_plan_move_to(start, target, 0, 0, 0, 0, 0.001, 10);
			go_path_thread = new Thread(() => motion_plan_trd(path, 1, vrobot));
			go_path_thread.Name = "go_path_thread";
			go_path_thread.IsBackground	= true;
			go_path_thread.Start();
		}
		#endregion

		#region 其他自定义函数
		/// <summary>
		/// 将左边面板中的值设定为robot的对应值
		/// </summary>
		/// <param name="robot"></param>
		public void param_sync_robot(Robot robot)
		{
			foreach (Control control in groupBox_angle_control.Controls)
			{
				if (control.Focused)//对于当前正在调整的控件，跳过同步
				{
					continue;
				}
				if (control is TrackBar)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""), 
						out uint id);
					if ((id >= 1) && (id <= 6))
					{
						(control as TrackBar).Value = (int)(robot.Angle[id - 1] * 180d / Math.PI);
					}
				}
				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
						out uint id);
					if ((id >= 1) && (id <= 6))
					{
						(control as NumericUpDown).Value = (decimal)(robot.Angle[id - 1] * 180d / Math.PI);
					}
				}
			}
			foreach (Control control in groupBox_tool_control.Controls)
			{
				if (control.Focused)//对于当前正在调整的控件，跳过同步
				{
					continue;
				}
				if (control is TrackBar)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
						out uint id);
					if ((id >= 1) && (id <= 3))
					{
						(control as TrackBar).Value = (int)(robot.Position[id - 1] * 1000d);
					}
					if ((id >= 4) && (id <= 6))
					{
						(control as TrackBar).Value = (int)(robot.RPY[id - 4] * 180d / Math.PI);
					}
				}

				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
						out uint id);
					if ((id >= 1) && (id <= 3))
					{
						(control as NumericUpDown).Value = (decimal)(robot.Position[id - 1] * 1000d);
					}
					if ((id >= 4) && (id <= 6))
					{
						(control as NumericUpDown).Value = (decimal)(robot.RPY[id - 4] * 180d / Math.PI);
					}
				}
			}
		}
		/// <summary>
		/// 将robot中的对应值设定为左边面板中的对应值
		/// </summary>
		/// <param name="robot"></param>
		/// <param name="control">当前所调整的控件</param>
		public void robot_symc_param(Robot robot, Control control)
		{
			if (control.Parent == groupBox_angle_control)//关节控制
			{
				double[] angle = robot.Angle;
				uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
						out uint id);
				if (control is TrackBar)
				{
					angle[id - 1] = (control as TrackBar).Value / 180d * Math.PI;
				}
				if (control is NumericUpDown)
				{
					angle[id - 1] = (double)(control as NumericUpDown).Value / 180d * Math.PI;
				}
				robot.Angle = angle;
				robot.set_robot();
			}
			if (control.Parent == groupBox_tool_control)//末端控制
			{
				uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
						out uint id);
				double[] pos = robot.Position;
				double[] rpy = robot.RPY;
				if (id >= 1 && id <= 3)//位置变化
				{
					if (control is TrackBar)
					{
						pos[id - 1] = (control as TrackBar).Value / 1000d;

					}
					if (control is NumericUpDown)
					{
						pos[id - 1] = (double)(control as NumericUpDown).Value / 1000d;
					}
				}
				robot.Position = pos;
				if (id >= 4 && id <= 6)//欧拉角变化
				{
					if (control is TrackBar)
					{
						rpy[id - 4] = (control as TrackBar).Value / 180d * Math.PI;
					}
					if (control is NumericUpDown)
					{
						rpy[id - 4] = (double)(control as NumericUpDown).Value / 180d * Math.PI;
					}
				}
				robot.RPY = rpy;
				if (!robot.set_robot())
				{
					MessageBox.Show("无解！");
				}
			}
		}
		public void robot_angle_symc_param(Robot robot)
		{
			double[] angle = robot.Angle;
			foreach (Control control in groupBox_angle_control.Controls)
			{
				if (control is NumericUpDown)
				{
					uint.TryParse(System.Text.RegularExpressions.Regex.Replace(control.Name, @"[^0-9]+", ""),
							out uint id);
					if (id >= 1 && id <= 6)
					{
						angle[id - 1] = (double)(control as NumericUpDown).Value / 180d * Math.PI;
					}
				}
			}
			robot.Angle = angle;
			robot.set_robot();
		}
		/// <summary>
		/// 关闭左边【关节控制】和【末端控制】的滑块与数字框对数字变化的响应事件
		/// </summary>
		/// <param name="sender">sender以外控件的响应事件会被关闭</param>
		private void disable_valuechange_event(object sender)
		{
			foreach (GroupBox groupBox in groupBoxes)
			{
				foreach (Control control in groupBox.Controls)
				{
					if (sender != control)
					{
						if (control is TrackBar)
						{
							(control as TrackBar).ValueChanged -= robot_param_change;
						}
						if (control is NumericUpDown)
						{
							(control as NumericUpDown).ValueChanged -= robot_param_change;
						}
					}
				}
			}
		}

		/// <summary>
		/// 开启左边【关节控制】和【末端控制】的滑块与数字框对数字变化的响应事件
		/// </summary>
		/// <param name="sender">sender以外控件的响应事件会被开启</param>
		private void enable_valuechange_event(object sender)
		{
			foreach (GroupBox groupBox in groupBoxes)
			{
				foreach (Control control in groupBox.Controls)
				{
					if (sender != control)
					{
						if (control is TrackBar)
						{
							(control as TrackBar).ValueChanged += robot_param_change;
						}
						if (control is NumericUpDown)
						{
							(control as NumericUpDown).ValueChanged += robot_param_change;
						}
					}
				}
			}
		}

		private void checkBox_vrobot_CheckedChanged(object sender, EventArgs e)
		{
			form1.vrobot_visual = checkBox_vrobot.Checked;
		}

		private void checkBox_rrobot_CheckedChanged(object sender, EventArgs e)
		{
			form1.rrobot_visual = checkBox_rrobot.Checked;
		}

		private void button_go_line_Click(object sender, EventArgs e)
		{

		}

		private void button_quint_tool_go_Click(object sender, EventArgs e)
		{

		}

		#endregion

		#region 轨迹规划线程
		private void motion_plan_trd(Matrix<double> path, double interv, Robot robot)
		{
			for (int i = 0; i < path.RowCount; i++)
			{
				robot.Angle = path.Row(i).ToArray();
				robot.set_robot();
				USBCANFD.delayms(0.99);
			}
		}
		#endregion

	}
}

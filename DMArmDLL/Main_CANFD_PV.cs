using MathNet.Numerics.LinearAlgebra;
using RobotControl;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMArmDLL
{
	internal class Main_CANFD_PV
	{
		static void Main(string[] args)
		{
			//StreamWriter logfile = new StreamWriter("D:\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt");
			USBCANFD CAN = new USBCANFD();
			Robot robot_v = new Robot();
			Robot robot_r = new Robot();
			UdpClass udp = new UdpClass("127.0.0.1", 11451);
			udp.bind();
			udp.connect("127.0.0.1", 11452);
			Joystick joystick = new Joystick();
			joystick.connect();

			
			CAN.open_device();
			CAN.init_device();
			CAN.start_device();
			Thread.Sleep(200);
			CAN.enable_all();
			CAN.start_can_thread(1);
			double[] q1 = new double[6] { -90, -90, 180, 0, 80, 90 };
			double[] q2 = new double[6] { -90, -90, 180, 0, 80, 90 };
			//double[] q2 = new double[6] { -90, 180, -95, 0, 0, 0 };
			double[] q3 = new double[6] { -90, 180, -95, 0, 93, 0 };
			for (int i = 0; i < 6; i++)
			{
				q1[i] = q1[i] * Math.PI / 180;
				q2[i] = q2[i] * Math.PI / 180;
				q3[i] = q3[i] * Math.PI / 180;
			}

			float[] motor_angle; bool[] in_range;
			if (robot_v.dh2motor(CAN.motors, q1, out motor_angle, out in_range))
			{
				Console.WriteLine("Clear!");
				for (int j = 0; j < 6; j++)
				{
					CAN.motors[j].PV.position_set = motor_angle[j];
					CAN.motors[j].PV.velocity_lim = 0.5f;
					CAN.motors[j].set();
				}
			}
			robot_v.Angle = q1;
			robot_v.set_robot();

			while (true)
			{
				double[] dh = robot_r.motor2dh(CAN.motors);
				robot_r.Angle = dh;
				Console.WriteLine((dh[0] * 180f / Math.PI).ToString("0.00").PadLeft(10)
					+ (dh[1] * 180f / Math.PI).ToString("0.00").PadLeft(10)
					+ (dh[2] * 180f / Math.PI).ToString("0.00").PadLeft(10)
					+ (dh[3] * 180f / Math.PI).ToString("0.00").PadLeft(10)
					+ (dh[4] * 180f / Math.PI).ToString("0.00").PadLeft(10)
					+ (dh[5] * 180f / Math.PI).ToString("0.00").PadLeft(10)
					//+ (CAN.motors[6].position * 180f / Math.PI).ToString("0.00").PadLeft(10)
					+ "  系统刷新率：" + CAN.CanParam[6].ToString(".0").PadRight(8));
				//Console.WriteLine(CAN.motors[0].recv_num.ToString().PadLeft(10)
				//	+ CAN.motors[1].recv_num.ToString().PadLeft(10)
				//	+ CAN.motors[2].recv_num.ToString().PadLeft(10)
				//	+ CAN.motors[3].recv_num.ToString().PadLeft(10)
				//	+ CAN.motors[4].recv_num.ToString().PadLeft(10)
				//	+ CAN.motors[5].recv_num.ToString().PadLeft(10)
				//	//+ CAN.motors[6].recv_num.ToString().PadLeft(10)
				//	+ "  频率：" + CAN.CanParam[0].ToString(".0").PadRight(8)
				//	+ "  负载率：" + CAN.CanParam[1].ToString(".00").PadRight(8)
				//	+ "  发送成功：" + CAN.CanParam[2].ToString().PadRight(8)
				//	+ "  发送失败：" + CAN.CanParam[3].ToString().PadRight(8)
				//	);

				byte[] data=new byte[49];
				for (int i = 0; i < dh.Length; i++)
				{
					BitConverter.GetBytes((float)dh[i]).CopyTo(data, i * 4);
					BitConverter.GetBytes((float)dh[i]).CopyTo(data, i * 4 + 24);
				}
				data[48] = 0b0010;

				udp.send(data);
				if (joystick.key.LeftShoulder)
				{
					robot_v.move_world(-(double)(joystick.left_x) / (Math.Pow(2, 26)), 'x');
					robot_v.move_world(-(double)(joystick.left_y) / (Math.Pow(2, 26)), 'y');
					robot_v.move_world((double)(joystick.right_y) / (Math.Pow(2, 26)), 'z');

					if (robot_v.dh2motor(CAN.motors, robot_v.Angle, out motor_angle, out in_range))
					{
						Console.WriteLine((double)(joystick.right_y) / (Math.Pow(2, 26)));
						for (int j = 0; j < 6; j++)
						{
							CAN.motors[j].PV.position_set = motor_angle[j];
							CAN.motors[j].PV.velocity_lim = 5f;
							CAN.motors[j].set();
						}
					}
					else
					{
						robot_v.Angle = robot_v.motor2dh(CAN.motors);
						robot_v.set_robot();
					}
				}

				if (joystick.key.B)
				{
					robot_v.Angle = q1;
					robot_v.set_robot();
					if (robot_v.dh2motor(CAN.motors, q1, out motor_angle, out in_range))
					{
						for (int j = 0; j < 6; j++)
						{
							CAN.motors[j].PV.position_set = motor_angle[j];
							CAN.motors[j].PV.velocity_lim = 0.5f;
							CAN.motors[j].set();
						}
					}

				}
				if (joystick.key.A)
				{
					Matrix<double> Q = MotionPlan.motion_plan_move_to(robot_v.Angle, q2, 0, 0, 0, 0, 0.001, 5);
					Thread trd = new Thread(() => motion_plan_thread(robot_v, Q, CAN));
					trd.IsBackground = true;
					trd.Start();
					trd.Join();
				}
				USBCANFD.delayms(10);
			}
		}
		static void motion_plan_thread(Robot robot_v, Matrix<double> Q, USBCANFD CAN)
		{
			float[] motor_angle; bool[] in_range;
			for (int i = 0; i < Q.RowCount; i++)
			{
				robot_v.Angle = Q.Row(i).ToArray();
				robot_v.set_robot();				
				if (robot_v.dh2motor(CAN.motors, Q.Row(i).ToArray(), out motor_angle, out in_range))
				{
					Console.WriteLine("Clear!");
					for (int j = 0; j < 6; j++)
					{
						CAN.motors[j].PV.position_set = motor_angle[j];
						CAN.motors[j].PV.velocity_lim = 0.5f;
						CAN.motors[j].set();
					}
				}
				USBCANFD.delayms(1);
			}
		}
	}
}

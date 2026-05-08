using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

//在USBCANFD中用List表示电机是否有可行性？
//Slave做基类，DMMotor和LKMotor继承？--感觉没有太大必要
//想办法通过MasterID和SlaveID定位电机--这个比较重要
namespace DMArmDLL
{
	internal class Main_ModeSWTest
	{
		static void Main(string[] args)
		{
			USBCANFD CAN = new USBCANFD();
			Robot robot_v = new Robot();
			Robot robot_r = new Robot();
			double[] q1 = new double[6] { -90, 165, -100, 0, 20, 0 };
			for (int i = 0; i < 6; i++)
			{
				q1[i] = q1[i] * Math.PI / 180d;
			}
			CAN.open_device();
			CAN.init_device();
			CAN.start_device();
			CAN.disable_all();
			////CAN.get_mode_all();
			//Console.WriteLine(CAN.get_status_all());
			//Console.ReadLine();
			CAN.enable_all();
			CAN.start_can_thread(1);

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
			Console.WriteLine(CAN.motors[2].mode);
			Console.ReadLine();
			double[] dh = robot_r.motor2dh(CAN.motors);
			robot_r.Angle = dh;
			robot_r.set_robot();

			for (int i = 0; i < 6; i++)
			{
				CAN.motors[i].set_empty_command();
			}
			CAN.mode_switch_flag = 1;
			Console.WriteLine("切换到MIT！！！");

			//Thread.Sleep(2000);
			//CAN.motors[2].set_empty_command_PV();
			//CAN.motors[2].set_empty_command_MIT();
			//CAN.mode_switch_flag = 2;
			//Console.WriteLine("切换到PV！！！");
			//Thread.Sleep(1000);
			//Console.WriteLine(CAN.motors[2].current_mode);
			while (true)
			{
				robot_r.Angle = robot_r.motor2dh(CAN.motors);
				robot_r.set_robot();
				CAN.motors[4].MIT.torque_set = (float)robot_r.Tau_G_Motor[4] * 1.0f;
				CAN.motors[4].set();
				CAN.motors[3].MIT.torque_set = (float)robot_r.Tau_G_Motor[3] * 1.0f;
				CAN.motors[3].set();
				CAN.motors[2].MIT.torque_set = (float)robot_r.Tau_G_Motor[2] * 1.1f;
				CAN.motors[2].set();
				CAN.motors[1].MIT.torque_set = (float)robot_r.Tau_G_Motor[1] * 1.1f;
				CAN.motors[1].set();
				Console.WriteLine(robot_r.Tau_G_Motor[3].ToString("0.00").PadLeft(10)
					+ CAN.motors[3].Torque.ToString("0.00").PadLeft(10)
					+ robot_r.Tau_G_Motor[4].ToString("0.00").PadLeft(10)
					+ CAN.motors[4].Torque.ToString("0.00").PadLeft(10)
					+ robot_r.Tau_G_Motor[5].ToString("0.00").PadLeft(10)
					+ CAN.motors[5].Torque.ToString("0.00").PadLeft(10)
					//+ CAN.motors[6].recv_num.ToString().PadLeft(10)
					+ "  频率：" + CAN.CanParam[0].ToString(".0").PadRight(8)
					);
			}
		}
	}
}

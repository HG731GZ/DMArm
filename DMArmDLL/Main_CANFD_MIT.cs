using RobotControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMArmDLL
{
	internal class Main_CANFD_MIT
	{
		static void Main(string[] args)
		{
			//StreamWriter logfile = new StreamWriter("D:\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt");
			USBCANFD CAN = new USBCANFD();
			Robot robot = new Robot();
			//TCP_Server server = new TCP_Server("127.0.0.1", 11451);
			Joystick joystick = new Joystick();
			joystick.connect();
			//server.start_tcp_server();
			CAN.open_device();
			CAN.init_device();
			CAN.start_device();
			CAN.enable_all();
			for (int i = 0; i < 6; i++)
			{
				CAN.motors[i].set_empty_command_MIT();
			}
			CAN.start_can_thread(1);
			while (true)
			{
				robot.Angle = robot.motor2dh(CAN.motors);
				robot.set_robot();
				Console.WriteLine(CAN.CanFps);
				CAN.motors[4].torque_set = (float)robot.Tau_G_Motor[4];
				CAN.motors[4].set_MIT();
				CAN.motors[3].torque_set = (float)robot.Tau_G_Motor[3];
				CAN.motors[3].set_MIT();
				CAN.motors[2].torque_set = (float)robot.Tau_G_Motor[2] * 1.1f;
				CAN.motors[2].set_MIT();
				CAN.motors[1].torque_set = (float)robot.Tau_G_Motor[1] * 1.1f;
				CAN.motors[1].set_MIT();
			}
		}
	}
}

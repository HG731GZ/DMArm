using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLGCAN;

namespace DMArmDLL
{
	internal class Main_ToolTest
	{
		static void Main(string[] args)
		{
			USBCANFD CAN = new USBCANFD();
			CAN.open_device();
			CAN.init_device();
			CAN.start_device();
			CAN.enable_all();
			CAN.start_can_thread(1);
			CAN.tools[0].PVT.position_set = 2.3f;
			CAN.tools[0].PVT.velocity_lim = 2f;
			CAN.tools[0].PVT.torque_lim = 1f;
			CAN.tools[0].set();
			if (CAN.IsOpen)
			{
				Console.WriteLine("输入：");
			}
			while (true) 
			{

				string str = Console.ReadLine();
				if (str == "1")
				{
					open(CAN);
				}
				if (str == "0")
				{
					close(CAN);
				}
				CAN.tools[0].set();
				Console.WriteLine(CAN.tools[0].Torque);
			}
		}
		static void open(USBCANFD CAN)
		{
			CAN.tools[0].PVT.position_set = 2.3f;
		}
		static void close(USBCANFD CAN)
		{
			CAN.tools[0].PVT.position_set = -5f;
		}
	}
}

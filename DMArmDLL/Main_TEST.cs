using RobotControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMArmDLL
{
	internal class Main_TEST
	{
		static void Main(string[] args)
		{
			double[] q1 = new double[6] { -90, -90, 220, 0, 20, 0 };
			double[] q2 = new double[6] { 0, -120, 120, 0, 50, 0 };
			Robot robot = new Robot();
			USBCANFD CAN = new USBCANFD();
			for (int i = 0; i < 6; i++)
			{
				q1[i] = q1[i] * Math.PI / 180;
				q2[i] = q2[i] * Math.PI / 180;
			}
			var M = MotionPlan.motion_plan_move_to(q1, q2, 0, 0, 0, 0, 0.01, 1);
			var M1 = robot.dh2motor(CAN.motors, M);
			for (int i=0;i<M1.RowCount;i++)
			{
				for (int j=0;j<M1.ColumnCount;j++)
				{
					Console.Write((M1[i, j]*180/Math.PI).ToString("##0.0").PadLeft(6));
				}
				Console.WriteLine();
			}
			Console.ReadLine();
		}
	}
}

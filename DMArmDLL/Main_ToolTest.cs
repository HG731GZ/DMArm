using MathNet.Numerics.LinearAlgebra;
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
			var urMaster = new Robot_UR();
			double[] q = new double[6] { 0.1745, 0.3491, 0.5236, 0.6981, 0.8727, 1.0472 };
			urMaster.Angle = q;
			urMaster.set_robot();
			var T = Matrix<double>.Build.DenseOfArray(urMaster.TransMatrix);

			Console.WriteLine(T.ToString());
		}
	}
}

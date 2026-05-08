using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMArmDLL
{
	public class MotionPlan
	{
		/// <summary>
		/// 关节角按照五次多项式插值前往特定关节角度
		/// </summary>
		/// <param name="Q0"></param>
		/// <param name="Q1"></param>
		/// <param name="w0"></param>
		/// <param name="w1"></param>
		/// <param name="a0"></param>
		/// <param name="a1"></param>
		/// <param name="deltaT"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Matrix<double> motion_plan_move_to(double[] Q0, double[] Q1, double w0, double w1, double a0, double a1, double deltaT, double t)
		{
			double[,] a = new double[6, 6];//五次多项式系数
			double[] deltaQ = Robot.minor_arc_dir(Q0, Q1);
			for (int i = 0; i < 6; i++)
			{

				a[i, 0] = Q0[i];
				a[i, 1] = w0;
				a[i, 2] = a0 / 2;
				a[i, 3] = (20 * (deltaQ[i]) - (8 * w1 + 12 * w0) * t - (3 * a0 - a1) * t * t) / (2 * t * t * t);
				a[i, 4] = (30 * (-deltaQ[i]) + (14 * w1 + 16 * w0) * t + (3 * a0 - 2 * a1) * t * t) / (2 * t * t * t * t);
				a[i, 5] = (12 * (deltaQ[i]) - (6 * w1 + 6 * w0) * t - (a0 - a1) * t * t) / (2 * t * t * t * t * t);

			}
			int Step = (int)(t / deltaT);
			Matrix<double> Q = Matrix<double>.Build.Dense(6, Step);
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < Step; j++)
				{
					for (int k = 0; k < 6; k++)
					{
						Q[i, j] = Robot.angle_clip_pnpi(Q[i, j] + a[i, k] * Math.Pow(j * deltaT, k));
					}
				}
			}
			return Q.Transpose();
		}
	}
}

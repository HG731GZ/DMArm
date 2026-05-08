using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMArmDLL
{
	public class RealtimeKalman1D
	{
		private Vector<double> x;         // 状态向量 [position; velocity; alpha]
		private Matrix<double> P;         // 协方差矩阵
		private readonly double R_position;
		private readonly double R_velocity;
		private readonly double Q_alpha_base;

		public RealtimeKalman1D(double initialPosition, double initialVelocity, double qAlphaBase = 12000, double rPosition = 1e-4, double rVelocity = 1e-3)
		{
			x = Vector<double>.Build.DenseOfArray(new double[] { initialPosition, initialVelocity, 0.0 });
			P = Matrix<double>.Build.DenseIdentity(3);
			Q_alpha_base = qAlphaBase;
			R_position = rPosition;
			R_velocity = rVelocity;
		}

		/// <summary>
		/// 实时更新卡尔曼滤波器，输入测量角度、角速度、时间间隔，返回平滑估计结果
		/// </summary>
		public (double position, double velocity, double alpha) Update(double positionMeas, double velocityMeas, double dt)
		{
			// 状态转移矩阵 A
			var A = Matrix<double>.Build.DenseOfArray(new double[,]
			{
			{ 1, dt, 0.5 * dt * dt },
			{ 0, 1, dt },
			{ 0, 0, 1 }
			});

			// 过程噪声 Q
			var Q = Matrix<double>.Build.DenseOfDiagonalArray(new double[3] 
			{ 1e-6 , 1e-5 , Q_alpha_base * dt * dt });

			// 观测矩阵 H（观测 position 和 velocity）
			var H = Matrix<double>.Build.DenseOfArray(new double[,]
			{
			{ 1, 0, 0 },
			{ 0, 1, 0 }
			});

			// 观测向量 z
			var z = Vector<double>.Build.DenseOfArray(new double[] { positionMeas, velocityMeas });

			// 测量噪声协方差 R
			var R = Matrix<double>.Build.DenseOfDiagonalArray(new double[] { R_position, R_velocity });

			// ---------- 预测 ----------
			var x_pred = A * x;
			var P_pred = A * P * A.Transpose() + Q;

			// ---------- 更新 ----------
			var S = H * P_pred * H.Transpose() + R;
			var K = P_pred * H.Transpose() * S.Inverse();

			var y = z - H * x_pred;
			x = x_pred + K * y;
			P = (Matrix<double>.Build.DenseIdentity(3) - K * H) * P_pred;

			return (x[0], x[1], x[2]);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Win32.SafeHandles;

/*
    *   添加UR主端支持，还未实机验证
 */
namespace DMArmDLL
{
    /// <summary>
    /// UR同构主端机械臂
    /// </summary>
    public class Robot_UR
    {
        public double[] ratio = new double[6] { 1, 1, -1, 1, -1, 1 };
        public double[] zero_offset = new double[6] { -1.57079632679490, 2.86784648706074, -1.58889015515032, 0, 0.127707486697677, 0 };

        public double[] Fh = new double[6];

        //private double a1, a2, a3, d1, d4, d6;
        private double a2 = -0.210, a3 = -0.200, d4 = 0.062, d5 = 0.073, d6 = 0.036;//主端参数

		private double[] m;
        //private double[] x, y, z;

        private double[]
            pos = new double[3],
            q = new double[6],
            rpy = new double[3],
            c = new double[6],
            s = new double[6];

        private double[,]
            trans = new double[4, 4],
            rot = new double[3, 3],
            jacob0 = new double[6, 6],
            jacob6 = new double[6, 6];
        private Matrix<double> T10, T21, T32, T43, T54, T65,
                               T20, T30, T40, T50, T60,
                               T61, T62, T63, T64,
                               R10, R21, R32, R43, R54, R65,
                               R20, R30, R40, R50, R60,
                               R01, R02, R03, R04, R05, R06,
                               Ja0, Ja6;
        private Vector<double> P10, P21, P32, P43, P54, P65,
                               P61, P62, P63, P64, P66,
                               Z1, Z2, Z3, Z4, Z5, Z6,
                               J1, J2, J3, J4, J5, J6,
                               r1, r2, r3, r4, r5, r6,
                               G1, G2, G3, G4, G5, G6,
                               F1, F2, F3, F4, F5, F6,
                               M1, M2, M3, M4, M5, M6;
        private Vector<double> g0 = Vector<double>.Build.DenseOfArray(new double[3] { 0, 0, -9.80665 });
        

        private double[] tau_g = new double[6];
        private double[] tau_fh = new double[6];
        private double[] tau = new double[6];

        private bool safe = false;
		/// <summary>
		/// 需要注意set的时候value必须是1x6double数组，不能单独修改其中某值
		/// </summary>
		public double[] Angle
        {
            get
            {
                double[] temp = new double[q.Length];
                q.CopyTo(temp, 0);
                return temp;
            }
            set
            {
                if (value.Length == 6)
                {
                    angle_clip_pnpi(value).CopyTo(q, 0);
                    safe = true;
                }
                else
                {
                    safe = false;
                }
            }
        }
		/// <summary>
		/// 需要注意set的时候value必须是1x3double数组，不能单独修改其中某值
		/// </summary>
		public double[] Position
        {
            get
            {
				double[] temp = new double[pos.Length];
                pos.CopyTo(temp, 0);
				return temp;
            }
        }
		/// <summary>
		/// 需要注意set的时候value必须是1x3double数组，不能单独修改其中某值
		/// </summary>
		public double[] RPY
        {
            get 
            {
				double[] temp = new double[rpy.Length];
                rpy.CopyTo(temp, 0);
				return temp; 
            }
        }
		/// <summary>
		/// 需要注意set的时候value必须是4x4double数组，不能单独修改其中某值
		/// </summary>
		public double[,] TransMatrix
        {
            get 
            {
				double[,] temp = new double[4, 4];
				Array.Copy(trans, temp, trans.Length);
				return temp;
			}
        }
        /// <summary>
        /// 需要注意set的时候value必须是3x3double数组，不能单独修改其中某值
        /// </summary>
        public double[,] RotMatrix
        {
            get 
            {
				double[,] temp = new double[3, 3];
				Array.Copy(rot, temp, rot.Length);
				return temp;
			}
        }
        public double[,] Jacob0
        {
            get
            {
                double[,] temp = new double[6, 6];
                Array.Copy(jacob0, temp, jacob0.Length);
                return temp; 
            }
        }
        public double[,] Jacob6
        {
            get
			{
				double[,] temp = new double[6, 6];
				Array.Copy(jacob6, temp, jacob6.Length);
				return temp;
			}
        }
        /// <summary>
        /// 各关节的重力补偿力矩
        /// </summary>
        public double[] Tau_G
        {
            get
            {
                double[] temp = new double[6];
                tau_g.CopyTo(temp, 0);
                return temp; 
            }
        }
        public double[] Tau_Fh
        {
            get 
            {
				double[] temp = new double[6];
				tau_fh.CopyTo(temp, 0);
				return temp;
			}
        }
        public double[] Tau
        {
            get 
            {
				double[] temp = new double[6];
				tau.CopyTo(temp, 0);
				return temp;
			}
        }
        public double[] Tau_G_Motor
        {
            get
            {
                double[] tau_g_motor = new double[6];
                for (int i = 0; i < 6; i++)
                {
                    tau_g_motor[i] = tau_g[i] / ratio[i];
                }
                return tau_g_motor;
            }
        }
        public double[] Tau_Fh_Motor
        {
            get
            {
                double[] tau_fh_motor = new double[6];
                for (int i = 0; i < 6; i++)
                {
                    tau_fh_motor[i] = tau_fh[i] / ratio[i];
                }
                return tau_fh_motor;
            }
        }
		public double[] G_Tool
		{
			get
			{
				return (R06 * g0).ToArray();
			}
		}//末端坐标系中，重力向量
		public Robot_UR()
        {
            q = new double[6];

            m = new double[6];
            //x = new double[6];
            //y = new double[6];
            //z = new double[6];

            double[][] mr = new double[6][];

            //-------达妙臂UR版重量参数(带示教器)----------
            mr[0] = new double[4] { 0.548728, 0, -0.002649,  -0.003134 };
            mr[1] = new double[4] { 0.750278, -0.164665, 0, 0.061074 };
            mr[2] = new double[4] { 0.653139, -0.151553, 0, -0.002837 };
            mr[3] = new double[4] { 0.431678, 0, -0.004718, -0.001152 };
            mr[4] = new double[4] { 0.431678, 0, 0.004718, -0.001152 };
            mr[5] = new double[4] { 0.277642,  -0.000290, 0.009891, 0.037114 };
			//-------达妙臂UR版重量参数(带示教器)----------

			for (int i = 0; i < 6; i++)
            {
                m[i] = mr[i][0];
                //x[i] = mr[i][1];
                //y[i] = mr[i][2];
                //z[i] = mr[i][3];
            }

            r1 = Vector<double>.Build.DenseOfArray(mr[0]).SubVector(1, 3);
            r2 = Vector<double>.Build.DenseOfArray(mr[1]).SubVector(1, 3);
            r3 = Vector<double>.Build.DenseOfArray(mr[2]).SubVector(1, 3);
            r4 = Vector<double>.Build.DenseOfArray(mr[3]).SubVector(1, 3);
            r5 = Vector<double>.Build.DenseOfArray(mr[4]).SubVector(1, 3);
            r6 = Vector<double>.Build.DenseOfArray(mr[5]).SubVector(1, 3);

            safe = true;
            set_robot();

        }

		/// <summary>
		/// 根据电机位置、零位偏置与减速比，将电机角度转换为DH角度
		/// </summary>
		/// <param name="motors">限定为6个元素的Motor数组</param>
		/// <returns></returns>
		public double[] motor2dh(DMMotor[] motors)
        {
            double[] dh = new double[6];
            for (int i = 0; i < 6; i++)
            {
                dh[i] = angle_clip_pnpi(motors[i].Position / ratio[i] + zero_offset[i]);
            }
            return dh;
        }
       
        /// <summary>
        /// 根据给定的角度/末端位姿等信息，计算机器人的位置、重力补偿力矩、外力输出力矩等参数。
        /// </summary>
        /// <returns>返回是否有解</returns>
        public bool set_robot()
        {
            if (!safe)
            {
                Console.WriteLine("无解！");
                return false;
            }//如果无解，就保持当前状态，直接返回
            for (int i = 0; i < 6; i++)
            {
                s[i] = Math.Sin(q[i]);
                c[i] = Math.Cos(q[i]);
            }
            T10 = Matrix<double>.Build.DenseOfArray(
                new double[4, 4]{
                {c[0],  -s[0],  0,  0},
                {s[0],  c[0],   0,  0},
                {0,     0,      1,  0},
                {0,     0,      0,  1 }});
            T21 = Matrix<double>.Build.DenseOfArray(
                new double[4, 4]{
                {c[1],  -s[1],  0,  0},
                {0,      0,    -1,  0},
                {s[1],   c[1],  0,  0 },
                {0,      0,     0,  1 }});
            T32 = Matrix<double>.Build.DenseOfArray(
                new double[4, 4]{
                {c[2],  -s[2],  0,  a2},
                {s[2],   c[2],  0,  0},
                {0,      0,     1,  0 },
                {0,      0,     0,  1 }});
            T43 = Matrix<double>.Build.DenseOfArray(
                new double[4, 4]{
                {c[3],  -s[3],  0,  a3},
                {s[3],   c[3],  0,  0 },
                {0,      0,     1,  d4 },
                {0,      0,     0,  1 }});
            T54 = Matrix<double>.Build.DenseOfArray(
                new double[4, 4]{
                {c[4],  -s[4],  0,  0},
                {0,      0,    -1, -d5},
                {s[4],   c[4],  0,  0 },
                {0,      0,     0,  1 }});
            T65 = Matrix<double>.Build.DenseOfArray(
                 new double[4, 4]{
                {c[5],  -s[5],  0,  0},
                {0,      0,     1,  d6 },
                {-s[5], -c[5],  0,  0 },
                {0,      0,     0,  1 }});

            T20 = T10 * T21;
            T30 = T20 * T32;
            T40 = T30 * T43;
            T50 = T40 * T54;
            T60 = T50 * T65;

            T61 = T21 * T32 * T43 * T54 * T65;
            T62 = T32 * T43 * T54 * T65;
            T63 = T43 * T54 * T65;
            T64 = T54 * T65;

            trans = T60.ToArray();
            pos = trans2pos(trans);
            rot = trans2rot(trans);
            rpy = rot2rpy(trans);

            R10 = T10.SubMatrix(0, 3, 0, 3);
            R21 = T21.SubMatrix(0, 3, 0, 3);
            R32 = T32.SubMatrix(0, 3, 0, 3);
            R43 = T43.SubMatrix(0, 3, 0, 3);
            R54 = T54.SubMatrix(0, 3, 0, 3);
            R65 = T65.SubMatrix(0, 3, 0, 3);

            R20 = T20.SubMatrix(0, 3, 0, 3);
            R30 = T30.SubMatrix(0, 3, 0, 3);
            R40 = T40.SubMatrix(0, 3, 0, 3);
            R50 = T50.SubMatrix(0, 3, 0, 3);
            R60 = T60.SubMatrix(0, 3, 0, 3);

            R01 = R10.Transpose();
            R02 = R20.Transpose();
            R03 = R30.Transpose();
            R04 = R40.Transpose();
            R05 = R50.Transpose();
            R06 = R60.Transpose();

            P10 = T10.Column(3).SubVector(0, 3);
            P21 = T21.Column(3).SubVector(0, 3);
            P32 = T32.Column(3).SubVector(0, 3);
            P43 = T43.Column(3).SubVector(0, 3);
            P54 = T54.Column(3).SubVector(0, 3);
            P65 = T65.Column(3).SubVector(0, 3);

            Z1 = T10.Column(2).SubVector(0, 3);
            Z2 = T20.Column(2).SubVector(0, 3);
            Z3 = T30.Column(2).SubVector(0, 3);
            Z4 = T40.Column(2).SubVector(0, 3);
            Z5 = T50.Column(2).SubVector(0, 3);
            Z6 = T60.Column(2).SubVector(0, 3);

            P61 = T61.Column(3).SubVector(0, 3);
            P62 = T62.Column(3).SubVector(0, 3);
            P63 = T63.Column(3).SubVector(0, 3);
            P64 = T64.Column(3).SubVector(0, 3);
            P66 = Vector<double>.Build.DenseOfArray(new double[3] { 0, 0, 0 });

            J1 = Append(cross(Z1, R10 * P61), Z1);
            J2 = Append(cross(Z2, R20 * P62), Z2);
            J3 = Append(cross(Z3, R30 * P63), Z3);
            J4 = Append(cross(Z4, R40 * P64), Z4);
            J5 = Append(cross(Z5, R50 * P65), Z5);
            J6 = Append(cross(Z6, R60 * P66), Z6);

            Ja0 = Matrix<double>.Build.DenseOfColumnVectors(new Vector<double>[6] { J1, J2, J3, J4, J5, J6 });
            Ja6 = ((R06.Append(Matrix<double>.Build.DenseOfArray(new double[3, 3]) * 0)).Stack
                ((Matrix<double>.Build.DenseOfArray(new double[3, 3]) * 0).Append(R06)) * Ja0);

            jacob0 = Ja0.ToArray();
            jacob6 = Ja6.ToArray();

            G1 = R01 * g0 * m[0];
            G2 = R02 * g0 * m[1];
            G3 = R03 * g0 * m[2];
            G4 = R04 * g0 * m[3];
            G5 = R05 * g0 * m[4];
            G6 = R06 * g0 * m[5];

            F6 = -G6;
            F5 = R65 * F6 - G5;
            F4 = R54 * F5 - G4;
            F3 = R43 * F4 - G3;
            F2 = R32 * F3 - G2;
            F1 = R21 * F2 - G1;

            M6 = -cross(r6, G6);
            M5 = R65 * M6 + cross(P65, (R65 * F6)) - cross(r5, G5);
            M4 = R54 * M5 + cross(P54, (R54 * F5)) - cross(r4, G4);
            M3 = R43 * M4 + cross(P43, (R43 * F4)) - cross(r3, G3);
            M2 = R32 * M3 + cross(P32, (R32 * F3)) - cross(r2, G2);
            M1 = R21 * M2 + cross(P21, (R21 * F2)) - cross(r1, G1);

            tau_g = new double[6] { M1[2], M2[2], M3[2], M4[2], M5[2], M6[2] };
            tau_fh = (Ja0.Transpose() * Vector<double>.Build.DenseOfArray(Fh)).ToArray();
            tau = (Vector<double>.Build.DenseOfArray(tau_g) + Vector<double>.Build.DenseOfArray(tau_fh)).ToArray();

            return true;
        }
        private static Vector<T> Append<T>(Vector<T> left, Vector<T> right) where T : struct, IEquatable<T>, IFormattable
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            Vector<T> vector = Vector<T>.Build.SameAs(left, left.Count + right.Count);
            left.Storage.CopySubVectorTo(vector.Storage, 0, 0, left.Count, ExistingData.AssumeZeros);
            right.Storage.CopySubVectorTo(vector.Storage, 0, left.Count, right.Count, ExistingData.AssumeZeros);
            return vector;
        }
        private Vector<double> cross(Vector<double> a, Vector<double> b)
        {
            Vector<double> res = Vector<double>.Build.DenseOfArray(new double[3]
            { a[1] * b[2] - a[2] * b[1] ,
            a[2]*b[0] - a[0]*b[2],
            a[0]*b[1] - a[1]*b[0]});
            return res;
        }
        
		/// <summary>
		/// 将角度归一化到(-pi,pi]
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		public static double angle_clip_pnpi(double q)
        {
            if (!islegal(q))
            {
                return double.NaN;
            }
            while ((q > Math.PI) || (q <= -Math.PI))
            {
                if (q > Math.PI)
                {
                    q = q - Math.PI * 2;
                    continue;
                }
                if (q <= -Math.PI)
                {
                    q = q + Math.PI * 2;
                    continue;
                }
            }
            return q;
        }
        private static double[] angle_clip_pnpi(double[] q)
        {
            double[] d = new double[q.Length];
            q.CopyTo(d, 0);
            for (int i = 0; i < q.Length; i++)
            {
                d[i] = angle_clip_pnpi(q[i]);
            }
            return d;
        }
        /// <summary>
        /// 将角度归一化到[0,2*pi)
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        private static double angle_clip_02pi(double q)
        {
            while ((q >= 2 * Math.PI) || (q < 0))
            {
                if (q >= 2 * Math.PI)
                {
                    q = q - Math.PI * 2;
                    continue;
                }
                if (q < 0)
                {
                    q = q + Math.PI * 2;
                    continue;
                }
            }
            return q;
        }
        /// <summary>
        /// 取两个角度之间的劣弧长度，弧度制
        /// </summary>
        /// <param name="angle1"></param>
        /// <param name="angle2"></param>
        /// <returns></returns>
        private static double minor_arc(double angle1, double angle2)
        {
            return Math.Abs(minor_arc_dir(angle1, angle2));
        }
        /// <summary>
        /// 取两组关节角之间的劣弧长度，弧度制
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        private static double[] minor_arc(double[] q1, double[] q2)
        {
            double[] result = new double[q1.Length];
            for (int i = 0; i < q1.Length; i++)
            {
                result[i] = minor_arc(q1[i], q2[i]);
            }
            return result;
        }
        /// <summary>
        /// 取起点与终点的带方向劣弧
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double minor_arc_dir(double start, double target)
        {
            double delta = angle_clip_pnpi(Math.PI * 2 - (start - target));
            return delta;
        }
        /// <summary>
        /// 取起点与终点角组合的带方向劣弧
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double[] minor_arc_dir(double[] start, double[] target)
        {
            double[] delta = new double[start.Length];
            for (int i = 0; i < start.Length; i++)
            {
                delta[i] = minor_arc_dir(start[i], target[i]);
            }
            return delta;
        }

        /// <summary>
        /// 给定旋转矩阵与位置变换矩阵，生成坐标变换矩阵
        /// </summary>
        public static double[,] rotpos2trans(double[,] Rot, double[] Pos)
        {
            double[,] T = new double[4, 4];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    T[i, j] = Rot[i, j];
            for (int i = 0; i < 3; i++)
            {
                T[3, i] = 0;
                T[i, 3] = Pos[i];
            }
            T[3, 3] = 1;
            return T;
        }
        /// <summary>
        /// 将坐标变换矩阵转为坐标系旋转矩阵
        /// </summary>
        public static double[,] trans2rot(double[,] T)
        {
            double[,] R = new double[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                { R[i, j] = T[i, j]; }
            return R;
        }
        /// <summary>
        /// 将坐标变换矩阵转为位移变换向量
        /// </summary>
        public static double[] trans2pos(double[,] T)
        {
            double[] P = { T[0, 3], T[1, 3], T[2, 3] };
            return P;
        }
		/// <summary>
		/// 将欧拉角转换为旋转矩阵
		/// </summary>
		/// <param name="RPY"></param>
		/// <returns></returns>
		public static double[,] rpy2rot(double[] RPY)
		{
			double a = RPY[0], b = RPY[1], c = RPY[2];
			double
				sinA = Math.Sin(a),
				cosA = Math.Cos(a),
				sinB = Math.Sin(b),
				cosB = Math.Cos(b),
				sinC = Math.Sin(c),
				cosC = Math.Cos(c);
			double[,] rot = new double[3, 3]
			{   {cosB*cosC,  cosC*sinA*sinB - cosA*sinC,  sinA*sinC + cosA*cosC*sinB },
				{cosB*sinC, cosA*cosC + sinA*sinB*sinC, cosA*sinB*sinC - cosC*sinA },
				{-sinB, cosB*sinA,  cosA*cosB }};
			return rot;
		}
		private static Matrix<double> rot_x(double a)
        {
            Matrix<double> Rot = Matrix<double>.Build.DenseOfArray(new double[3, 3]
            { {1,0,0},
            {0,Math.Cos(a),-Math.Sin(a) },
            {0,Math.Sin(a),Math.Cos(a) } });
            return Rot;
        }
        private static Matrix<double> rot_y(double b)
        {
            Matrix<double> Rot = Matrix<double>.Build.DenseOfArray(new double[3, 3]
            { {Math.Cos(b),0,Math.Sin(b)},
            {0,1,0 },
            {-Math.Sin(b),0,Math.Cos(b) } });
            return Rot;
        }
        private static Matrix<double> rot_z(double c)
        {
            Matrix<double> Rot = Matrix<double>.Build.DenseOfArray(new double[3, 3]
            { {Math.Cos(c),-Math.Sin(c),0},
            {Math.Sin(c),Math.Cos(c),0 },
            {0,0,1 } });
            return Rot;
        }
		public static double[] rot2rpy(double[,] R)
		{
			double a, b, c;

			// Check for singularity (when abs(R(3,1) - 1.0) is near 0)
			if (Math.Abs(R[2, 0] - 1.0) < 1.0e-15)
			{
				a = 0.0;
				b = -Math.PI / 2.0;
				c = Math.Atan2(-R[0, 1], -R[0, 2]);
			}
			else if (Math.Abs(R[2, 0] + 1.0) < 1.0e-15)  // Another singularity case
			{
				a = 0.0;
				b = Math.PI / 2.0;
				c = -Math.Atan2(R[0, 1], R[0, 2]);
			}
			else
			{
				a = Math.Atan2(R[2, 1], R[2, 2]);
				c = Math.Atan2(R[1, 0], R[0, 0]);
				double cosC = Math.Cos(c);
				double sinC = Math.Sin(c);

				if (Math.Abs(cosC) > Math.Abs(sinC))
				{
					b = Math.Atan2(-R[2, 0], R[0, 0] / cosC);
				}
				else
				{
					b = Math.Atan2(-R[2, 0], R[1, 0] / sinC);
				}
			}

			return new double[] { a, b, c };
		}
		/// <summary>
		/// 判断一个double值是否为实际值
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		private static bool islegal(double a)
        {
            if ((double.IsInfinity(a)) || (double.IsNaN(a)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
	}
}

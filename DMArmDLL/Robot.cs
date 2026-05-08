using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Win32.SafeHandles;


namespace DMArmDLL
{
    /// <summary>
    /// 正置机械臂
    /// </summary>
    public class Robot
    {
        public double[] ratio = new double[6] { 1, 1, -1, 1, -1, 1 };
        public double[] zero_offset = new double[6] { -1.57079632679490, 2.7321, -1.58889015515032, 0, 0.127707486697677, 0 };

        private double[] fh = new double[6];

        private double a1, a2, a3, d1, d4, d6;
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
            set
            {
                if (value.Length == 3)
                {
                    double[] q_now = ikine_near(rotpos2trans(rot, value), q);
                    if (q_now != null)//如果有解才改变机械臂状态
                    {
                        q_now.CopyTo(q, 0);
                        value.CopyTo(pos, 0);
                        trans = rotpos2trans(rot, pos);
                        safe = true;
                    }
                    else
                    {
                        safe = false;
                    }
                }
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
            set
            {
                if (value.Length == 3)
                {
                    double[] q_now = ikine_near(rotpos2trans(rpy2rot(value), pos), q);
                    if (q_now != null)//如果有解才改变机械臂状态
                    {
                        q_now.CopyTo(q, 0);
                        rot = rpy2rot(value);
                        trans = rotpos2trans(rot, pos);
                        safe = true;
                    }
                    else
                    {
                        safe = false;
                    }
                }
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
            set
            {
                if ((value.GetLength(0) == 4) && (value.GetLength(1) == 4))
                {
                    double[] q_now = ikine_near(value, q);
                    if (q_now != null)//如果有解才改变机械臂状态
                    {
                        q_now.CopyTo(q, 0);
                        trans = value;
                        rot = trans2rot(trans);
                        pos = trans2pos(trans);
                        safe = true;
                    }
                    else
                    {
                        safe = false;
                    }
                }
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
            set
            {
                if ((value.GetLength(0) == 3) && (value.GetLength(1) == 3))
                {
                    double[] q_now = ikine_near(rotpos2trans(value, pos), q);
                    if (q_now != null)//如果有解才改变机械臂状态
                    {
                        rot = value;
                        trans = rotpos2trans(rot, pos);
                        q_now.CopyTo(q, 0);
                        safe = true;
                    }
                    else
                    {
                        safe = false;
                    }
                }
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
        public double[] Fh
        {
            set
            {
                for (int i = 0; i < value.Length; i++)
                {
                    fh[i] = value[i];
                }
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
		public Robot()
        {
            d1 = 0.111; d4 = 0.245; d6 = 0.087;
            a1 = 0; a2 = 0.294; a3 = -0.0665;
            q = new double[6] { -Math.PI / 2, -Math.PI / 2, Math.PI / 2, 0, 0, Math.PI / 2 };
            q = new double[6];
            m = new double[6];
            //x = new double[6];
            //y = new double[6];
            //z = new double[6];

            double[][] mr = new double[6][];

            //-------达妙臂重量参数(无末端)----------
            mr[0] = new double[4] { 0.161449, 0, 0, -0.036144};
            mr[1] = new double[4] { 1.455285, 0.142943, 0.00000, -0.003 };
            //mr[2] = new double[4] { 0.750670, -0.064898, -0.052477, 0 };
			mr[2] = new double[4] { 0.597494, -0.066003, -0.118093, 0 };
			mr[3] = new double[4] { 0.521769, 0, -0.000857, -0.003509 };//黑色达妙臂参数
			mr[4] = new double[4] { 0.368476, 0, -0.05902, 0 };
			mr[5] = new double[4] { 0, 0, 0, 0 };
			//-------达妙臂重量参数(无末端)----------

			//-------末端力控夹钳参数----------------
			//mr[4] = new double[4] { 0.824610, 0, -0.107254, 0 };
			//-------末端力控夹钳参数----------------

			//-------末端示教器参数------------------
			//mr[4] = new double[4] { 0.628796, 0, -0.087305, 0 };
			//-------末端示教器参数------------------
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
        /// 重新设定机械臂参数：质量质心、杆长、减速比、零位偏置
        /// </summary>
        /// <param name="mr"></param>
        /// <param name="D1"></param>
        /// <param name="D4"></param>
        /// <param name="D6"></param>
        /// <param name="A1"></param>
        /// <param name="A2"></param>
        /// <param name="A3"></param>
        /// <param name="Ratio"></param>
        /// <param name="ZeroOffset"></param>
        public void reset_param(double[][] mr, 
            double D1, double D4, double D6, double A1, double A2, double A3, 
            double[] Ratio, double[] ZeroOffset)
        {
			d1 = D1; d4 = D4; d6 = D6;
			a1 = A1; a2 = A2; a3 = A3;
			q = new double[6] { -Math.PI / 2, -Math.PI / 2, Math.PI / 2, 0, 0, Math.PI / 2 };

            Ratio.CopyTo(ratio, 0);
            ZeroOffset.CopyTo(zero_offset, 0);

			m = new double[6];

			for (int i = 0; i < 6; i++)
			{
				m[i] = mr[i][0];
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
        public void reset_tool_param(double[] tool_mr)
        {
			double[][] mr = new double[6][];
			//-------达妙臂重量参数(用户设定末端)----------
			mr[0] = new double[4] { 0.161449, 0, 0, -0.036144 };
            mr[1] = new double[4] { 1.455285, 0.142943, 0.00000, -0.003 };
            //mr[2] = new double[4] { 0.750670, -0.064898, -0.052477, 0 };
			mr[2] = new double[4] { 0.597494, -0.066003, -0.118093, 0 };//黑色达妙臂参数														
			mr[3] = new double[4] { 0.521769, 0, -0.000857, -0.003509 };
			mr[4] = tool_mr;
			mr[5] = new double[4] { 0, 0, 0, 0 };
			//-------达妙臂重量参数(用户设定末端)---------- 
			m = new double[6];

			for (int i = 0; i < 6; i++)
			{
				m[i] = mr[i][0];
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
        /// 判断第i个电机motor_i能否到达DHi角度
        /// </summary>
        /// <param name="motor_i">第i个电机</param>
        /// <param name="dh_i">第i个DH角度</param>
        /// <param name="r">该关节的减速比</param>
        /// <param name="z">该关节的零位偏置</param>
        /// <returns></returns>
        private bool in_motor_lim(DMMotor motor_i, double dh_i, double r, double z)
        {
            double q1 = (dh_i - z) * r;
            double d = q1;
            if ((d < motor_i.angle_lim[0]) || (d > motor_i.angle_lim[1]))
            {
                d = q1 + 2 * Math.PI;
                if ((d < motor_i.angle_lim[0]) || (d > motor_i.angle_lim[1]))
                {
                    d = q1 - 2 * Math.PI;
                    if ((d < motor_i.angle_lim[0]) || (d > motor_i.angle_lim[1]))
                    {
                        return false;//不在电机范围内
                    }
                }
            }
            return true;
        }

		/// <summary>
		/// 判断电机能否到达当前的DH角度组合
		/// </summary>
		/// <param name="motors">限定为6个元素的Motor数组</param>
		/// <param name="dh">限定为6个元素的DH角度</param>
		/// <param name="InRange">为false的元素为不满足的关节</param>
		/// <returns>是否所有关节都在电机限位内</returns>
		private bool in_all_motor_lim(DMMotor[] motors, double[] dh, out bool[] InRange)
        {
            InRange = new bool[6];
            for (int i = 0; i < 6; i++)
            {
				InRange[i] = in_motor_lim(motors[i], dh[i], ratio[i], zero_offset[i]);
            }
            for (int i = 0; i < 6; i++)
            {
                if (!InRange[i])
                {
                    return false;
                }
            }
            return true;
        }

		/// <summary>
		/// 将DH角度组合根据零位偏置与减速比转换为电机角度组合
		/// </summary>
		/// <param name="motors"></param>
		/// <param name="dh"></param>
		/// <param name="motor_angle">转换完成的电机角度组合</param>
		/// <param name="InRange">若不成功，数组会记录哪个关节不成功</param>
		/// <returns>转换是否成功</returns>
		public bool dh2motor(DMMotor[] motors, double[] dh,out float[] motor_angle,out bool[] InRange)
        {
            InRange = new bool[6];
			motor_angle = new float[6];
			if (!in_all_motor_lim(motors, dh, out InRange))//如果无法满足
            {
                return false;
            }
            for (int i = 0; i < 6; i++)
            {
                double temp = (dh[i] - zero_offset[i]) * ratio[i];
                motor_angle[i] = (float)temp;
                if ((temp > motors[i].angle_lim[0]) && (temp < motors[i].angle_lim[1]))
                {
                    motor_angle[i] = (float)temp;
                    continue;
                }
                if ((temp + Math.PI * 2 > motors[i].angle_lim[0]) && (temp + Math.PI * 2 < motors[i].angle_lim[1]))
                {
                    motor_angle[i] = (float)(temp + Math.PI * 2);
                    continue;
                }
                if ((temp - Math.PI * 2 > motors[i].angle_lim[0]) && (temp - Math.PI * 2 < motors[i].angle_lim[1]))
                {
                    motor_angle[i] = (float)(temp - Math.PI * 2);
                    continue;
                }
            }
            return true;
        }
        public bool dh2motor(DMMotor[] motors, Matrix<double> dh, out Matrix<float> res)
        {
            res = Matrix<float>.Build.Dense(dh.RowCount, dh.ColumnCount);
			float[] motor_angle; bool[] in_range;
			for (int i = 0; i < dh.RowCount; i++)
            {                
                if (dh2motor(motors, dh.Row(i).ToArray(), out motor_angle, out in_range))
                {
                    res.SetRow(i, motor_angle);
                }
                else
                {
                    return false;
                }
            }
            return true;
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
                {0,     0,      1,  d1 },
                {0,     0,      0,  1 }});
            T21 = Matrix<double>.Build.DenseOfArray(
                new double[4, 4]{
                {c[1],  -s[1],  0,  a1},
                {0,      0,     1,  0},
                {-s[1], -c[1],  0,  0 },
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
                {0,      0,    -1, -d4 },
                {s[3],   c[3],  0,  0 },
                {0,      0,     0,  1 }});
            T54 = Matrix<double>.Build.DenseOfArray(
                new double[4, 4]{
                {c[4],  -s[4],  0,  0},
                {0,      0,     1,  0 },
                {-s[4], -c[4],  0,  0 },
                {0,      0,     0,  1 }});
            T65 = Matrix<double>.Build.DenseOfArray(
                 new double[4, 4]{
                {c[5],  -s[5],  0,  0},
                {0,      0,    -1, -d6 },
                {s[5],   c[5],  0,  0 },
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
            tau_fh = (Ja0.Transpose() * Vector<double>.Build.DenseOfArray(fh)).ToArray();
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
        /// 让机械臂末端沿世界坐标系轴移动设定距离
        /// </summary>
        /// <param name="distance">距离，单位为米</param>
        /// <param name="axis">轴，x,y,z</param>
        /// <returns>返回是否有解</returns>
        public bool move_world(double distance, char axis)
        {
            double[] pos_now = new double[3];
            pos.CopyTo(pos_now, 0);
            switch (axis)
            {
                case ('x'):
                    {
                        pos_now[0] = pos_now[0] + distance;
                        break;
                    }
                case ('y'):
                    {
                        pos_now[1] = pos_now[1] + distance;
                        break;
                    }
                case ('z'):
                    {
                        pos_now[2] = pos_now[2] + distance;
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }
            double[] q_now = ikine_near(rotpos2trans(rot, pos_now), q);
            if (q_now == null)
            {
                return false;
            }
            else
            {
                q_now.CopyTo(q, 0);
                set_robot();
                return true;
            }
        }
        /// <summary>
        /// 让机械臂末端沿末端坐标系轴移动设定距离
        /// </summary>
        /// <param name="distance">距离，单位为米</param>
        /// <param name="axis">轴，x,y,z</param>
        /// <returns>返回是否有解</returns>
        public bool move_self(double distance, char axis)
        {
            Matrix<double> I4 = Matrix<double>.Build.DenseIdentity(4);
            switch (axis)
            {
                case ('x'):
                    {
                        I4[0, 3] = distance;
                        break;
                    }
                case ('y'):
                    {
                        I4[1, 3] = distance;
                        break;
                    }
                case ('z'):
                    {
                        I4[2, 3] = distance;
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }
            double[,] trans_now = (Matrix<double>.Build.DenseOfArray(trans) * I4).ToArray();
            double[] q_now = ikine_near(trans_now, q);
            if (q_now == null)
            {
                return false;
            }
            else
            {
                q_now.CopyTo(q, 0);
                set_robot();
                return true;
            }
        }
        /// <summary>
        /// 绕世界坐标系的特定轴旋转特定角度
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="axis">轴：x/y/z</param>
        /// <returns></returns>
        public bool rotate_world(double angle, char axis)
        {
            Matrix<double> rot_old = Matrix<double>.Build.DenseOfArray(rot);
            Matrix<double> rot_new = Matrix<double>.Build.DenseOfArray(rot);
            switch (axis)
            {
                case ('x'):
                    {
                        rot_new = rot_x(angle) * rot_old;
                        break;
                    }
                case ('y'):
                    {
                        rot_new = rot_y(angle) * rot_old;
                        break;
                    }
                case ('z'):
                    {
                        rot_new = rot_z(angle) * rot_old;
                        break;
                    }
                default: break;
            }
            double[,] trans_now = rotpos2trans(rot_new.ToArray(), pos);
            double[] q_now = ikine_near(trans_now, q);
            if (q_now == null)
            {
                return false;
            }
            else
            {
                q_now.CopyTo(q, 0);
                set_robot();
                return true;
            }
        }
        /// <summary>
        /// 绕末端坐标系的特定轴旋转特定角度
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="axis">轴：x/y/z</param>
        /// <returns></returns>
        public bool rotate_self(double angle, char axis)
        {
            Matrix<double> rot_old = Matrix<double>.Build.DenseOfArray(rot);
            Matrix<double> rot_new = Matrix<double>.Build.DenseOfArray(rot);
            switch (axis)
            {
                case ('x'):
                    {
                        rot_new = rot_old * rot_x(angle);
                        break;
                    }
                case ('y'):
                    {
                        rot_new = rot_old * rot_y(angle);
                        break;
                    }
                case ('z'):
                    {
                        rot_new = rot_old * rot_z(angle);
                        break;
                    }
                default: break;
            }
            double[,] trans_now = rotpos2trans(rot_new.ToArray(), pos);
            double[] q_now = ikine_near(trans_now, q);
            if (q_now == null)
            {
                return false;
            }
            else
            {
                q_now.CopyTo(q, 0);
                set_robot();
                return true;
            }
        }
		public List<Vector<double>> ikine8(double[,] T6_0)
		{
			Matrix<double> T = Matrix<double>.Build.DenseOfArray(T6_0);
			T = T * Matrix<double>.Build.DenseOfArray(new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, -d6 }, { 0, 0, 0, 1 } });
			T[2, 3] = T[2, 3] - d1;

			double
			nx = T[0, 0], ny = T[1, 0], nz = T[2, 0],
			ox = T[0, 1], oy = T[1, 1], oz = T[2, 1],
			ax = T[0, 2], ay = T[1, 2], az = T[2, 2],
			px = T[0, 3], py = T[1, 3], pz = T[2, 3];
			double s1, c1, s2, c2, s3, c3, s5, c5;

			Tree t = new Tree(0);
			int t0 = 0;
			//---------求解q1-------------
			double[] q1 =
			{
				Math.Atan2(py, px),
				Math.Atan2(-py, -px),
			};
			int[] t1 =
			{
				t.addNode(t0,q1[0]),
				t.addNode(t0,q1[1]),
			};

			//---------求解q3-------------
			int[] t3 = new int[4];
			for (int i = 0; i < 2; i++)
			{
				s1 = t.node[t1[i]].sin;
				c1 = t.node[t1[i]].cos;
				double k3 = (px * px * c1 * c1 - py * py * c1 * c1 + a1 * a1 + py * py + pz * pz - 2 * a1 * px * c1 - 2 * a1 * py * s1 + 2 * px * py * c1 * s1 - a2 * a2 - a3 * a3 - d4 * d4) / (2 * a2);
				double[] q3 =
				{
					Math.Asin(k3/(Math.Sqrt(a3*a3+d4*d4)))-Math.Atan2(a3,d4),
					Math.PI-Math.Asin(k3/(Math.Sqrt(a3*a3+d4*d4)))-Math.Atan2(a3,d4)
				};
				t3[i] = t.addNode(t1[i], q3[0]);
				t3[i + 2] = t.addNode(t1[i], q3[1]);
			}

			//---------求解q2-------------
			int[] t2 = new int[4];
			for (int i = 0; i < 4; i++)
			{
				s3 = t.node[t3[i]].sin; c3 = t.node[t3[i]].cos;
				s1 = t.node[t.getParent(t3[i])].sin;
				c1 = t.node[t.getParent(t3[i])].cos;
				double
				e2 = a2 + d4 * s3 + a3 * c3,
				f2 = d4 * c3 - a3 * s3,
				g2 = px * c1 - a1 + py * s1,
				q2 = Math.Atan2(f2 * g2 - e2 * pz, g2 * e2 + f2 * pz);
				t2[i] = t.addNode(t3[i], q2);
			}

			//---------求解q5-------------
			int[] t5 = new int[8];
			for (int i = 0; i < 4; i++)
			{
				s2 = t.node[t2[i]].sin;
				c2 = t.node[t2[i]].cos;
				s3 = t.node[t.getParent(t2[i])].sin;
				c3 = t.node[t.getParent(t2[i])].cos;
				s1 = t.node[t.getParent(t2[i], 2)].sin;
				c1 = t.node[t.getParent(t2[i], 2)].cos;
				double[] q5 =
				{
					Math.Acos(az * c2 * c3 + ax * c1 * c2 * s3 + ax * c1 * c3 * s2 + ay * c2 * s1 * s3 + ay * c3 * s1 * s2 - az * s2 * s3),
					-Math.Acos(az * c2 * c3 + ax * c1 * c2 * s3 + ax * c1 * c3 * s2 + ay * c2 * s1 * s3 + ay * c3 * s1 * s2 - az * s2 * s3)
				};

				t5[i] = t.addNode(t2[i], q5[0]);
				t5[i + 4] = t.addNode(t2[i], q5[1]);
			}

			//---------求解q4,q6----------
			int[] t4 = new int[8], t6 = new int[8];
			for (int i = 0; i < 8; i++)
			{
				double q4, q6;
				double q5_ = t.node[t5[i]].q;
				s5 = t.node[t5[i]].sin;
				s2 = t.node[t.getParent(t5[i], 1)].sin;
				c2 = t.node[t.getParent(t5[i], 1)].cos;
				s3 = t.node[t.getParent(t5[i], 2)].sin;
				c3 = t.node[t.getParent(t5[i], 2)].cos;
				s1 = t.node[t.getParent(t5[i], 3)].sin;
				c1 = t.node[t.getParent(t5[i], 3)].cos;

				if (q5_ == 0)      //如果接近奇异点
				{
					double
					c46 = nx * c1 * c2 * c3 - nz * c3 * s2 - nz * c2 * s3 + ny * c2 * c3 * s1 - nx * c1 * s2 * s3 - ny * s1 * s2 * s3,
					s46 = ny * c1 - nx * s1,
					q46 = Math.Atan2(s46, c46);
					q4 = Math.PI / 2;//可以通过判断q4和q5的大小来判断是否接近奇异点
					q6 = q46 - q4;
				}
				else
				{
					double
					e4 = ax * c1 * c2 * c3 - az * c3 * s2 - az * c2 * s3 + ay * c2 * c3 * s1 - ax * c1 * s2 * s3 - ay * s1 * s2 * s3,
					f4 = ay * c1 - ax * s1,
					k6 = oz * c2 * c3 + ox * c1 * c2 * s3 + ox * c1 * c3 * s2 + oy * c2 * s1 * s3 + oy * c3 * s1 * s2 - oz * s2 * s3,
					r6 = nz * s2 * s3 - nz * c2 * c3 - nx * c1 * c2 * s3 - nx * c1 * c3 * s2 - ny * c2 * s1 * s3 - ny * c3 * s1 * s2;
					q4 = Math.Atan2(f4 / s5, e4 / s5);
					q6 = Math.Atan2(k6 / s5, r6 / s5);
				}
				t4[i] = t.addNode(t5[i], q4);
				t6[i] = t.addNode(t4[i], q6);
			}
			//----------------------------
			double[,] Q = new double[8, 6];
			for (int i = 0; i < 8; i++)
			{
				Q[i, 0] = angle_clip_pnpi(t.node[t.getParent(t6[i], 5)].q);
				Q[i, 1] = angle_clip_pnpi(t.node[t.getParent(t6[i], 3)].q);
				Q[i, 2] = angle_clip_pnpi(t.node[t.getParent(t6[i], 4)].q);
				Q[i, 3] = angle_clip_pnpi(t.node[t.getParent(t6[i], 1)].q);
				Q[i, 4] = angle_clip_pnpi(t.node[t.getParent(t6[i], 2)].q);
				Q[i, 5] = angle_clip_pnpi(t.node[t6[i]].q);
			}
			Matrix<double> Q_ = Matrix<double>.Build.DenseOfArray(Q);
			Vector<double> Q_row_sum = Q_.RowSums();
			List<Vector<double>> QS = new List<Vector<double>>();
			for (int i = 0; i < 8; i++)
			{
				if (!double.IsNaN(Q_row_sum[i]))
				{
					QS.Add(Q_.Row(i));
				}
			}
			return QS;
		}
		public double[] ikine_near(double[,] T6_0, double[] q0)
		{
			List<Vector<double>> QS = ikine8(T6_0);
			if (QS.Count == 0)
			{
				return null;
			}

			double min = double.PositiveInfinity;
			int min_index = 0;
			for (int i = 0; i < QS.Count; i++)
			{
				double[] q = QS[i].ToArray();
				if (q[4] == 0)//腕点奇异时，保持肘回转不变，只转腕回转
				{
					double q46 = q[3] + q[5];
					q[3] = q0[3];
					q[5] = q46 - q[3];
					QS[i][3] = q[3];
					QS[i][5] = q[5];
				}
				double minor_arc_norm = Vector<double>.Build.DenseOfArray(minor_arc(q, q0)).L2Norm();
				if (minor_arc_norm < min)
				{
					min = minor_arc_norm;
					min_index = i;
				}
			}
			return QS[min_index].ToArray();
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
        
        /*
        private double[] torque_out_nog(double[] q, double Fx, double Fy, double Fz, double Mx, double My, double Mz)
        {
            double[] t = new double[6];

            t[0] = Mz - Fx * (Math.Sin(q[0]) * (a1 + a3 * Math.Cos(q[1] + q[2]) + d4 * Math.Sin(q[1] + q[2]) + a2 * Math.Cos(q[1]) + (d6 * Math.Cos(q[1] + q[2]) * Math.Sin(q[3] + q[4])) / 2 + d6 * Math.Sin(q[1] + q[2]) * Math.Cos(q[4]) - (d6 * Math.Sin(q[3] - q[4]) * Math.Cos(q[1] + q[2])) / 2) + d6 * Math.Cos(q[0]) * Math.Sin(q[3]) * Math.Sin(q[4])) + Fy * (Math.Cos(q[0]) * (a1 + a3 * Math.Cos(q[1] + q[2]) + d4 * Math.Sin(q[1] + q[2]) + a2 * Math.Cos(q[1]) + (d6 * Math.Cos(q[1] + q[2]) * Math.Sin(q[3] + q[4])) / 2 + d6 * Math.Sin(q[1] + q[2]) * Math.Cos(q[4]) - (d6 * Math.Sin(q[3] - q[4]) * Math.Cos(q[1] + q[2])) / 2) - d6 * Math.Sin(q[0]) * Math.Sin(q[3]) * Math.Sin(q[4]));
            t[1] = My * Math.Cos(q[0]) - Fz * (a2 * Math.Cos(q[1]) + a3 * Math.Cos(q[1]) * Math.Cos(q[2]) + d4 * Math.Cos(q[1]) * Math.Sin(q[2]) + d4 * Math.Cos(q[2]) * Math.Sin(q[1]) - a3 * Math.Sin(q[1]) * Math.Sin(q[2]) + d6 * Math.Cos(q[1]) * Math.Cos(q[4]) * Math.Sin(q[2]) + d6 * Math.Cos(q[2]) * Math.Cos(q[4]) * Math.Sin(q[1]) + d6 * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Sin(q[4]) - d6 * Math.Cos(q[3]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4])) - Mx * Math.Sin(q[0]) - Fx * Math.Cos(q[0]) * (Math.Sin(q[1]) * (a2 + d6 * (Math.Cos(q[4]) * Math.Sin(q[2]) + Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Sin(q[4])) + a3 * Math.Cos(q[2]) + d4 * Math.Sin(q[2])) - Math.Cos(q[1]) * (d6 * (Math.Cos(q[2]) * Math.Cos(q[4]) - Math.Cos(q[3]) * Math.Sin(q[2]) * Math.Sin(q[4])) + d4 * Math.Cos(q[2]) - a3 * Math.Sin(q[2]))) - Fy * Math.Sin(q[0]) * (Math.Sin(q[1]) * (a2 + d6 * (Math.Cos(q[4]) * Math.Sin(q[2]) + Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Sin(q[4])) + a3 * Math.Cos(q[2]) + d4 * Math.Sin(q[2])) - Math.Cos(q[1]) * (d6 * (Math.Cos(q[2]) * Math.Cos(q[4]) - Math.Cos(q[3]) * Math.Sin(q[2]) * Math.Sin(q[4])) + d4 * Math.Cos(q[2]) - a3 * Math.Sin(q[2])));
            t[2] = My * Math.Cos(q[0]) - Fz * (a3 * Math.Cos(q[1] + q[2]) + d4 * Math.Sin(q[1] + q[2]) + d6 * Math.Sin(q[1] + q[2]) * Math.Cos(q[4]) + d6 * Math.Cos(q[1] + q[2]) * Math.Cos(q[3]) * Math.Sin(q[4])) - Mx * Math.Sin(q[0]) - Fx * Math.Cos(q[0]) * (Math.Sin(q[1] + q[2]) * (a3 + d6 * Math.Cos(q[3]) * Math.Sin(q[4])) - Math.Cos(q[1] + q[2]) * (d4 + d6 * Math.Cos(q[4]))) - Fy * Math.Sin(q[0]) * (Math.Sin(q[1] + q[2]) * (a3 + d6 * Math.Cos(q[3]) * Math.Sin(q[4])) - Math.Cos(q[1] + q[2]) * (d4 + d6 * Math.Cos(q[4])));
            t[3] = Mz * Math.Cos(q[1] + q[2]) + Mx * Math.Sin(q[1] + q[2]) * Math.Cos(q[0]) + My * Math.Sin(q[1] + q[2]) * Math.Sin(q[0]) - Fx * d6 * Math.Sin(q[4]) * (Math.Cos(q[3]) * Math.Sin(q[0]) + Math.Cos(q[0]) * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[3]) - Math.Cos(q[0]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3])) + Fy * d6 * Math.Sin(q[4]) * (Math.Cos(q[0]) * Math.Cos(q[3]) - Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[0]) * Math.Sin(q[3]) + Math.Sin(q[0]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3])) + Fz * d6 * Math.Sin(q[1] + q[2]) * Math.Sin(q[3]) * Math.Sin(q[4]);
            t[4] = Mz * Math.Sin(q[1] + q[2]) * Math.Sin(q[3]) + My * Math.Cos(q[0]) * Math.Cos(q[3]) - Mx * Math.Cos(q[3]) * Math.Sin(q[0]) - Mx * Math.Cos(q[0]) * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[3]) - My * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[0]) * Math.Sin(q[3]) + Mx * Math.Cos(q[0]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) + My * Math.Sin(q[0]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) + Fy * d6 * Math.Cos(q[0]) * Math.Cos(q[4]) * Math.Sin(q[3]) - Fz * d6 * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[4]) - Fx * d6 * Math.Cos(q[4]) * Math.Sin(q[0]) * Math.Sin(q[3]) + Fz * d6 * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) - Fz * d6 * Math.Cos(q[1]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[2]) - Fz * d6 * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[1]) - Fx * d6 * Math.Cos(q[0]) * Math.Cos(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) - Fx * d6 * Math.Cos(q[0]) * Math.Cos(q[2]) * Math.Sin(q[1]) * Math.Sin(q[4]) - Fy * d6 * Math.Cos(q[1]) * Math.Sin(q[0]) * Math.Sin(q[2]) * Math.Sin(q[4]) - Fy * d6 * Math.Cos(q[2]) * Math.Sin(q[0]) * Math.Sin(q[1]) * Math.Sin(q[4]) + Fx * d6 * Math.Cos(q[0]) * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) + Fy * d6 * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[0]) - Fx * d6 * Math.Cos(q[0]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[1]) * Math.Sin(q[2]) - Fy * d6 * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[0]) * Math.Sin(q[1]) * Math.Sin(q[2]);
            t[5] = Mz * (Math.Cos(q[1] + q[2]) * Math.Cos(q[4]) - Math.Sin(q[1] + q[2]) * Math.Cos(q[3]) * Math.Sin(q[4])) - Mx * (Math.Sin(q[4]) * (Math.Sin(q[0]) * Math.Sin(q[3]) - Math.Cos(q[3]) * (Math.Cos(q[0]) * Math.Cos(q[1]) * Math.Cos(q[2]) - Math.Cos(q[0]) * Math.Sin(q[1]) * Math.Sin(q[2]))) - Math.Cos(q[4]) * (Math.Cos(q[0]) * Math.Cos(q[1]) * Math.Sin(q[2]) + Math.Cos(q[0]) * Math.Cos(q[2]) * Math.Sin(q[1]))) + My * (Math.Sin(q[4]) * (Math.Cos(q[0]) * Math.Sin(q[3]) - Math.Cos(q[3]) * (Math.Sin(q[0]) * Math.Sin(q[1]) * Math.Sin(q[2]) - Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[0]))) + Math.Cos(q[4]) * (Math.Cos(q[1]) * Math.Sin(q[0]) * Math.Sin(q[2]) + Math.Cos(q[2]) * Math.Sin(q[0]) * Math.Sin(q[1])));
            double[] tt = { t[0], t[1], t[2], t[3], t[4], t[5] };
            return tt;
        }
        private double[] torque_g(double[] q)
        {
            double g = -9.80665;
            double[] t = new double[6];
            t[0] = 0;
            t[1] = g * (a2 * m[2] * Math.Cos(q[1]) + a2 * m[3] * Math.Cos(q[1]) + a2 * m[4] * Math.Cos(q[1]) + a2 * m[5] * Math.Cos(q[1]) + m[1] * x[1] * Math.Cos(q[1]) - m[1] * y[1] * Math.Sin(q[1]) - m[2] * x[2] * Math.Sin(q[1]) * Math.Sin(q[2]) + a3 * m[3] * Math.Cos(q[1]) * Math.Cos(q[2]) + a3 * m[4] * Math.Cos(q[1]) * Math.Cos(q[2]) + a3 * m[5] * Math.Cos(q[1]) * Math.Cos(q[2]) + d4 * m[3] * Math.Cos(q[1]) * Math.Sin(q[2]) + d4 * m[3] * Math.Cos(q[2]) * Math.Sin(q[1]) + d4 * m[4] * Math.Cos(q[1]) * Math.Sin(q[2]) + d4 * m[4] * Math.Cos(q[2]) * Math.Sin(q[1]) + d4 * m[5] * Math.Cos(q[1]) * Math.Sin(q[2]) + d4 * m[5] * Math.Cos(q[2]) * Math.Sin(q[1]) + m[2] * x[2] * Math.Cos(q[1]) * Math.Cos(q[2]) - a3 * m[3] * Math.Sin(q[1]) * Math.Sin(q[2]) - a3 * m[4] * Math.Sin(q[1]) * Math.Sin(q[2]) - a3 * m[5] * Math.Sin(q[1]) * Math.Sin(q[2]) - m[2] * y[2] * Math.Cos(q[1]) * Math.Sin(q[2]) - m[2] * y[2] * Math.Cos(q[2]) * Math.Sin(q[1]) + m[3] * z[3] * Math.Cos(q[1]) * Math.Sin(q[2]) + m[3] * z[3] * Math.Cos(q[2]) * Math.Sin(q[1]) + d6 * m[5] * Math.Cos(q[1]) * Math.Cos(q[4]) * Math.Sin(q[2]) + d6 * m[5] * Math.Cos(q[2]) * Math.Cos(q[4]) * Math.Sin(q[1]) + m[3] * x[3] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) - m[3] * y[3] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[3]) - m[4] * y[4] * Math.Cos(q[1]) * Math.Cos(q[4]) * Math.Sin(q[2]) - m[4] * y[4] * Math.Cos(q[2]) * Math.Cos(q[4]) * Math.Sin(q[1]) - m[4] * z[4] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[3]) + m[5] * z[5] * Math.Cos(q[1]) * Math.Cos(q[4]) * Math.Sin(q[2]) + m[5] * z[5] * Math.Cos(q[2]) * Math.Cos(q[4]) * Math.Sin(q[1]) - m[3] * x[3] * Math.Cos(q[3]) * Math.Sin(q[1]) * Math.Sin(q[2]) - m[4] * x[4] * Math.Cos(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) - m[4] * x[4] * Math.Cos(q[2]) * Math.Sin(q[1]) * Math.Sin(q[4]) + m[3] * y[3] * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) + m[4] * z[4] * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) + d6 * m[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Sin(q[4]) + m[4] * x[4] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) - m[4] * y[4] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Sin(q[4]) - m[5] * y[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[5]) * Math.Sin(q[3]) + m[5] * z[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Sin(q[4]) - d6 * m[5] * Math.Cos(q[3]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) - m[4] * x[4] * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[1]) * Math.Sin(q[2]) - m[5] * x[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[3]) * Math.Sin(q[5]) - m[5] * x[5] * Math.Cos(q[1]) * Math.Cos(q[5]) * Math.Sin(q[2]) * Math.Sin(q[4]) - m[5] * x[5] * Math.Cos(q[2]) * Math.Cos(q[5]) * Math.Sin(q[1]) * Math.Sin(q[4]) + m[4] * y[4] * Math.Cos(q[3]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) + m[5] * y[5] * Math.Cos(q[5]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) + m[5] * y[5] * Math.Cos(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) * Math.Sin(q[5]) + m[5] * y[5] * Math.Cos(q[2]) * Math.Sin(q[1]) * Math.Sin(q[4]) * Math.Sin(q[5]) - m[5] * z[5] * Math.Cos(q[3]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) + m[5] * x[5] * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) * Math.Sin(q[5]) + m[5] * x[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Cos(q[5]) - m[5] * y[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[5]) - m[5] * x[5] * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Cos(q[5]) * Math.Sin(q[1]) * Math.Sin(q[2]) + m[5] * y[5] * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[5]));
            t[2] = g * (a3 * m[3] * Math.Cos(q[1]) * Math.Cos(q[2]) - m[2] * x[2] * Math.Sin(q[1]) * Math.Sin(q[2]) + a3 * m[4] * Math.Cos(q[1]) * Math.Cos(q[2]) + a3 * m[5] * Math.Cos(q[1]) * Math.Cos(q[2]) + d4 * m[3] * Math.Cos(q[1]) * Math.Sin(q[2]) + d4 * m[3] * Math.Cos(q[2]) * Math.Sin(q[1]) + d4 * m[4] * Math.Cos(q[1]) * Math.Sin(q[2]) + d4 * m[4] * Math.Cos(q[2]) * Math.Sin(q[1]) + d4 * m[5] * Math.Cos(q[1]) * Math.Sin(q[2]) + d4 * m[5] * Math.Cos(q[2]) * Math.Sin(q[1]) + m[2] * x[2] * Math.Cos(q[1]) * Math.Cos(q[2]) - a3 * m[3] * Math.Sin(q[1]) * Math.Sin(q[2]) - a3 * m[4] * Math.Sin(q[1]) * Math.Sin(q[2]) - a3 * m[5] * Math.Sin(q[1]) * Math.Sin(q[2]) - m[2] * y[2] * Math.Cos(q[1]) * Math.Sin(q[2]) - m[2] * y[2] * Math.Cos(q[2]) * Math.Sin(q[1]) + m[3] * z[3] * Math.Cos(q[1]) * Math.Sin(q[2]) + m[3] * z[3] * Math.Cos(q[2]) * Math.Sin(q[1]) + d6 * m[5] * Math.Cos(q[1]) * Math.Cos(q[4]) * Math.Sin(q[2]) + d6 * m[5] * Math.Cos(q[2]) * Math.Cos(q[4]) * Math.Sin(q[1]) + m[3] * x[3] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) - m[3] * y[3] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[3]) - m[4] * y[4] * Math.Cos(q[1]) * Math.Cos(q[4]) * Math.Sin(q[2]) - m[4] * y[4] * Math.Cos(q[2]) * Math.Cos(q[4]) * Math.Sin(q[1]) - m[4] * z[4] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[3]) + m[5] * z[5] * Math.Cos(q[1]) * Math.Cos(q[4]) * Math.Sin(q[2]) + m[5] * z[5] * Math.Cos(q[2]) * Math.Cos(q[4]) * Math.Sin(q[1]) - m[3] * x[3] * Math.Cos(q[3]) * Math.Sin(q[1]) * Math.Sin(q[2]) - m[4] * x[4] * Math.Cos(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) - m[4] * x[4] * Math.Cos(q[2]) * Math.Sin(q[1]) * Math.Sin(q[4]) + m[3] * y[3] * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) + m[4] * z[4] * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) + d6 * m[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Sin(q[4]) + m[4] * x[4] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) - m[4] * y[4] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Sin(q[4]) - m[5] * y[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[5]) * Math.Sin(q[3]) + m[5] * z[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Sin(q[4]) - d6 * m[5] * Math.Cos(q[3]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) - m[4] * x[4] * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[1]) * Math.Sin(q[2]) - m[5] * x[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[3]) * Math.Sin(q[5]) - m[5] * x[5] * Math.Cos(q[1]) * Math.Cos(q[5]) * Math.Sin(q[2]) * Math.Sin(q[4]) - m[5] * x[5] * Math.Cos(q[2]) * Math.Cos(q[5]) * Math.Sin(q[1]) * Math.Sin(q[4]) + m[4] * y[4] * Math.Cos(q[3]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) + m[5] * y[5] * Math.Cos(q[5]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) + m[5] * y[5] * Math.Cos(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) * Math.Sin(q[5]) + m[5] * y[5] * Math.Cos(q[2]) * Math.Sin(q[1]) * Math.Sin(q[4]) * Math.Sin(q[5]) - m[5] * z[5] * Math.Cos(q[3]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) + m[5] * x[5] * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[3]) * Math.Sin(q[5]) + m[5] * x[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Cos(q[5]) - m[5] * y[5] * Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[5]) - m[5] * x[5] * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Cos(q[5]) * Math.Sin(q[1]) * Math.Sin(q[2]) + m[5] * y[5] * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[5]));
            t[3] = -g * Math.Sin(q[1] + q[2]) * (m[3] * y[3] * Math.Cos(q[3]) + m[4] * z[4] * Math.Cos(q[3]) + m[3] * x[3] * Math.Sin(q[3]) - m[4] * y[4] * Math.Sin(q[3]) * Math.Sin(q[4]) + m[5] * z[5] * Math.Sin(q[3]) * Math.Sin(q[4]) + m[5] * y[5] * Math.Cos(q[3]) * Math.Cos(q[5]) + d6 * m[5] * Math.Sin(q[3]) * Math.Sin(q[4]) + m[4] * x[4] * Math.Cos(q[4]) * Math.Sin(q[3]) + m[5] * x[5] * Math.Cos(q[3]) * Math.Sin(q[5]) + m[5] * x[5] * Math.Cos(q[4]) * Math.Cos(q[5]) * Math.Sin(q[3]) - m[5] * y[5] * Math.Cos(q[4]) * Math.Sin(q[3]) * Math.Sin(q[5]));
            t[4] = Math.Cos(q[5]) * (g * m[5] * z[5] * (Math.Cos(q[1] + q[2]) * Math.Cos(q[5]) * Math.Sin(q[4]) - Math.Sin(q[1] + q[2]) * Math.Sin(q[3]) * Math.Sin(q[5]) + Math.Sin(q[1] + q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Cos(q[5])) + g * m[5] * x[5] * (Math.Cos(q[1] + q[2]) * Math.Cos(q[4]) - Math.Sin(q[1] + q[2]) * Math.Cos(q[3]) * Math.Sin(q[4]))) + Math.Sin(q[5]) * (g * m[5] * z[5] * (Math.Sin(q[1] + q[2]) * Math.Cos(q[5]) * Math.Sin(q[3]) + Math.Cos(q[1] + q[2]) * Math.Sin(q[4]) * Math.Sin(q[5]) + Math.Sin(q[1] + q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[5])) - g * m[5] * y[5] * (Math.Cos(q[1] + q[2]) * Math.Cos(q[4]) - Math.Sin(q[1] + q[2]) * Math.Cos(q[3]) * Math.Sin(q[4]))) + d6 * g * m[5] * (Math.Cos(q[1]) * Math.Cos(q[2]) * Math.Sin(q[4]) - Math.Sin(q[1]) * Math.Sin(q[2]) * Math.Sin(q[4]) + Math.Cos(q[1]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[2]) + Math.Cos(q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[1])) + g * m[4] * x[4] * (Math.Cos(q[1] + q[2]) * Math.Cos(q[4]) - Math.Sin(q[1] + q[2]) * Math.Cos(q[3]) * Math.Sin(q[4])) - g * m[4] * y[4] * (Math.Cos(q[1] + q[2]) * Math.Sin(q[4]) + Math.Sin(q[1] + q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]));
            t[5] = -g * m[5] * y[5] * (Math.Cos(q[1] + q[2]) * Math.Cos(q[5]) * Math.Sin(q[4]) - Math.Sin(q[1] + q[2]) * Math.Sin(q[3]) * Math.Sin(q[5]) + Math.Sin(q[1] + q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Cos(q[5])) - g * m[5] * x[5] * (Math.Sin(q[1] + q[2]) * Math.Cos(q[5]) * Math.Sin(q[3]) + Math.Cos(q[1] + q[2]) * Math.Sin(q[4]) * Math.Sin(q[5]) + Math.Sin(q[1] + q[2]) * Math.Cos(q[3]) * Math.Cos(q[4]) * Math.Sin(q[5]));

            double[] tt = { t[0], t[1], t[2], t[3], t[4], t[5] };
            return tt;
        }
        */
    
    }
}

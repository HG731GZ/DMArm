using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;


namespace DMArmDLL
{
    public class DMMotor
    {
        private uint id, master_id;

        public readonly uint PARAM_SET_ID = 0x7FF;

        public float max_position, max_velocity, max_torque;//注意这三个参数一定要和电机实际设定参数匹配，否则控制会出现错误
        public float kp_max = 500.0f, kd_max = 5.0f;
        
        private float position, velocity, torque;
                
        private uint ERR;
        public int tem_mos, tem_rotor;

        private bool enable = false;
        private byte[] mit_command = new byte[8], pv_command = new byte[8], pvt_command = new byte[8];

        public ulong recv_num;

        public static byte[] enable_command = new byte[8] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFC };
        public static byte[] disable_command = new byte[8] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFD };
        public static byte[] clear_error_command = new byte[8] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFB };
        public static byte[] set_zero_command = new byte[8] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE };
        
		public byte[] set_mit_command = new byte[8] { 0x00, 0x00, 0x55, 10, 0x01, 0x00, 0x00, 0x00 };
		public byte[] set_pv_command = new byte[8] { 0x00, 0x00, 0x55, 10, 0x02, 0x00, 0x00, 0x00 };
		public byte[] set_pvt_command = new byte[8] { 0x00, 0x00, 0x55, 10, 0x04, 0x00, 0x00, 0x00 };
        public byte[] get_mode_command = new byte[] { 0x00, 0x00, 0x33, 10, 0x00, 0x00, 0x00, 0x00 };

		public MIT_ MIT = new MIT_();
		public PV_ PV = new PV_();
		public PVT_ PVT = new PVT_();

		/// <summary>
		/// 当前模式，1为MIT，2为PV
		/// </summary>
		private int mode = 2;

        public double[] angle_lim = new double[2] { -Math.PI, Math.PI };

        public struct MIT_
        {
            public float position_set, velocity_set, torque_set, kp_set, kd_set;
        }
        public struct PV_
        {
            public float position_set, velocity_lim;
        }
        public struct PVT_
        {
            public float position_set, velocity_lim, torque_lim;
        }

        /*
        //public Motor(uint motorID, float MaxPosition, float MaxVelocity, float MaxTorque)
        //{
        //    id = motorID;
        //    id_offset = motorID + 0x100;
        //    max_position = MaxPosition;
        //    max_velocity = MaxVelocity;
        //    max_torque = MaxTorque;
        //    position_set = 0;
        //    velocity_set = 0;
        //    torque_set = 0;
        //    kd_set = 0; kp_set = 0;
        //    canbuffer = new CANBuffer(buffer_size);
        //    enable = false;
        //    command = new byte[] { 0x7F, 0xFF, 0x7F, 0xF0, 0x00, 0x00, 0x07, 0xFF };
        //    recv_num = 0;
        //    set_empty_command_PV();
        //}
        */
        #region 私有类读取接口
        public string ERRCODE
		{
			get
			{
				switch (ERR)
				{
                    case 0:
                        return "失能";
                    case 1:
                        return "使能";
					case 0x08:
						return "超压";
					case 0x09:
						return "欠压";
					case 0x0A:
						return "过流";
					case 0x0B:
						return "MOS过热";
					case 0x0C:
						return "线圈过热";
					case 0x0D:
						return "通讯丢失";
					case 0x0E:
						return "过载";
					default:
						return "??";
				}
			}
		}
		public float Position
        {
            get { return position; }
        }
		public float Velocity
		{
			get { return velocity; }
		}
		public float Torque
		{
			get { return torque; }
		}
        public byte[] Command
        {
            get
            {
                switch (mode)
                {
                    case 1:
                        return mit_command;
                    case 2:
                        return pv_command;
                    case 4:
                        return pvt_command;
                    case -1:
                        return set_mit_command;
                    case -2:
                        return set_pv_command;
                    default: return disable_command;
                }
            }
        }
        public uint ID_OFFSET//根据模式偏移ID
		{
            get
            {
                switch (mode)
                {
                    case 1:
                        return id;
                    case 2:
                        return id + 0x100;
                    case 4:
                        return id + 0x300;
                    case -1:
                        return PARAM_SET_ID;
                    case -2:
                        return PARAM_SET_ID;
                    case -4:
                        return PARAM_SET_ID;
                    default: return id;
                }
			}
        }
        public uint ID
        {
            get
            {
                return id;
            }
        }
        public uint ID_MASTER
        {
            get
            {
                return master_id;
            }
        }
		public bool Enable
        {
            get
            {
                return enable;
            }
        }
        /// <summary>
        /// 1为MIT模式，2为PV模式；-1为设定为MIT模式，-2为设定为PV模式
        /// </summary>
        public int Mode
        {
            get
            {
                return mode;
            }
        }
        public string ModeName
        {
            get
            {
                switch (mode)
                {
                    case 1:
                        return "MIT";
                    case 2:
                        return "PV";
                    case 4:
                        return "PVT";
                    default:
                        return "??";
                }
            }
        }
        #endregion
		public DMMotor(uint motorID, uint motorType, string mode)  //按照电机类型与上电模式来定义电机参数简化初始化
        {
            id = motorID;
            master_id = id + 0x010;

            max_position = 0;
            max_velocity = 0;
            max_torque = 0;

            switch (motorType)
            {
                case 4340:
                    {
						max_position = 12.5f;
						max_velocity = 10.0f;
						max_torque = 28.0f;
                        break;
					}
                case 4310:
                    {
						max_position = 12.5f;
						max_velocity = 30.0f;
						max_torque = 10.0f;
                        break;
					}
                case 3507:
                    {
						max_position = 12.5f;
						max_velocity = 50.0f;
						max_torque = 5.0f;
                        break;
					}
                default:
                    {
						max_position = 12.5f;
						max_velocity = 10.0f;
						max_torque = 28.0f;
                        break;
					}
            }
			switch (mode)
			{
				case "MIT":
					{
						this.mode = 1;
						break;
					}
				case "PV":
					{
						this.mode = 2;
						break;
					}
				case "PVT":
					{
						this.mode = 4;
						break;
					}
			}
			
            enable = false;
			set_mit_command[0] = (byte)id;
            set_pv_command[0] = (byte)id;
			set_pvt_command[0] = (byte)id;
			get_mode_command[0] = (byte)id;

			recv_num = 0;

            set_empty_command();          
        }

        private byte[] convert_to_candata_MIT(float Position, float Velocity, float Torque, float KP, float KD)
        {
            byte[] CANDATA = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            UInt16 pos = (UInt16)(((Position + max_position) * 65535) / (max_position * 2));
            CANDATA[0] = (Byte)(0xFF & (pos >> 8));
            CANDATA[1] = (Byte)(0xFF & pos);

            UInt16 vel = (UInt16)(((Velocity + max_velocity) * 4095) / (max_velocity * 2));
            CANDATA[2] = (Byte)(0xFF & (vel >> 4));

            UInt16 kp = (UInt16)(KP * 4095 / kp_max);
            CANDATA[3] = (Byte)(0xFF & (vel << 4 | kp >> 8));
            CANDATA[4] = (Byte)(0xFF & kp);

            UInt16 kd = (UInt16)(KD * 4095 / kd_max);
            CANDATA[5] = (Byte)(0xFF & (kd >> 4));

            UInt16 tor = (UInt16)(((Torque + max_torque) * 4095) / (max_torque * 2));
            CANDATA[6] = (Byte)(0xFF & (kd << 4 | tor >> 8));
            CANDATA[7] = (Byte)(0xFF & tor);
            return CANDATA;
        }
        private static byte[] convert_to_candata_PV(float Position, float Velocity)
        {
            byte[] p_des = BitConverter.GetBytes(Position);
            byte[] v_des = BitConverter.GetBytes(Velocity);
            byte[] CANDATA = p_des.Concat(v_des).ToArray<byte>();
            return CANDATA;
        }
        private static byte[] convert_to_candata_PVT(float Position, UInt16 Velocity, UInt16 Current)
        {
            Current = Math.Min(Current, (UInt16)5000);
            Velocity = Math.Min(Velocity, (UInt16)5000);
            Position = Math.Min(Position, 2.35f);
            byte[] p_des = BitConverter.GetBytes(Position);
            byte[] v_des = BitConverter.GetBytes(Velocity);
            byte[] i_des = BitConverter.GetBytes(Current);
			byte[] CANDATA = p_des.Concat(v_des).Concat(i_des).ToArray();
			return CANDATA;
		}
		private static byte[] convert_to_candata_PVT(float Position, UInt16 Velocity, float Torque)
		{
            UInt16 Current = (UInt16)(Math.Abs(Torque) * 1800);
            return convert_to_candata_PVT(Position, Velocity, Current);
		}
        public bool read_motor(byte[] CANDATA)
        {
            //如果此时电机处于模式切换状态，可能收到模式切换指令
            if (mode < 0)
            {
                if (get_motor_mode(CANDATA))
                { 
                    return true; 
                }
            }
            recv_num += 1;
            if (CANDATA == null)
            {
                return false;//解析失败
            }
            if ((uint)(CANDATA[0] & 0x0F) != id)
            {
                return false;
            }
            ERR = (uint)(0xF0 & CANDATA[0]) >> 4;
            position = (float)(CANDATA[1] << 8 | CANDATA[2]) / 65536 * max_position * 2 - max_position;
            velocity = (float)(((CANDATA[3] << 4) | (CANDATA[4] >> 4)) - 2048) / 4096 * max_velocity * 2;
            torque = (float)((((CANDATA[4] & 0x0F) << 8) | CANDATA[5]) - 2048) / 4096 * max_torque * 2;
            tem_mos = CANDATA[6];
            tem_rotor = CANDATA[7];

            if (ERR == 0)
            {
                enable = false;
            }
            if (ERR == 1)
            {
                enable = true;
            }
            return true;
        }
        public bool get_motor_mode(byte[] CANDATA)
        {
            Array.Resize(ref CANDATA, 8);
            if (((CANDATA[2] == 0x33) || (CANDATA[2] == 0x55)) && (CANDATA[3] == 10))
            {
                mode = CANDATA[4];
                return true;
            }
            return false;//说明此帧不是模式切换指令的反馈
        }

		public void set()
        {
            switch (mode)
            {
                case 1://如果是MIT模式
                    {
                        mit_command = convert_to_candata_MIT(MIT.position_set, MIT.velocity_set, MIT.torque_set, 
                            MIT.kp_set, MIT.kd_set);						
                        break;
                    }
                case 2:
                    {
                        pv_command = convert_to_candata_PV(PV.position_set, PV.velocity_lim);
						break;
                    }
                case 4:
                    {
                        pvt_command = convert_to_candata_PVT(PVT.position_set,
                            (UInt16)(PVT.velocity_lim * 100), PVT.torque_lim);
						break;
                    }
                default:
                    {
                        set_empty_command();
						break;
                    }
            }
        }
        private void set_empty_command_PV()
        {
            pv_command = convert_to_candata_PV(0f, 0f);
            PV = new PV_();
        }
        private void set_empty_command_MIT()
        {
            mit_command = convert_to_candata_MIT(0f, 0f, 0f, 0f, 0f);
            MIT = new MIT_();
        }
		private void set_empty_command_PVT()
		{
            pvt_command = convert_to_candata_PVT(0f, 0, 0);
            PVT = new PVT_();
		}
        public void set_empty_command()
        {
			set_empty_command_PV();
			set_empty_command_MIT();
			set_empty_command_PVT();
		}
        public void set_mode(int mode)
        {
            this.mode = -mode;
            set_empty_command();
        }
		#region 旧版代码不再使用
		private void set_MIT()
		{
			//mit_command = convert_to_candata_MIT(position_set, velocity_set, torque_set, kp_set, kd_set);
		}
		private void set_PV()
		{
			//pv_command = convert_to_candata_PV(position_set, velocity_set);
		}
		private void set_PVT()
		{
			//pvt_command = convert_to_candata_PVT(position_set, (UInt16)(velocity_set * 100), torque_set);
		}
		#endregion
	}
}

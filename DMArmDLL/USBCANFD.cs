using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLGCAN;
using System.Threading;
using DMArmDLL;
using SharpDX.XInput;
using SharpDX;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;
using System.ComponentModel.Design;

namespace DMArmDLL
{
    public class USBCANFD
    {
        IntPtr device_handle_;
        IntPtr channel_handle_;
        DeviceInfo kDeviceType = new DeviceInfo(Define.ZCAN_USBCANFD_MINI, 1);
        readonly int channel_index_ = 0;
        //const int CANFD_BRS = 0x01;

        const int MOTOR_NUM = 6, TOOL_NUM = 1;
        const int SLAVE_NUM = MOTOR_NUM + TOOL_NUM;

        /// <summary>
        /// 系统每刷新多少次就更新一次末端信息
        /// </summary>
        private uint tool_update = 1;
        private bool is_open, is_updating;

        /// <summary>
        /// CAN总线信息(帧率、负载率)多久更新一次，单位毫秒
        /// </summary>
        public uint can_param_time = 200;
        /// <summary>
        /// CAN总线信息，依次为[0]帧率，[1]负载率，[2]发送成功帧数，[3]发送失败帧数，[4]接收帧数，[5]系统刷新次数、[6]系统刷新率
        /// </summary>
        private double[] can_param = new double[7];

        private ulong send_suc_num, send_err_num, recv_num, system_update;//发送成功、错误、接收，以及系统刷新次数的计数

        public DMMotor[] motors = new DMMotor[MOTOR_NUM];
        public DMMotor[] tools = new DMMotor[Math.Max(TOOL_NUM, 1)];

        private ZCAN_TransmitFD_Data[] canfd_queue = new ZCAN_TransmitFD_Data[SLAVE_NUM];

        private Thread can_recv_trd, can_send_trd, can_param_update_trd;

        private ZCAN_TransmitFD_Data canfd_send_data = new ZCAN_TransmitFD_Data();

        #region 与电机有关的变量
        private List<int> motor_mode = new List<int>(new int[MOTOR_NUM]);
        /// <summary>
        /// 检查电机是否锁定。只在系统模式为MIT模式时，调整锁定有意义。
        /// </summary>
        private List<bool> motor_lock = new List<bool>(new bool[MOTOR_NUM]);
		#endregion

		/// <summary>
		/// 非0时表示需要切换模式，1为MIT，2为位置速度
		/// </summary>
		public byte mode_switch_flag = 0;

        #region 开放外界读取参数
        /// <summary>
        /// 设备是否开启
        /// </summary>
        public bool IsOpen
        { get { return is_open; } }
        /// <summary>
        /// 设备是否在更新
        /// </summary>
        public bool IsUpdating
        { get { return is_updating; } }
		/// <summary>
		/// CAN总线信息，依次为[0]帧率，[1]负载率，[2]发送成功帧数，[3]发送失败帧数，[4]接收帧数，[5]系统刷新次数、[6]系统刷新率
		/// </summary>
		public double[] CanParam
        { 
            get
            {
                return can_param; 
            } 
        }
        public byte Mode
        {
            get
            {
                if (motor_mode.All(v => v == 1))
                {
                    return 1;
                }
                if (motor_mode.All(v => v == 2))
                {
                    return 2;
                }
                return 0;
            }
        }
        #endregion
        public USBCANFD()
        {
            
            is_open = false;
            is_updating = false;
            tool_update = 50;
            for (uint i = 0; i < 3; i++)
            {
                motors[i] = new DMMotor(i + 1, 4340, "PV");
            }
            for (uint i = 3; i < 6; i++)
            {
                motors[i] = new DMMotor(i + 1, 4310, "PV");
            }
			//motors[6] = new Motor(0x21, 4310);
   //         motors[6].id_offset = motors[6].id;

			motors[1].angle_lim = new double[2] { 0, 213d * Math.PI / 180d };
            motors[2].angle_lim = new double[2] { 0, 182d * Math.PI / 180d };
            motors[4].angle_lim = new double[2] { -84d * Math.PI / 180d, 98d * Math.PI / 180 };

            tools[0] = new DMMotor(7, 3507, "PVT");

            canfd_send_data.frame.can_id = 0;
            canfd_send_data.frame.data = new byte[64];
            canfd_send_data.frame.len = 8;
            canfd_send_data.transmit_type = 1;
            canfd_send_data.frame.flags = 0x01;

            //CANDFD队列发送初始化
			for (uint i = 0; i < MOTOR_NUM; i++)
            {
				canfd_queue[i].transmit_type = 1;
				canfd_queue[i].frame.len = 8;
				canfd_queue[i].frame.can_id = motors[i].ID_OFFSET;
				canfd_queue[i].frame.data = new byte[64];
				canfd_queue[i].frame.flags = 0x11;
				canfd_queue[i].frame.__res0 = 1;
				canfd_queue[i].frame.__res1 = 0;
				motors[i].Command.CopyTo(canfd_queue[i].frame.data, 0);
			}
            for (uint i = 0; i < MOTOR_NUM; i++)
            {
                motor_mode[(int)i] = motors[i].Mode;
            }
		}
        /// <summary>
        /// 开启USB转CANFD设备
        /// </summary>
        /// <returns>是否成功</returns>
        public bool open_device()
        {
            uint device_index_ = 0;
            device_handle_ = Method.ZCAN_OpenDevice(kDeviceType.device_type, device_index_, 0);
            if ((int)device_handle_ == 0)
            {
                Console.WriteLine("无法打开设备");
                return false;
            }
            is_open = true;
            return true;
        }
        /// <summary>
        /// 关闭USB转CAN设备
        /// </summary>
        /// <returns>是否成功</returns>
        public bool close_device()
        {
            stop_can();
            if (is_open == true)//如果打开了才执行
            {
                if (Method.ZCAN_CloseDevice(device_handle_) == Define.STATUS_OK)
                {
                    is_open = false;
                }
            }
            return is_open != true;
        }
        /// <summary>
        /// 初始化CANFD设备
        /// </summary>
        /// <returns></returns>
        public bool init_device()
        {
            uint type = kDeviceType.device_type;
            bool usbCanfd = true;
            bool canfdDevice = usbCanfd;
            if (!setCANFDStandard(0))
            {
                Console.WriteLine("设置CANFD标准失败");
                return false;
            }
            bool result = true;
            //result = setFdBaudrate(1000000, 5000000);
            result = setCustomBaudrate("1.0Mbps(75%),5.0Mbps(75%),(60,00000E2B,00800001)");
            if (!result)
            {
                Console.WriteLine("设置波特率失败");
                return false;
            }
            ZCAN_CHANNEL_INIT_CONFIG config_ = new ZCAN_CHANNEL_INIT_CONFIG();
            config_.can_type = Define.TYPE_CANFD;
            config_.canfd.mode = 0;
            IntPtr pConfig = Marshal.AllocHGlobal(Marshal.SizeOf(config_));
            Marshal.StructureToPtr(config_, pConfig, true);

            channel_handle_ = Method.ZCAN_InitCAN(device_handle_, (uint)channel_index_, pConfig);
            Marshal.FreeHGlobal(pConfig);
            if ((int)channel_handle_ == 0)
            {
                Console.WriteLine("初始化CAN失败");
                return false;
            }
            if (!setResistanceEnable(true))
            {
                Console.WriteLine("使能终端电阻失败");
                return false;
            }
            if (!setFilter())
            {
                Console.WriteLine("滤波设置失败");
                return false;
            }
            if (Method.ZCAN_ClearBuffer(channel_handle_) != Define.STATUS_OK)
            {
                Console.WriteLine("清空缓冲区失败");
                return false;
            }
			Method.ZCAN_SetValue(device_handle_, "0/set_device_tx_echo", Encoding.ASCII.GetBytes("0"));
			return true;
        }

        /// <summary>
        /// 启动CANFD设备
        /// </summary>
        public bool start_device()
        {
            if (Method.ZCAN_StartCAN(channel_handle_) != Define.STATUS_OK)
            {
                Console.WriteLine("启动CAN失败");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 通过CANFD以CANFD加速发送一帧数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool canfd_send(uint id, byte[] data)
        {
            int frame_type_index = 0;
            //int protocol_index = 1;//0是CAN，1是CANFD
            int send_type_index = 1;//单次发送
            int canfd_exp_index = 1;//是否CANFD加速
            uint result; //发送的帧数
			//ZCAN_TransmitFD_Data canfd_data = new ZCAN_TransmitFD_Data();
			//canfd_data.frame.can_id = MakeCanId(id, frame_type_index, 0, 0);
			//canfd_data.frame.data = new byte[8];
			//canfd_data.frame.len = 8;
			//canfd_data.transmit_type = (uint)send_type_index;
			//canfd_data.frame.flags = (byte)((canfd_exp_index != 0) ? CANFD_BRS : 0);
			//canfd_send_data.frame.can_id = MakeCanId(id, frame_type_index, 0, 0);
			canfd_send_data.frame.can_id = id;
			data.CopyTo(canfd_send_data.frame.data, 0);
            try
            {
                //IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(canfd_send_data));
                //Marshal.StructureToPtr(canfd_send_data, ptr, true);
                result = Method.ZCAN_TransmitFD(channel_handle_, ref canfd_send_data, 1);
                //Marshal.FreeHGlobal(ptr);
            }
            catch
            {
                result = 1;
            }
            if (result != 1)
            {
				//Console.WriteLine("发送成功");
				return true;
            }
            else
            {
                //Console.WriteLine("发送失败");
                return false;
            }
        }
        /// <summary>
        /// 通过CAN(1M波特率)发送一帧数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool can_send(uint id, byte[] data)
        {
            int frame_type_index = 0;
            int send_type_index = 1;//单次发送
            uint result; //发送的帧数

            ZCAN_Transmit_Data can_data = new ZCAN_Transmit_Data();
            can_data.frame.can_id = MakeCanId(id, frame_type_index, 0, 0);
            can_data.frame.data = new byte[8];
            can_data.frame.can_dlc = 8;
            can_data.transmit_type = (uint)send_type_index;
            data.CopyTo(can_data.frame.data, 0);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(can_data));
            Marshal.StructureToPtr(can_data, ptr, true);
            result = Method.ZCAN_Transmit(channel_handle_, ptr, 1);
            Marshal.FreeHGlobal(ptr);


            if (result != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// can发送线程，可选类型为经典can或fdcan
        /// </summary>
        /// <param name="type">0为can，1为fdcan</param>
        private void can_send_thread(uint type)
        {
            if (type == 0)//经典can
            {
                while (is_updating)
                {
                    for (int i = 0; i < MOTOR_NUM; i++)
                    {
                        //can_send(motors[i].id_offset, motors[i].command);
						delayms(0.1);
					}
                }
            }
            else
            {
                while (is_updating)
                {
                    for (int i = 0; i < MOTOR_NUM; i++)
                    {
                        //canfd_send(motors[i].id_offset, motors[i].command);
                        //delayms(0.05);
                    }
                }
            }
            Console.WriteLine("退出线程");
        }
        /// <summary>
        /// canfd队列发送线程
        /// </summary>
        private void canfd_queue_send_thread()
        {
            setQueueSend();
            clearQueueSend();
            ulong frame = 0;
            system_update = 0;
            while (is_updating)
            {
				frame++;
                if (mode_switch_flag != 0)//如果要切换模式
                {
                    for (int i = 0; i < MOTOR_NUM; i++)
                    {
                        canfd_queue[i].transmit_type = 1;
                        canfd_queue[i].frame.len = 8;
                        canfd_queue[i].frame.can_id = motors[i].PARAM_SET_ID;
                        canfd_queue[i].frame.data = new byte[64];
                        canfd_queue[i].frame.flags = 0x11;
                        canfd_queue[i].frame.__res0 = 1;
                        canfd_queue[i].frame.__res1 = 0;
                        if (mode_switch_flag == 1)//切换到MIT模式
                        {
                            motors[i].set_mit_command.CopyTo(canfd_queue[i].frame.data, 0);
                        }
                        else//切换到PV模式
                        {
                            motors[i].set_pv_command.CopyTo(canfd_queue[i].frame.data, 0);
                        }
                    }
                }
                else//不切换模式时
                {
                    for (int i = 0; i < MOTOR_NUM; i++)
                    {
                        canfd_queue[i].transmit_type = 1;
                        canfd_queue[i].frame.len = 8;
                        canfd_queue[i].frame.can_id = motors[i].ID_OFFSET;
                        canfd_queue[i].frame.data = new byte[64];
                        canfd_queue[i].frame.flags = 0x11;
                        canfd_queue[i].frame.__res0 = 1;
                        canfd_queue[i].frame.__res1 = 0;
                        //if (motors[i].current_mode == 1)//为MIT模式
                        //{
                        //    canfd_queue[i].frame.can_id = motors[i].id;
                        //    motors[i].mit_command.CopyTo(canfd_queue[i].frame.data, 0);
                        //}
                        //if (motors[i].current_mode == 2)//为PV模式
                        //{
                        //    canfd_queue[i].frame.can_id = motors[i].id_offset;
                        //    motors[i].pv_command.CopyTo(canfd_queue[i].frame.data, 0);
                        //}
						canfd_queue[i].frame.can_id = motors[i].ID_OFFSET;
						motors[i].Command.CopyTo(canfd_queue[i].frame.data, 0);
						//循环内的东西必须保留不能删掉
					}
                }
                //关节以外的电机不受模式控制指令的影响
                for (int i = MOTOR_NUM; i < SLAVE_NUM; i++)
                {
                    canfd_queue[i].transmit_type = 1;
                    canfd_queue[i].frame.len = 8;
                    canfd_queue[i].frame.can_id = tools[i - MOTOR_NUM].ID_OFFSET;
                    canfd_queue[i].frame.data = new byte[64];
                    canfd_queue[i].frame.flags = 0x11;
                    canfd_queue[i].frame.__res0 = 1;
                    canfd_queue[i].frame.__res1 = 0;
                    tools[i - MOTOR_NUM].Command.CopyTo(canfd_queue[i].frame.data, 0);
                }

                int sizeOfStruct = Marshal.SizeOf(typeof(ZCAN_TransmitFD_Data));
                IntPtr pTransmit = Marshal.AllocHGlobal(sizeOfStruct * (SLAVE_NUM));
                try
                {
                    //这一句比较花时间，一般不加。
                    //IntPtr free_count = Method.ZCAN_GetValue(device_handle_, "0/get_device_available_tx_count/1");
                    //int fr = Marshal.ReadInt32(free_count);
                    for (int i = 0; i < SLAVE_NUM; i++)
                    {
                        IntPtr ptr = IntPtr.Add(pTransmit, i * sizeOfStruct);
                        Marshal.StructureToPtr(canfd_queue[i], ptr, true);
                    }
                    uint retFd;
                    if (frame % tool_update == 0)
                    {
                        retFd = Method.ZCAN_TransmitFD(channel_handle_, pTransmit, SLAVE_NUM);
                        send_err_num = send_err_num + SLAVE_NUM - retFd;
                    }
                    else
                    {
                        retFd = Method.ZCAN_TransmitFD(channel_handle_, pTransmit, MOTOR_NUM);
                        send_err_num = send_err_num + MOTOR_NUM - retFd;
                    }
                    send_suc_num = send_suc_num + retFd;
                    system_update = frame;
                }
                finally
                {
                    Marshal.FreeHGlobal(pTransmit);
                }
            }
        }

        /// <summary>
        /// CAN总线信息刷新线程
        /// </summary>
        private void can_param_update_thread()
        {
            System.Diagnostics.Stopwatch stopTime = new System.Diagnostics.Stopwatch();
            ulong recv_num_before = 0, send_suc_num_before = 0, send_err_num_before = 0,
                recv_num_in_time = 0, send_suc_num_in_time = 0, send_err_num_in_time = 0,
                system_update_before = 0, system_update_in_time = 0;
            while (is_updating)
            {
                recv_num_before = recv_num;
                send_suc_num_before = send_suc_num;
                send_err_num_before = send_err_num;
                system_update_before = system_update;
                stopTime.Start();
                while (stopTime.Elapsed.TotalMilliseconds < can_param_time)
                {

                }
                stopTime.Stop();
                stopTime.Reset();
                recv_num_in_time = recv_num - recv_num_before;
                send_suc_num_in_time = send_suc_num - send_suc_num_before;
                send_err_num_in_time = send_err_num - send_err_num_before;
                system_update_in_time = system_update - system_update_before;
				can_param[0] = ((double)(recv_num_in_time) / (double)can_param_time) * 1000;
                //发送与接收一帧，加上3bit的帧间隔是49.4us，计算这段时间内总线有多长时间被占用即可估算总线负载率
                can_param[1] = (double)(recv_num_in_time + send_suc_num_in_time + send_err_num_in_time) * 49.4 / 1000 / can_param_time * 100;
				can_param[2] = send_suc_num;
				can_param[3] = send_err_num;
                can_param[4] = recv_num;
                can_param[5] = system_update;
                can_param[6] = (double)system_update_in_time / (double)can_param_time * 1000;
			}
            send_suc_num = 0;
            send_err_num = 0;
            recv_num = 0;
		}


		public void start_can_thread(uint type)
        {
            is_updating = true;
            if (type == 0)//为CAN
            {
                can_recv_trd = new Thread(can_receive_thread);
				can_recv_trd.Name = "can_receive_thread";
			}
            else//为CANFD
            {
                can_recv_trd = new Thread(canfd_receive_thread);
				can_recv_trd.Name = "canfd_receive_thread";
			}
			can_recv_trd.IsBackground = true;
            can_recv_trd.Start();

            //can_send_trd = new Thread(() => can_send_thread(type));
            can_send_trd = new Thread(canfd_queue_send_thread);
            can_send_trd.Name = "canfd_queue_send_thread";
			can_send_trd.IsBackground = true;
            can_send_trd.Start();

            can_param_update_trd = new Thread(can_param_update_thread);
            can_param_update_trd.Name = "can_param_update_thread";
			can_param_update_trd.IsBackground = true;
            can_param_update_trd.Start();
        }
        private void can_receive_thread()
        {
            const int TYPE_CAN = 0;
            const int TYPE_CANFD = 1;
            ZCAN_Receive_Data[] can_data = new ZCAN_Receive_Data[10000];
            uint len = 0;
            byte[] data = new byte[64];
            while (is_updating)
            {
                len = Method.ZCAN_GetReceiveNum(channel_handle_, TYPE_CAN);//CAN收到东西了
                if (len > 0)
                {
                    int size = Marshal.SizeOf(typeof(ZCAN_Receive_Data));
                    IntPtr ptr = Marshal.AllocHGlobal((int)100 * size);
                    len = Method.ZCAN_Receive(channel_handle_, ptr, 100, 50);

                    can_data[0] = (ZCAN_Receive_Data)Marshal.PtrToStructure(
                        (IntPtr)((Int64)ptr + 0 * size), typeof(ZCAN_Receive_Data));
                    can_data_proc(can_data[0]);
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
        private void canfd_receive_thread()
        {
            const int TYPE_CAN = 0;
            const int TYPE_CANFD = 1;
            ZCAN_ReceiveFD_Data[] canfd_data = new ZCAN_ReceiveFD_Data[10000];
            uint len = 0;
            byte[] data = new byte[64];
            while (is_updating)
            {
                len = Method.ZCAN_GetReceiveNum(channel_handle_, TYPE_CANFD);//CANFD收到东西了
                if (len > 0)
                {
                    int size = Marshal.SizeOf(typeof(ZCAN_ReceiveFD_Data));
                    IntPtr ptr = Marshal.AllocHGlobal((int)100 * size);
                    len = Method.ZCAN_ReceiveFD(channel_handle_, ptr, 100, 50);
                    for (int i = 0; i < len; i++)
                    {
                        canfd_data[i] = (ZCAN_ReceiveFD_Data)Marshal.PtrToStructure(
                            (IntPtr)((Int64)ptr + i * size), typeof(ZCAN_ReceiveFD_Data));


                        if (!canfd_data_proc(canfd_data[i]))//如果收到的不是电机反馈信号
                        {
                            //Console.WriteLine(canfd_data[i].frame.can_id.ToString("X4"));
                        }
                        else
                        {
							//Console.WriteLine("CANFD接收！" + canfd_data[i].frame.can_id.ToString("X2"));
						}
						//Console.WriteLine("CANFD接收！" + canfd_data[i].frame.can_id.ToString("X2"));
						//Console.WriteLine("CANFD接收！" + len+" "+i);
					}
					Marshal.FreeHGlobal(ptr);
				}
                
			}
        }
        /// <summary>
        /// 根据电机ID的实际情况来修改内容
        /// </summary>
        /// <param name="canfd_data"></param>
        /// <returns></returns>
        private bool canfd_data_proc(ZCAN_ReceiveFD_Data canfd_data)
        {
            recv_num++;
            if (mode_switch_flag != 0)//如果发送了全电机模式切换指令
            {
                if (canfd_data.frame.data[0] - 1 < MOTOR_NUM)//只有在电机失能或读写参数时会出现data[0]=id的情况
                {
                    if (motors[canfd_data.frame.data[0] - 1].get_motor_mode(canfd_data.frame.data))//如果成功读取到模式
                    {
                        motor_mode[canfd_data.frame.data[0] - 1] = motors[canfd_data.frame.data[0] - 1].Mode;
                        if (motor_mode.All(v => v == motor_mode[0]))//如果所有电机模式相同，判断为模式切换完成
                        {
                            mode_switch_flag = 0;//模式切换完成
                            Console.WriteLine("模式切换完成");
                        }
                    }
                }
                return true;
            }
			if ((canfd_data.frame.can_id >= 0x11) && (canfd_data.frame.can_id <= 0x16))
            {
				motors[(canfd_data.frame.data[0] & 0x0F) - 1].read_motor(canfd_data.frame.data);
                return true;
            }
            else if (canfd_data.frame.can_id == 0x17)
            {
                tools[0].read_motor(canfd_data.frame.data);
            }
			else if (canfd_data.frame.can_id == 0x31)
            {
				//预留的其他ID情况
				return true;
			}
            return false;
        }
        private void can_data_proc(ZCAN_Receive_Data can_data)
        {
            if ((can_data.frame.can_id >= 0x10) && (can_data.frame.can_id <= 0x16))
            {
                motors[(can_data.frame.data[0] & 0x0F) - 1].read_motor(can_data.frame.data);
            }
        }
        /// <summary>
        /// 停止所有收发线程
        /// </summary>
        public void stop_can()
        {
            is_updating = false;
            if ((can_recv_trd == null) || (can_send_trd == null) || (can_param_update_trd == null))
            {
                return;
            }
            //如果有任何收发线程运行，就停止所有收发线程
            while (can_recv_trd.IsAlive || can_send_trd.IsAlive || can_param_update_trd.IsAlive)
            {
                is_updating = false;
                //先停掉所有收发线程
            }
        }

        /// <summary>
        /// 在接收线程停止的状态下(会先停止收发线程)等待CAN总线设备回复
        /// </summary>
        /// <param name="can_index"></param>
        /// <param name="timeout">超时等待时间，单位为毫秒</param>
        /// <returns>返回收到的数据，仅限一帧9byte，最后一byte是帧ID。超时没收到就返回null</returns>
        private byte[] wait_for_rec(double timeout)
        {
			const int TYPE_CAN = 0;
			const int TYPE_CANFD = 1;
			ZCAN_Receive_Data[] can_data = new ZCAN_Receive_Data[10000];
			ZCAN_ReceiveFD_Data[] canfd_data = new ZCAN_ReceiveFD_Data[10000];
			uint len = 0;

            byte[] data_recv = new byte[8];
            byte[] result = new byte[9];

			System.Diagnostics.Stopwatch stopTime = new System.Diagnostics.Stopwatch();
            stopTime.Reset();
            stopTime.Start();
            while (stopTime.Elapsed.TotalMilliseconds <= timeout)
            {
                len = Method.ZCAN_GetReceiveNum(channel_handle_, TYPE_CAN);//CAN收到东西了
                if (len > 0)
                {
                    int size = Marshal.SizeOf(typeof(ZCAN_Receive_Data));
                    IntPtr ptr = Marshal.AllocHGlobal((int)100 * size);
                    len = Method.ZCAN_Receive(channel_handle_, ptr, 100, 50);

                    can_data[0] = (ZCAN_Receive_Data)Marshal.PtrToStructure(
                        (IntPtr)((Int64)ptr + 0 * size), typeof(ZCAN_Receive_Data));
					for (int i = 0; i < 8; i++)
					{
						result[i] = can_data[0].frame.data[i];
					}
					result[8] = (byte)can_data[0].frame.can_id;
					Marshal.FreeHGlobal(ptr);
					return result;
				}

                len = Method.ZCAN_GetReceiveNum(channel_handle_, TYPE_CANFD);//CANFD收到东西了
                if (len > 0)
                {
                    int size = Marshal.SizeOf(typeof(ZCAN_ReceiveFD_Data));
                    IntPtr ptr = Marshal.AllocHGlobal((int)100 * size);
                    len = Method.ZCAN_ReceiveFD(channel_handle_, ptr, 100, 50);
                    //for (int i = 0; i < len; ++i)
                    //{
                    canfd_data[0] = (ZCAN_ReceiveFD_Data)Marshal.PtrToStructure(
                        (IntPtr)((Int64)ptr + 0 * size), typeof(ZCAN_ReceiveFD_Data));
                    for (int i = 0; i < 8; i++)
                    {
                        result[i] = canfd_data[0].frame.data[i];
                    }
                    result[8] = (byte)canfd_data[0].frame.can_id;
					Marshal.FreeHGlobal(ptr);
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 发送一帧数据，然后等待CAN总线反馈
        /// </summary>
        /// <param name="type">0为can，1为canfd</param>
        /// <param name="motor_id">从机ID</param>
        /// <param name="send_data">发送的数据</param>
        /// <param name="timeout">超时设定，单位为毫秒</param>
        /// <returns>返回收到的数据，9byte，最后1byte为帧id。超时没收到就返回null</returns>
        public byte[] send_wait(uint type, uint motor_id, byte[] send_data, double timeout)
        {
            stop_can();
            clearRecvBuffer();
            if (type == 0)
            {
                can_send(motor_id, send_data);
            }
            else
            {
                canfd_send(motor_id, send_data);
            }
            return wait_for_rec(timeout);
        }
		public void enable_all()
        {
            delayms(5);
			stop_can();
            byte[] data;
			for (int i = 0; i < MOTOR_NUM; i++)
			{
                data = send_wait(1, motors[i].ID, DMMotor.clear_error_command, 5);
                motors[i].read_motor(data);
				data = send_wait(1, motors[i].ID, DMMotor.enable_command, 5);
				motors[i].read_motor(data);
			}
			for (int i = 0; i < TOOL_NUM; i++)
			{
				data = send_wait(1, tools[i].ID, DMMotor.clear_error_command, 5);
				tools[i].read_motor(data);
				data = send_wait(1, tools[i].ID, DMMotor.enable_command, 5);
				tools[i].read_motor(data);
			}
		}
		public void disable_all()
		{
			stop_can();
			byte[] data;
			for (int i = 0; i < MOTOR_NUM; i++)
			{
				//canfd_send(motors[i].ID, motors[i].disable_command);
				//delayms(5);
				data = send_wait(1, motors[i].ID, DMMotor.disable_command, 5);
				motors[i].read_motor(data);
			}
			for (int i = 0; i < TOOL_NUM; i++)
			{
				//canfd_send(tools[i].ID, tools[i].disable_command);
				//delayms(5);
				data = send_wait(1, tools[i].ID, DMMotor.disable_command, 5);
				tools[i].read_motor(data);
			}
		}
        public void set_zero(uint id)
        {
            stop_can();
            byte[] data;
            data = send_wait(1, id, DMMotor.set_zero_command, 5);
            if (id > MOTOR_NUM)
            {
                tools[id - MOTOR_NUM - 1].read_motor(data);
                tools[id - MOTOR_NUM - 1].set_empty_command();
                data = send_wait(1, id, tools[id - MOTOR_NUM - 1].Command, 5);
				tools[id - MOTOR_NUM - 1].read_motor(data);
			}
            else
            {
                motors[id - 1].read_motor(data);
                motors[id - 1].set_empty_command();
                data = send_wait(1, id, motors[id - 1].Command, 5);
				motors[id - 1].read_motor(data);
			}
        }
        public bool get_status_all()//读取所有电机的模式
        {
            for (int i = 0; i < MOTOR_NUM; i++)
            {
                motor_mode[i] = (byte)i;
            }
            byte[] data;
            for (int i = 0; i < MOTOR_NUM; i++)
            {
                data = send_wait(1, 0x7FF, motors[i].get_mode_command, 5);
                if (motors[i].get_motor_mode(data))
                {
                    motor_mode[i] = motors[i].Mode;
                }
                else
                {
                    return false;
                }
            }
            if (!motor_mode.All(v => v == motor_mode[0]))
            {
                return false;
            }
            for (int i = 0; i < MOTOR_NUM; i++)
            {
                data = null;
                motors[i].set_empty_command();
                data = send_wait(1, motors[i].ID, motors[i].Command, 5);
                if (data == null)
                {
                    return false;
                }
                motors[i].read_motor(data);
            }
            for (int i = 0; i < TOOL_NUM; i++)
            {
				data = send_wait(1, 0x7FF, tools[i].get_mode_command, 5);
				if (!tools[i].get_motor_mode(data))
				{
					//return false;
				}
				tools[i].set_empty_command();
				data = send_wait(1, tools[i].ID, tools[i].Command, 5);
				if (data == null)
				{
					//return false;
				}
				tools[i].read_motor(data);
			}
            return true;
        }

		/// <summary>
		/// 在不持续发送的情况下设置所有电机的模式
		/// </summary>
		/// <param name="mode">1为MIT，2为PV</param>
		/// <returns>返回是否成功</returns>
		public bool set_mode_all(byte mode)
        {
            byte[] data;
            switch (mode)
            {
                case 1:
                    {
						for (int i = 0; i < MOTOR_NUM; i++)
						{
                            data = send_wait(1, 0x7FF, motors[i].set_mit_command, 50);
							motors[i].get_motor_mode(data);
                            motor_mode[i] = motors[i].Mode;
						}
						break;
                    }
                case 2:
                    {
						for (int i = 0; i < MOTOR_NUM; i++)
						{
							data = send_wait(1, 0x7FF, motors[i].set_pv_command, 50);
							motors[i].get_motor_mode(data);
							motor_mode[i] = motors[i].Mode;
						}
                        break;
					}
                default:
                    {
                        return false;
                    }
            }
            if (motor_mode.All(v => v == motor_mode[0]))
            {
                return true;
            }
            return false;
		}
		public static double delayms(double time)       //手动设定延迟
        {
            if (time == 0)
            {
                return 0;
            }
            System.Diagnostics.Stopwatch stopTime = new System.Diagnostics.Stopwatch();

            stopTime.Start();
            while (stopTime.Elapsed.TotalMilliseconds < time)
            {
            }
            stopTime.Stop();
            stopTime.Reset();

            return stopTime.Elapsed.TotalMilliseconds;
        }

		#region ZLG设置函数
        /// <summary>
        /// 清除接收缓冲区
        /// </summary>
        /// <returns></returns>
		public bool clearRecvBuffer()
		{
			if (Method.ZCAN_ClearBuffer(channel_handle_) != Define.STATUS_OK)
			{
				return false;
			}
			return true;
		}
		/// <summary>
		/// 设置队列发送
		/// </summary>
		public void setQueueSend()
        {
			string path = channel_index_ + "/set_send_mode";
			string value = "1";
            Method.ZCAN_SetValue(device_handle_, path, Encoding.ASCII.GetBytes(value));
        }
		/// <summary>
		/// 清空队列发送缓存
		/// </summary>
		public void clearQueueSend()
		{
			string path = channel_index_ + "/clear_delay_send_queue";
			string value = "0";
			Method.ZCAN_SetValue(device_handle_, path, Encoding.ASCII.GetBytes(value));
		}
		private uint MakeCanId(uint id, int eff, int rtr, int err)//1:extend frame 0:standard frame
        {
            uint ueff = (uint)(!!(Convert.ToBoolean(eff)) ? 1 : 0);
            uint urtr = (uint)(!!(Convert.ToBoolean(rtr)) ? 1 : 0);
            uint uerr = (uint)(!!(Convert.ToBoolean(err)) ? 1 : 0);
            return id | ueff << 31 | urtr << 30 | uerr << 29;
        }
        //设置CANFD标准
        private bool setCANFDStandard(int canfd_standard)
        {
            string path = channel_index_ + "/canfd_standard";
            string value = canfd_standard.ToString();
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            uint ret = Method.ZCAN_SetValue(device_handle_, path, Encoding.ASCII.GetBytes(value));
            return (ret == 1);
        }
        private bool setFdBaudrate(UInt32 abaud, UInt32 dbaud)
        {
            string path = channel_index_ + "/canfd_abit_baud_rate";
            string value = abaud.ToString();
            if (1 != Method.ZCAN_SetValue(device_handle_, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            path = channel_index_ + "/canfd_dbit_baud_rate";
            value = dbaud.ToString();
            if (1 != Method.ZCAN_SetValue(device_handle_, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            return true;
        }
        //设置终端电阻使能
        private bool setResistanceEnable(bool enable)
        {
            string path = channel_index_ + "/initenal_resistance";
            string value = (enable ? "1" : "0");
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            return 1 == Method.ZCAN_SetValue(device_handle_, path, Encoding.ASCII.GetBytes(value));
        }

        //设置自定义波特率, 需要从CANMaster目录下的baudcal生成字符串
        private bool setCustomBaudrate(string ABIT)
        {
            string path = channel_index_ + "/baud_rate_custom";
            string baudrate = ABIT;
            return 1 == Method.ZCAN_SetValue(device_handle_, path, Encoding.ASCII.GetBytes(baudrate));
        }

        private bool setFilter()
        {
            string path = channel_index_ + "/filter_clear";//清除滤波
            string value = "0";

            if (0 == Method.ZCAN_SetValue(device_handle_, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}

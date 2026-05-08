using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;
using System.IO;

namespace DMArmDLL
{
    #region USBCAN预设部分
    /*------------兼容ZLG的数据类型---------------------------------*/

    //1.ZLGCAN系列接口卡信息的数据类型。
    //public struct VCI_BOARD_INFO 
    //{ 
    //    public UInt16 hw_Version;
    //    public UInt16 fw_Version;
    //    public UInt16 dr_Version;
    //    public UInt16 in_Version;
    //    public UInt16 irq_Num;
    //    public byte   can_Num;
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)] public byte []str_Serial_Num;
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    //    public byte[] str_hw_Type;
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    //    public byte[] Reserved;
    //}

    //以下为简易定义与调用方式，在项目属性->生成->勾选使用不安全代码即可
    unsafe public struct VCI_BOARD_INFO//使用不安全代码
    {
        public UInt16 hw_Version;
        public UInt16 fw_Version;
        public UInt16 dr_Version;
        public UInt16 in_Version;
        public UInt16 irq_Num;
        public byte can_Num;

        public fixed byte str_Serial_Num[20];
        public fixed byte str_hw_Type[40];
        public fixed byte Reserved[8];
    }

    //2.定义CAN信息帧的数据类型。
    unsafe public struct VCI_CAN_OBJ  //使用不安全代码
    {
        public uint ID;
        public uint TimeStamp;        //时间标识
        public byte TimeFlag;         //是否使用时间标识
        public byte SendType;         //发送标志。保留，未用
        public byte RemoteFlag;       //是否是远程帧
        public byte ExternFlag;       //是否是扩展帧
        public byte DataLen;          //数据长度
        public fixed byte Data[8];    //数据
        public fixed byte Reserved[3];//保留位

    }

    //3.定义初始化CAN的数据类型
    public struct VCI_INIT_CONFIG
    {
        public UInt32 AccCode;
        public UInt32 AccMask;
        public UInt32 Reserved;
        public byte Filter;   //0或1接收所有帧。2标准帧滤波，3是扩展帧滤波。
        public byte Timing0;  //波特率参数，具体配置，请查看二次开发库函数说明书。
        public byte Timing1;
        public byte Mode;     //模式，0表示正常模式，1表示只听模式,2自测模式
        //1Mbps的波特率，Timing0=0x00,Timing1=0x14
    }

    /*------------其他数据结构描述---------------------------------*/
    //4.USB-CAN总线适配器板卡信息的数据类型1，该类型为VCI_FindUsbDevice函数的返回参数。
    public struct VCI_BOARD_INFO1
    {
        public UInt16 hw_Version;
        public UInt16 fw_Version;
        public UInt16 dr_Version;
        public UInt16 in_Version;
        public UInt16 irq_Num;
        public byte can_Num;
        public byte Reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] str_Serial_Num;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] str_hw_Type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] str_Usb_Serial;
    }

    /*------------数据结构描述完成---------------------------------*/

    public struct CHGDESIPANDPORT
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] szpwd;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] szdesip;
        public Int32 desport;

        public void Init()
        {
            szpwd = new byte[10];
            szdesip = new byte[20];
        }
    }
    #endregion
    public class USBCAN
    {
        #region USBCAN预定义部分不要修改
        const int DEV_USBCAN = 3;
        const int DEV_USBCAN2 = 4;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceType"></param>
        /// <param name="DeviceInd"></param>
        /// <param name="Reserved"></param>
        /// <returns></returns>
        #region DLLImport
        /*------------兼容ZLG的函数描述---------------------------------*/
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);

        /*------------其他函数描述---------------------------------*/

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_FindUsbDevice2(ref VCI_BOARD_INFO pInfo);
        /*------------函数描述结束---------------------------------*/
        #endregion DLLImport
        #endregion
        private static uint device_type = 4;//USBCAN2
        private static uint device_index = 0;
        private bool is_open, is_updating;
        VCI_CAN_OBJ[] can_frame_rec = new VCI_CAN_OBJ[3000];
        private int frame_num = 500;
        public double[] can_fps = new double[2];
        private double send_interval = 0.2;

        private Thread[] can_rec_trd = new Thread[2];
        private Thread[] can_send_trd = new Thread[2];

        public Motor[] motors = new Motor[6];
        #region 开放外界读取参数
        public bool IsOpen
        { get { return is_open; } }
        public bool IsUpdating
        { get { return is_updating; } }
        #endregion

        public USBCAN()
        {
            is_open = false;
            is_updating = false;
            for (uint i = 0; i < 3; i++)
            {
                motors[i] = new Motor(i + 1, 4340, "PV");
            }
            for (uint i = 3; i < 6; i++)
            {
                motors[i] = new Motor(i + 1, 4310, "PV");
            }
            motors[1].angle_lim = new double[2] { 0, 213 * Math.PI / 180 };
            motors[2].angle_lim = new double[2] { 0, 162 * Math.PI / 180 };
            motors[4].angle_lim = new double[2] { -84 * Math.PI / 180, 98 * Math.PI / 180 };

            can_rec_trd = new Thread[2];
            can_send_trd = new Thread[2];
        }
        #region USB转CAN相关
        /// <summary>
        /// 开启USB转CAN设备
        /// </summary>
        /// <returns>是否成功</returns>
        public bool open_device()
        {
            stop_can();
            if (!is_open)//如果此时没有打开
            {
                is_open = (VCI_OpenDevice(device_type, device_index, 0) == 1);
            }
            return is_open;
        }
        /// <summary>
        /// 关闭USB转CAN设备
        /// </summary>
        /// <returns>是否成功</returns>
        public bool close_device()
        {
            stop_can();
            uint close_device = 1;
            if (is_open == true)//如果打开了才执行
            {
                close_device = VCI_CloseDevice(device_type, device_index);
                if (close_device == 1)
                {
                    is_open = false;
                }
            }
            return close_device == 1;
        }

        /// <summary>
        /// 初始化一路CAN总线，参数为第几路CAN，双路CAN的话ID应为0或1，每一路需要各自调用
        /// </summary>
        /// <param name="id">第几路CAN，双路CAN的话ID应为0或1</param>
        /// <returns>返回1表示成功，0表示失败</returns>
        public bool init_single_can(uint can_index)
        {
            if (is_open == false)//如果设备没打开
            {
                return false;
            }
            stop_can();
            VCI_INIT_CONFIG init_config = new VCI_INIT_CONFIG();
            init_config.AccCode = 0x00000000;
            init_config.AccMask = 0xFFFFFFFF;//接收所有ID
            init_config.Timing0 = 0x00;
            init_config.Timing1 = 0x14;//Timing0和Timing1这样设定表示波特率为1M
            init_config.Filter = 1;//接收所有类型
            init_config.Mode = 0;//正常工作模式

            uint init_flag = VCI_InitCAN(device_type, device_index, can_index, ref init_config);
            return init_flag == 1;
        }

        public bool start_single_can(uint can_index)
        {
            stop_can();
            if (VCI_StartCAN(device_type, device_index, can_index) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool init_double_can()
        {
            bool tempflag = init_single_can(0) && init_single_can(1);
            return tempflag;
        }
        public bool start_double_can()
        {
            bool tempflag = start_single_can(0) && start_single_can(1);
            return tempflag;
        }
        /// <summary>
        /// 停止所有收发线程
        /// </summary>
        public void stop_can()
        {
            is_updating = false;
            for (int i = 0; i < 2; i++)
            {
                if ((can_rec_trd[i] == null) || (can_send_trd[i] == null))
                {
                    return;
                }
            }
            //如果有任何收发线程运行，就停止所有收发线程
            while ((can_rec_trd[0].IsAlive
                || can_rec_trd[1].IsAlive
                || can_send_trd[0].IsAlive
                || can_send_trd[1].IsAlive))
            {
                is_updating = false;
                //先停掉所有收发线程
            }
            delayms(5);

        }
        public void start_can_thread()
        {
            stop_can();
            is_updating = true;
            can_rec_trd[0] = new Thread(() => can_receive_thread(0));//如果这个写进循环里可能导致index=2。不知道为什么。
            can_rec_trd[1] = new Thread(() => can_receive_thread(1));
            can_send_trd[0] = new Thread(() => can_send_thread(0));
            can_send_trd[1] = new Thread(() => can_send_thread(1));
            for (uint i = 0; i < 2; i++)
            {
                can_rec_trd[i].IsBackground = true;
                can_rec_trd[i].Start();
            }
            delayms(10);//先开启接收线程
            for (uint i = 0; i < 2; i++)
            {
                can_send_trd[i].IsBackground = true;
                can_send_trd[i].Start();
            }
        }
        /// <summary>
        /// 启动某一路CAN总线的接收进程
        /// </summary>
        /// <param name="can_index"></param>
        unsafe private void can_receive_thread(uint can_index)
        {
            byte[] data = new byte[8];
            int listViewIndex = 0;
            System.Diagnostics.Stopwatch stopTime = new System.Diagnostics.Stopwatch();
            while (is_updating)
            {
                uint res = 0;
                res = VCI_Receive(device_type, device_index, can_index, ref can_frame_rec[0], 2500, 0);
                if (res != 0)
                {
                    for (UInt32 i = 0; i < res; i++)
                    {
                        // 使用 Marshal 复制数据
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)));
                        try
                        {
                            Marshal.StructureToPtr(can_frame_rec[i], ptr, false);

                            // 获取指向 Data 字段的指针
                            IntPtr dataPtr = (IntPtr)((long)ptr + Marshal.OffsetOf(typeof(VCI_CAN_OBJ), "Data").ToInt64());

                            // 将 Data 字段中的数据复制到 byte[] 数组中
                            Marshal.Copy(dataPtr, data, 0, 8);
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(ptr); // 释放非托管内存
                        }
                        if (listViewIndex > int.MaxValue - 1)//防止溢出
                        {
                            listViewIndex = 0;
                        }
                        if (listViewIndex % frame_num == 0)//每接收到frame_num数量的反馈帧就计时一次，计算帧率
                        {
                            can_fps[can_index] = frame_num / stopTime.Elapsed.TotalMilliseconds * 1000;
                            stopTime.Stop();
                            stopTime.Reset();
                            stopTime.Start();
                        }
                        listViewIndex++; //索引值计数
                        //Console.WriteLine(data[0]&0x0F);
                        if ((can_frame_rec[i].ID <= 0x016) && (can_frame_rec[i].ID >= 0x011))//达妙电机反馈帧ID为Master ID，已设定为电机ID|0x010
                        {
                            int MotorID = data[0] & 0x0F;
                            motors[MotorID - 1].read_motor(data);
                            //for (int j = 0; j < 8; j++)
                            //{
                            //    Console.Write(data[j].ToString("X2").PadLeft(4));
                            //}
                            //Console.Write(can_fps[0].ToString("0.00").PadLeft(10));
                            //Console.WriteLine();
                        }
                        if ((can_frame_rec[i].ID >= 0x141))//夹钳用的RMD电机反馈帧为电机自身的ID，且挂载在第二路CAN
                        {
                            //for (int j = 0; j < 8; j++)
                            //{
                            //    Console.Write(data[j].ToString("X2").PadLeft(4));
                            //}
                            //Console.Write(can_fps[0].ToString("0.00").PadLeft(10));
                            //Console.WriteLine();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 通过CAN总线发送一帧数据
        /// </summary>
        /// <param name="can_index">第几路CAN</param>
        /// <param name="motor_id">电机(从机)id</param>
        /// <param name="data"></param>
        unsafe public void can_send(uint can_index, uint motor_id, byte[] data)
        {
            if (is_open == false)
            {
                return;
            }
            uint index = can_index;
            VCI_CAN_OBJ can_frame_send = new VCI_CAN_OBJ();
            can_frame_send.RemoteFlag = 0;//不是远程帧
            can_frame_send.ExternFlag = 0;//不是拓展帧
            can_frame_send.ID = motor_id;
            can_frame_send.DataLen = 8;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)));
            try
            {
                Marshal.StructureToPtr(can_frame_send, ptr, false);

                // 获取指向 Data 字段的指针
                IntPtr dataPtr = (IntPtr)((long)ptr + Marshal.OffsetOf(typeof(VCI_CAN_OBJ), "Data").ToInt64());

                // 将 byte[] data 复制到 Data 字段
                Marshal.Copy(data, 0, dataPtr, data.Length);

                // 将非托管内存中的数据拷贝回托管对象
                can_frame_send = (VCI_CAN_OBJ)Marshal.PtrToStructure(ptr, typeof(VCI_CAN_OBJ));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr); // 释放非托管内存
            }
            VCI_Transmit(device_type, device_index, index, ref can_frame_send, 1);
        }
        /// <summary>
        /// 开启某一路CAN总线的发送线程，会持续一直发送
        /// </summary>
        /// <param name="can_index"></param>
        private void can_send_thread(uint can_index)
        {
            
            byte[] data = new byte[8] { 0xA0, 0, 0, 0, 0, 0, 0, 0 };
            while (is_updating)
            {
                if (can_index == 0)
                {
                    for (uint i = 0; i < 6; i++)
                    {
                        can_send(can_index, motors[i].id_offset, motors[i].command);
                        //can_send(can_index,0x143, data);
                        delayms(send_interval);

                    }
                }
                else
                {
                    for (uint i = 3; i < 6; i++)
                    {
                        //can_send(can_index, motors[i].id_offset, motors[i].command);
                        //delayms(send_interval);
                    }
                }
            }
        }
        /// <summary>
        /// 在接收线程停止的状态下(会先停止收发线程)等待CAN总线设备回复
        /// </summary>
        /// <param name="can_index"></param>
        /// <param name="timeout">超时等待时间，单位为毫秒</param>
        /// <returns>返回收到的数据，仅限一帧9byte，最后一byte是帧ID。超时没收到就返回null</returns>
        private byte[] wait_for_rec(uint can_index, double timeout)
        {
            stop_can();
            VCI_CAN_OBJ[] can_frame_rec = new VCI_CAN_OBJ[3000];
            byte[] can_rec = new byte[9];
            System.Diagnostics.Stopwatch stopTime = new System.Diagnostics.Stopwatch();
            stopTime.Reset();
            stopTime.Start();
            while (stopTime.Elapsed.TotalMilliseconds <= timeout)
            {
                uint res;
                res = VCI_Receive(device_type, device_index, can_index, ref can_frame_rec[0], 2500, 0);
                if (res != 0)
                {
                    for (UInt32 i = 0; i < res; i++)
                    {
                        // 使用 Marshal 复制数据
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)));
                        try
                        {
                            Marshal.StructureToPtr(can_frame_rec[i], ptr, false);

                            // 获取指向 Data 字段的指针
                            IntPtr dataPtr = (IntPtr)((long)ptr + Marshal.OffsetOf(typeof(VCI_CAN_OBJ), "Data").ToInt64());

                            // 将 Data 字段中的数据复制到 byte[] 数组中
                            Marshal.Copy(dataPtr, can_rec, 0, 8);
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(ptr); // 释放非托管内存
                        }
                        for (int j = 0; j < 8; j++)
                        {
                            Console.Write(can_rec[j].ToString("X2") + " ");
                        }
                        Console.WriteLine();
                        Console.WriteLine(res);
                        can_rec[8] = (byte)can_frame_rec[i].ID;
                        return can_rec;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 发送一帧数据，然后等待CAN总线反馈
        /// </summary>
        /// <param name="can_index">总线索引</param>
        /// <param name="motor_id">从机ID</param>
        /// <param name="send_data">发送的数据</param>
        /// <param name="timeout">超时设定，单位为毫秒</param>
        /// <returns>返回收到的数据，9byte，最后1byte为帧id。超时没收到就返回null</returns>
        public byte[] send_wait(uint can_index, uint motor_id, byte[] send_data, double timeout)
        {
            stop_can();
            can_send(can_index, motor_id, send_data);
            return wait_for_rec(can_index, timeout);
        }
        #endregion

        public void enable_all()
        {
            stop_can();
            for (int i = 0; i < 6; i++)
            {
                can_send(0, motors[i].id_offset, motors[i].clear_error_command);
                delayms(5);
                can_send(0, motors[i].id_offset, motors[i].enable_command);
                delayms(5);
            }
        }
        public void disable_all()
        {
            stop_can();
            for (int i = 0; i < 6; i++)
            {
                can_send((motors[i].id - 1) / 3, motors[i].id_offset, motors[i].disable_command);
                delayms(5);
            }
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
/*
 * 20250312更新：	加入is_bound与Bound属性以识别udp是否成功绑定
 * 20250314更新：	加入wait_for_rec函数;
 *					将UdpDataRecv.set中，fromIP等三行挪到最前;
 *					接收时加入线程锁;
 */
namespace DMArmDLL
{
	public struct UdpDataRecv
	{
		public byte[] data;
		public string fromIP;
		public int fromPort;
		public long time;
		public void set(byte[] Data, string IP, int Port, long Time)
		{
			fromIP = IP;
			fromPort = Port;
			time = Time;
			if (Data != null)
			{
				data = new byte[Data.Length];
				Data.CopyTo(data, 0);
			}
			else
			{
				data = null;
			}
		}
	}
	public struct UdpProtocolFrame
	{
		public double[] q;
		public byte mode;
		public double[] grip;
		public uint crc;

		public void set(double[] Q, byte Mode, double[] Grip, uint Crc)
		{
			q = new double[6];
			grip = new double[3];
			Array.Copy(Q, q, q.Length);
			Array.Copy(Grip, grip, grip.Length);
			mode = Mode;
			crc = Crc;
		}
	}
	/// <summary>
	/// 建立一个UDP类，LocalIP和Port表示该UDP节点的IP与端口，另一程序此节点发送数据时需填入该IP与端口
	/// </summary>
	public class UdpClass
	{
		public const int ProtocolFrameLength = 85;
		public const int ProtocolDataLength = 73;
		private const int ProtocolDataOffset = 4;
		private const int ProtocolCrcOffset = 77;
		private const int ProtocolFooterOffset = 81;
		private static readonly byte[] ProtocolHeader = new byte[] { 0x55, 0xAA, 0x55, 0xAA };
		private static readonly byte[] ProtocolFooter = new byte[] { 0x0D, 0x0A, 0x0D, 0x0A };
		private static readonly uint[] Crc32Table = build_crc32_table();

		private string localIp;
		private int localPort;
		private UdpClient udpClient;
		private IPEndPoint remotePoint;
		public UdpDataRecv data_recv;
		private Thread trdRecv;
		private bool is_receiving, is_bound;

		static uint IOC_IN = 0x80000000;
		static uint IOC_VENDOR = 0x18000000;
		uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

		public int LocalPort
		{
			get { return localPort; }
		}
		public bool Bound
		{
			get
			{
				return is_bound;
			}
		}
		public UdpClass(string LocalIP, int LocalPort)
		{
			localIp = LocalIP;
			localPort = LocalPort;
			data_recv.set(null, "", 0, 0);
			is_receiving = false;
			is_bound = false;


		}
		public UdpClass(int LocalPort)
		{
			localIp = GetLocalIp();
			localPort = LocalPort;
			data_recv.set(null, "", 0, 0);
			is_receiving = false;
			is_bound = false;
		}
		/// <summary>
		/// 以本地ip与端口，创建一个udp节点
		/// </summary>
		/// <returns></returns>
		public bool bind()
		{
			IPAddress ip = IPAddress.Parse(localIp);
			IPEndPoint localPoint = new IPEndPoint(ip, localPort);
			try
			{
				udpClient = new UdpClient(localPoint);
				udpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
			}
			catch (Exception ex)
			{
				return false;
			}
			is_bound = true;
			return true;
		}
		/// <summary>
		/// 从预先设定的localPort上逐渐累加，尝试寻找一个空闲的port。
		/// </summary>
		/// <returns></returns>
		public int try_bind()
		{
			while (!bind())
			{
				localPort++;
			}
			return localPort;
		}
		public void close()
		{
			stop_recv_trd();
			is_bound = false;
			if (udpClient != null)
			{
				try
				{
					udpClient.Close();
					udpClient.Dispose();
					udpClient = null;
				}
				catch (Exception ex)
				{
				}
			}
		}
		public void connect(string RemoteIP, int RemotePort)
		{
			IPAddress rip = IPAddress.Parse(RemoteIP);
			remotePoint = new IPEndPoint(rip, RemotePort);
		}
		public int send(byte[] data)
		{
			int len = 0;
			if (udpClient != null && remotePoint != null)
			{
				len = udpClient.Send(data, data.Length, remotePoint);
			}
			return len;
		}
		public int send_protocol_frame(double[] q, byte mode, double[] grip)
		{
			return send(pack_protocol_frame(q, mode, grip));
		}
		public static byte[] pack_protocol_frame(double[] q, byte mode, double[] grip)
		{
			if (q == null || q.Length != 6)
			{
				throw new ArgumentException("q must contain 6 values.", "q");
			}
			if (grip == null || grip.Length != 3)
			{
				throw new ArgumentException("grip must contain 3 values.", "grip");
			}

			byte[] frame = new byte[ProtocolFrameLength];
			Array.Copy(ProtocolHeader, 0, frame, 0, ProtocolHeader.Length);

			int offset = ProtocolDataOffset;
			for (int i = 0; i < q.Length; i++)
			{
				write_double_big_endian(frame, offset, q[i]);
				offset += 8;
			}

			frame[offset] = mode;
			offset++;

			for (int i = 0; i < grip.Length; i++)
			{
				write_double_big_endian(frame, offset, grip[i]);
				offset += 8;
			}

			uint crc = calc_crc32(frame, ProtocolDataOffset, ProtocolDataLength);
			write_uint32_big_endian(frame, ProtocolCrcOffset, crc);
			Array.Copy(ProtocolFooter, 0, frame, ProtocolFooterOffset, ProtocolFooter.Length);
			return frame;
		}
		public static bool try_parse_protocol_frame(byte[] frame, out UdpProtocolFrame data)
		{
			string error;
			return try_parse_protocol_frame(frame, out data, out error);
		}
		public static bool try_parse_protocol_frame(byte[] frame, out UdpProtocolFrame data, out string error)
		{
			data = new UdpProtocolFrame();
			error = "";

			if (frame == null)
			{
				error = "Frame is null.";
				return false;
			}
			if (frame.Length != ProtocolFrameLength)
			{
				error = "Frame length must be 85 bytes.";
				return false;
			}
			if (!bytes_equal(frame, 0, ProtocolHeader))
			{
				error = "Invalid frame header.";
				return false;
			}
			if (!bytes_equal(frame, ProtocolFooterOffset, ProtocolFooter))
			{
				error = "Invalid frame footer.";
				return false;
			}

			uint crcRx = read_uint32_big_endian(frame, ProtocolCrcOffset);
			uint crcCalc = calc_crc32(frame, ProtocolDataOffset, ProtocolDataLength);
			if (crcRx != crcCalc)
			{
				error = "Invalid CRC32.";
				return false;
			}

			double[] q = new double[6];
			double[] grip = new double[3];
			int offset = ProtocolDataOffset;
			for (int i = 0; i < q.Length; i++)
			{
				q[i] = read_double_big_endian(frame, offset);
				offset += 8;
			}

			byte mode = frame[offset];
			offset++;

			for (int i = 0; i < grip.Length; i++)
			{
				grip[i] = read_double_big_endian(frame, offset);
				offset += 8;
			}

			data.set(q, mode, grip, crcRx);
			return true;
		}
		private void recv_trd()
		{
			IPEndPoint p = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1);//用于抓取接收消息的ip地址与端口号
			is_receiving = true;
			while (is_receiving)
			{
				byte[] received = udpClient.Receive(ref p);
				long timestamp = DateTime.UtcNow.Ticks;
				if (received != null)
				{
					lock (this)
					{
						data_recv.set(received, p.Address.ToString(), p.Port, timestamp);
					}
				}
			}
		}
		/// <summary>
		/// 等待UDP接收
		/// </summary>
		/// <param name="timeout">以毫秒为单位的超时时间</param>
		/// <returns></returns>
		private bool wait_for_rec(double timeout)
		{
			if (is_receiving == false || trdRecv == null)
			{
				return false;
			}
			data_recv.set(null, "", 0, 0);
			System.Diagnostics.Stopwatch stopTime = new System.Diagnostics.Stopwatch();
			stopTime.Reset();
			stopTime.Start();
			while (stopTime.Elapsed.TotalMilliseconds <= timeout)
			{
				if (data_recv.data != null)
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 发送一帧数据并等待UDP接收
		/// </summary>
		/// <param name="senddata">要发送的数组</param>
		/// <param name="timeout">以毫秒为单位的超时时间</param>
		/// <returns></returns>
		public bool send_wait(byte[] senddata, double timeout)
		{
			send(senddata);
			return wait_for_rec(timeout);
		}
		public void start_recv_trd()
		{
			stop_recv_trd();
			trdRecv = new Thread(recv_trd);
			trdRecv.Name = "UdpReceiveThread";
			trdRecv.IsBackground = true;
			trdRecv.Start();
		}
		public void stop_recv_trd()
		{
			is_receiving = false;
			if (trdRecv != null)
			{
				try
				{
					trdRecv.Abort();
				}
				catch (Exception e) { }
			}
		}
		/// <summary>
		/// 这个函数目前会抓到VMWare的IP，需要修改
		/// </summary>
		/// <returns></returns>
		private static string GetLocalIp()
		{
			///获取本地的IP地址
			string AddressIP = string.Empty;
			foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
			{
				if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
				{
					AddressIP = _IPAddress.ToString();
				}
			}
			return AddressIP;
		}
		private static bool bytes_equal(byte[] data, int offset, byte[] expected)
		{
			for (int i = 0; i < expected.Length; i++)
			{
				if (data[offset + i] != expected[i])
				{
					return false;
				}
			}
			return true;
		}
		private static void write_double_big_endian(byte[] data, int offset, double value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			Array.Copy(bytes, 0, data, offset, bytes.Length);
		}
		private static double read_double_big_endian(byte[] data, int offset)
		{
			byte[] bytes = new byte[8];
			Array.Copy(data, offset, bytes, 0, bytes.Length);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			return BitConverter.ToDouble(bytes, 0);
		}
		private static void write_uint32_big_endian(byte[] data, int offset, uint value)
		{
			data[offset] = (byte)((value >> 24) & 0xFF);
			data[offset + 1] = (byte)((value >> 16) & 0xFF);
			data[offset + 2] = (byte)((value >> 8) & 0xFF);
			data[offset + 3] = (byte)(value & 0xFF);
		}
		private static uint read_uint32_big_endian(byte[] data, int offset)
		{
			return ((uint)data[offset] << 24)
				| ((uint)data[offset + 1] << 16)
				| ((uint)data[offset + 2] << 8)
				| data[offset + 3];
		}
		private static uint calc_crc32(byte[] data, int offset, int length)
		{
			uint crc = 0xFFFFFFFF;
			for (int i = 0; i < length; i++)
			{
				crc = Crc32Table[(crc ^ data[offset + i]) & 0xFF] ^ (crc >> 8);
			}
			return crc ^ 0xFFFFFFFF;
		}
		private static uint[] build_crc32_table()
		{
			uint[] table = new uint[256];
			for (uint i = 0; i < table.Length; i++)
			{
				uint crc = i;
				for (int bit = 0; bit < 8; bit++)
				{
					if ((crc & 1) != 0)
					{
						crc = 0xEDB88320 ^ (crc >> 1);
					}
					else
					{
						crc >>= 1;
					}
				}
				table[i] = crc;
			}
			return table;
		}
	}
}

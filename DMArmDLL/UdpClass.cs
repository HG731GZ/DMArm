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
	/// <summary>
	/// 建立一个UDP类，LocalIP和Port表示该UDP节点的IP与端口，另一程序此节点发送数据时需填入该IP与端口
	/// </summary>
	public class UdpClass
	{
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
	}
}

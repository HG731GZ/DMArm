using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace DMArmDLL
{
	public class TCP_Server
	{
		public IPAddress ip_add;
		public int port_num;
		TcpListener listener;
		private byte[] rec_data = new byte[1024];
		public string rec_str;
		public int rec_len;
		public bool is_serving;
		public int ready_to_send = 0;
		public string send_data_string = "";
		public byte[] send_data_byte;
		public byte[] rec_byte;
		public bool client_connected = false;
		private NetworkStream stream;
		public TcpClient tcpClient;

		Thread waitForConnect, tcpSending, tcpReading;

		public TCP_Server(string IPAdd, int portNum)
		{
			ip_add = IPAddress.Parse(IPAdd);
			port_num = portNum;
		}

		private void tcp_send()
		{

			while ((is_serving) && (!client_connected))
			{
				//在客户端连上来之前先等着
			}
			while (is_serving)
			{
				if (ready_to_send != 0)     //发现要发送了
				{
					try
					{
						if (stream == null) continue;
						if (ready_to_send == 1)       //要发送byte[]
						{
							stream.Write(send_data_byte, 0, send_data_byte.Length);
							for (int i = 0; i < send_data_byte.Length; i++)
							{
								send_data_byte[i] = 0;
							}
						}
						else
						{
							stream.Write(Encoding.ASCII.GetBytes(send_data_string), 0, Encoding.ASCII.GetBytes(send_data_string).Length);
							send_data_string = "";
						}
						ready_to_send = 0;
					}
					catch (IOException ex)
					{
						is_serving = false;
						client_connected = false;
						return;
					}
				}
			}

			//cancellationToken.ThrowIfCancellationRequested();
		}
		private void tcp_read()
		{
			while ((is_serving) && (!client_connected))
			{
				//在客户端连上来之前先等着
			}
			while (is_serving)
			{
				try
				{
					rec_len = stream.Read(rec_data, 0, 1024);
					if (rec_len > 0)
					{
						rec_byte = new byte[rec_len];
						rec_byte = rec_data;
						rec_str = Encoding.ASCII.GetString(rec_data, 0, rec_len);
					}
					//Console.WriteLine(DateTime.Now.TimeOfDay.ToString().PadLeft(20, ' ') + " | " + "收到：" + rec_str);
				}
				catch (IOException e)
				{
					is_serving = false;
					client_connected = false;
					return;
				}
			}

		}

		private void wait_client_connect()
		{
			if (listener == null)
			{
				listener = new TcpListener(ip_add, port_num);
				listener.Start();
			}
			else
			{
				listener.Stop();
				listener = new TcpListener(ip_add, port_num);
				listener.Start();
			}
			while ((is_serving) && (!client_connected))
			{
				if (!listener.Pending())
				{
					//当没有连接请求时，什么也不做，有了请求再执行到tcpListener.AcceptTcpClient()
				}
				else
				{
					tcpClient = listener.AcceptTcpClient();
					stream = tcpClient.GetStream();
					client_connected = true;
					//Console.WriteLine("已连接");
					break;
				}
			}
		}

		public void start_tcp_server()
		{
			is_serving = true;
			waitForConnect = new Thread(wait_client_connect);
			waitForConnect.IsBackground = true;
			waitForConnect.Start();

			tcpReading = new Thread(tcp_read);
			tcpReading.IsBackground = true;
			tcpReading.Start();

			tcpSending = new Thread(tcp_send);
			tcpSending.IsBackground = true;
			tcpSending.Start();
		}

		public void close_tcp_server()
		{
			is_serving = false;
			client_connected = false;
			if (stream != null)
			{
				stream.Close();
				tcpClient.Close();
				listener.Stop();
			}
		}
	}
}

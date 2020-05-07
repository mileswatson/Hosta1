using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hosta.Net
{
	public class Server
	{
		private readonly IPEndPoint localEndPoint;
		private readonly Socket listener;

		public Server(int port)
		{
			IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			localEndPoint = new IPEndPoint(LocalIPAddresses[0], port);

			listener = new Socket(
				ipAddress.AddressFamily,
				SocketType.Stream,
				ProtocolType.Tcp);
		}

		public static IPAddress[] LocalIPAddresses {
			get {
				List<IPAddress> output = new List<IPAddress>();
				foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) // Iterate over each network interface
				{  // Find the network interface which has been provided in the arguments, break the loop if found
					if (item.OperationalStatus == OperationalStatus.Up)
					{   // Fetch the properties of this adapter
						IPInterfaceProperties adapterProperties = item.GetIPProperties();
						// Check if the gateway adress exist, if not its most likley a virtual network or smth
						if (adapterProperties.GatewayAddresses.Count != 0)
						{   // Iterate over each available unicast adresses
							foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
							{   // If the IP is a local IPv4 adress
								if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
								{   // Found a match!
									output.Add(ip.Address);
								}
							}
						}
					}
				}
				// Return results
				return output.ToArray();
			}
		}

		public async void StartListening()
		{
			try
			{
				listener.Bind(localEndPoint);
				listener.Listen(100);

				while (true)
				{
					Console.WriteLine("Waiting for a connection...");
					ConversationStreamer messenger = await AcceptConnection();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			Console.WriteLine("Press ENTER to continue");
			Console.Read();
		}

		private Task<ConversationStreamer> AcceptConnection()
		{
			var tcs = new TaskCompletionSource<ConversationStreamer>();

			listener.BeginAccept(ar =>
			{
				try
				{
					var s = ar.AsyncState as Socket;
					var socketStream = new SocketStream(s.EndAccept(ar));
					tcs.SetResult(new ConversationStreamer(socketStream));
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			}, listener);

			return tcs.Task;
		}
	}
}
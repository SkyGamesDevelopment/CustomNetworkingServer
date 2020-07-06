using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Server
{
	class Server
	{
		#region variables
		public static int maxPlayers { get; private set; }
		public static int port { get; private set; }
		public static IPAddress ipAdress = IPAddress.Parse("192.168.0.10");

		private static TcpListener tcpListener;
		private static UdpClient udpListener;

		public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

		public delegate void PacketHandler(int fromPlayer, Packet packet);
		public static Dictionary<int, PacketHandler> packetHandlers;
		#endregion

		public static void Start(int _maxPlayers, int _port)
		{
			maxPlayers = _maxPlayers;
			port = _port;

			Console.WriteLine($"Starting server on ip {ipAdress} port {port} max players {maxPlayers}...");

			InitializeServerData();

			tcpListener = new TcpListener(ipAdress, port);
			tcpListener.Start();
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

			udpListener = new UdpClient(port);
			udpListener.BeginReceive(UDPReceiveCallback, null);

			Console.WriteLine("Server Started sucessfully!");
		}

		private static void TCPConnectCallback(IAsyncResult result)
		{
			TcpClient client = tcpListener.EndAcceptTcpClient(result);
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

			Console.WriteLine($"Incomming connection from: {client.Client.RemoteEndPoint}");

			for (int i = 1; i <= maxPlayers; i++)
			{
				if (clients[i].tcp.socket == null)
				{
					clients[i].tcp.Connect(client);
					return;
				}
			}

			Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
		}

		private static void UDPReceiveCallback(IAsyncResult result)
		{
			try
			{
				IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
				udpListener.BeginReceive(UDPReceiveCallback, null);

				if (data.Length < 4)
					return;

				using (Packet packet = new Packet(data))
				{
					int clientId = packet.ReadInt();

					if (clientId == 0)
						return;

					if (clients[clientId].udp.endPoint == null)
					{
						clients[clientId].udp.Connect(clientEndPoint);
						return;
					}

					if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
						clients[clientId].udp.HandleData(packet);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error receiving UDP data: {ex}");
			}
		}

		public static void SendUDPData(IPEndPoint endPoint, Packet packet)
		{
			try
			{
				if (endPoint != null)
					udpListener.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error sending data to {endPoint} via UDP: {ex}");
			}
		}

		private static void InitializeServerData()
		{
			for (int i = 1; i <= maxPlayers; i++)
			{
				clients.Add(i, new Client(i));
			}

			packetHandlers = new Dictionary<int, PacketHandler>()
			{
				{ (int)ClientPackets.TCP_HandShakeReturn, ServerHandle.TCP_HandShakeReturn },
				{ (int)ClientPackets.UDP_PlayerInput, ServerHandle.UDP_PlayerInput }
			};

			Console.WriteLine("Server packets initialized");
		}
	}
}

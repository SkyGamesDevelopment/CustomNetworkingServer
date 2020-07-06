using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;

namespace Server
{
	class Client
	{
		public Player player;
		public int id { get; private set; }
		public TCP tcp { get; private set; }
		public UDP udp { get; private set; }

		public static int dataBufferSize = 4096;

		public Client(int _id)
		{
			id = _id;
			tcp = new TCP(id);
			udp = new UDP(id);
		}

		public class TCP
		{
			private int id;
			public TcpClient socket { get; private set; }
			private NetworkStream stream;
			private byte[] receiveBuffer;
			private Packet receivedData;

			public TCP(int _id)
			{
				id = _id;
			}

			public void Connect(TcpClient _socket)
			{
				socket = _socket;
				socket.ReceiveBufferSize = dataBufferSize;
				socket.SendBufferSize = dataBufferSize;

				receiveBuffer = new byte[dataBufferSize];

				stream = socket.GetStream();
				stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

				receivedData = new Packet();

				ServerSend.TCP_HandShake(id);
			}

			public void SendData(Packet packet)
			{
				try
				{
					if (packet != null)
					{
						stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error sending data to player {id} via TCP: {ex}");
				}
			}

			private void ReceiveCallback(IAsyncResult result)
			{
				try
				{
					int byteLength = stream.EndRead(result);

					if (byteLength <= 0)
					{
						//TODO Disconnect
						return;
					}

					byte[] data = new byte[byteLength];
					Array.Copy(receiveBuffer, data, byteLength);

					receivedData.Reset(HandleData(data));

					stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error receiving TCP data: {ex}");

					//TODO Disconnect
				}
			}

			private bool HandleData(byte[] data)
			{
				int packetLenght = 0;

				receivedData.SetBytes(data);

				if (receivedData.UnreadLength() >= 4)
				{
					packetLenght = receivedData.ReadInt();

					if (packetLenght <= 0)
						return true;
				}

				while (packetLenght > 0 && packetLenght <= receivedData.UnreadLength())
				{
					byte[] packetBytes = receivedData.ReadBytes(packetLenght);

					ThreadManager.ExecuteOnMainThread(() =>
					{
						using (Packet packet = new Packet(packetBytes))
						{
							int packetId = packet.ReadInt();
							Server.packetHandlers[packetId](id, packet);
						}
					});

					packetLenght = 0;

					if (receivedData.UnreadLength() >= 4)
					{
						packetLenght = receivedData.ReadInt();

						if (packetLenght <= 0)
							return true;
					}
				}

				if (packetLenght <= 1)
					return true;
				else
					return false;
			}
		}

		public class UDP
		{
			public IPEndPoint endPoint;
			private int id;

			public UDP(int _id)
			{
				id = _id;
			}

			public void Connect(IPEndPoint _endPoint)
			{
				endPoint = _endPoint;
			}

			public void SendData(Packet packet)
			{
				Server.SendUDPData(endPoint, packet);
			}

			public void HandleData(Packet _packet)
			{
				int packetLength = _packet.ReadInt();
				byte[] packetBytes = _packet.ReadBytes(packetLength);

				ThreadManager.ExecuteOnMainThread(() =>
				{
					using (Packet packet = new Packet(packetBytes))
					{
						int packetId = packet.ReadInt();
						Server.packetHandlers[packetId](id, packet);
					}
				});
			}
		}

		#region client methods
		public void SpawnPlayers()
		{
			player = new Player(id, new Vector3(0f, 0f, 0f));

			foreach (Client _client in Server.clients.Values)
			{
				if (_client.player != null && _client.id != id)
					ServerSend.TCP_SpawnPlayer(id, _client.player);
			}
			foreach (Client _client in Server.clients.Values)
			{
				if (_client.player != null)
					ServerSend.TCP_SpawnPlayer(_client.id, player);
			}
		}
		#endregion
	}
}

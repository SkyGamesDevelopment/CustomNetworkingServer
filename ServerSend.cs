using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class ServerSend
	{
		#region Send TCP
		private static void SendTCPToOne(int toClient, Packet packet)
		{
			packet.WriteLength();

			Server.clients[toClient].tcp.SendData(packet);
		}

		private static void SendTCPToAll(Packet packet)
		{
			packet.WriteLength();

			for (int i = 1; i <= Server.maxPlayers; i++)
			{
				Server.clients[i].tcp.SendData(packet);
			}
		}

		private static void SendTCPExceptOne(int exceptPlayer, Packet packet)
		{
			packet.WriteLength();

			for (int i = 1; i <= Server.maxPlayers; i++)
			{
				if (i != exceptPlayer)
					Server.clients[i].tcp.SendData(packet);
			}
		}
		#endregion

		#region Send UDP
		private static void SendUDPToOne(int toClient, Packet packet)
		{
			packet.WriteLength();

			Server.clients[toClient].udp.SendData(packet);
		}

		private static void SendUDPToAll(Packet packet)
		{
			packet.WriteLength();

			for (int i = 1; i <= Server.maxPlayers; i++)
			{
				Server.clients[i].udp.SendData(packet);
			}
		}

		private static void SendUDPExceptOne(int exceptPlayer, Packet packet)
		{
			packet.WriteLength();

			for (int i = 1; i <= Server.maxPlayers; i++)
			{
				if (i != exceptPlayer)
					Server.clients[i].udp.SendData(packet);
			}
		}
		#endregion

		#region TCP packets
		public static void TCP_HandShake(int toClient)
		{
			using (Packet packet = new Packet((int)ServerPackets.TCP_HandShake))
			{
				packet.Write(toClient);

				SendTCPToOne(toClient, packet);
			}
		}

		public static void TCP_SpawnPlayer(int toClient, Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.TCP_SpawnPlayer))
			{
				packet.Write(player.id);
				packet.Write(player.position);
				packet.Write(player.rotation);

				SendTCPToOne(toClient, packet);
			}
		}
		#endregion

		#region UDP packets
		public static void UDP_PlayerPosition(Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.UDP_PlayerPosition))
			{
				packet.Write(player.id);
				packet.Write(player.position);

				SendUDPToAll(packet);
			}
		}

		public static void UDP_PlayerRotation(Player player)
		{
			using (Packet packet = new Packet((int)ServerPackets.UDP_PlayerRotation))
			{
				packet.Write(player.id);
				packet.Write(player.rotation);

				SendUDPExceptOne(player.id, packet);
			}
		}
		#endregion
	}
}

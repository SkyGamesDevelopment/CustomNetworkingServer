using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class ServerHandle
	{
		#region TCP Handle
		public static void TCP_HandShakeReturn(int fromClient, Packet packet)
		{
			try
			{
				int clientId = packet.ReadInt();

				Console.WriteLine($"{Server.clients[clientId].tcp.socket.Client.RemoteEndPoint} connected sucessfully via TCP and is now player {clientId}");

				Server.clients[fromClient].SpawnPlayers();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error unpacking packet: {ex}");
			}
		}
		#endregion

		#region UDP handle
		public static void UDP_PlayerInput(int fromClient, Packet packet)
		{
			bool[] inputs = new bool[packet.ReadInt()];

			for (int i = 0; i < inputs.Length; i++)
			{
				inputs[i] = packet.ReadBool();
			}

			Quaternion rotation = packet.ReadQuaternion();

			Server.clients[fromClient].player.SetInputs(inputs, rotation);
		}
		#endregion
	}
}

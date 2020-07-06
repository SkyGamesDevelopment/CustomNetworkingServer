using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	class Player
	{
		#region variables
		public int id;
		public Vector3 position;
		public Quaternion rotation;
		private bool[] inputs;

		private const float moveSpeed = 5f / Constants.TICKS_PER_SECOND;
		#endregion

		public Player(int _id, Vector3 _position)
		{
			id = _id;
			position = _position;
			rotation = Quaternion.Identity;

			inputs = new bool[4];
		}

		public void Update()
		{
			Vector2 inputDirection = Vector2.Zero;

			//W -> S -> A -> D
			if (inputs[0])
				inputDirection.Y += 1;
			if (inputs[1])
				inputDirection.Y -= 1;
			if (inputs[2])
				inputDirection.X += 1;
			if (inputs[3])
				inputDirection.X -= 1;

			MovePlayer(inputDirection);
		}

		private void MovePlayer(Vector2 inputDirection)
		{
			Vector3 forward = Vector3.Transform(new Vector3(0f, 0f, 1f), rotation);
			Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0f, 1f, 0f)));
			Vector3 moveDirection = right * inputDirection.X + forward * inputDirection.Y;
			position += moveDirection * moveSpeed;

			ServerSend.UDP_PlayerPosition(this);
			ServerSend.UDP_PlayerRotation(this);
		}

		public void SetInputs(bool[] _inputs, Quaternion _rotation)
		{
			inputs = _inputs;
			rotation = _rotation;
		}
	}
}

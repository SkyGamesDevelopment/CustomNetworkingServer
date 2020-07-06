using System;
using System.Threading;

namespace Server
{
	class Program
	{
		private static bool isRunning = false;

		static void Main(string[] args)
		{
			isRunning = true;

			Console.Title = "Game server";

			Thread mainThread = new Thread(new ThreadStart(MainThread));
			mainThread.Start();

			Server.Start(10, 7777);
		}

		private static void MainThread()
		{
			Console.WriteLine($"Main thread started and running at {Constants.TICKS_PER_SECOND} ticks per second");

			DateTime nextLoop = DateTime.Now;

			while (isRunning)
			{
				while (nextLoop < DateTime.Now)
				{
					GameLogic.Update();

					nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

					if (nextLoop > DateTime.Now)
						Thread.Sleep(nextLoop - DateTime.Now);
				}
			}
		}
	}
}

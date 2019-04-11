using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace WorldServer.Network
{
    public class WorldSocket
    {
        public bool Started { get; private set; } = false;

        private readonly CancellationTokenSource token;
        private TcpListener worldListener;

        public WorldSocket() => token = new CancellationTokenSource();

        public bool Start()
        {
            try
            {
                worldListener = new TcpListener(IPAddress.Parse("127.0.0.1"), WorldServer.Sandbox.WorldPort);
                worldListener.Start();
                Started = true;
            }
            catch (Exception e)
            {
                Log.Message(LogType.ERROR, "{0}", e.Message);
                Log.Message();
                Started = false;
            }

            return Started;
        }

        public void StartConnectionThread() => new Thread(AcceptConnection).Start();

        protected void AcceptConnection()
        {
            while (true)
            {
                Thread.Sleep(1);
                if (worldListener.Pending())
                {
                    WorldManager World = new WorldManager
                    {
                        Socket = worldListener.AcceptSocket()
                    };
                    Task.Run(World.Recieve, token.Token);
                }
            }
        }

        protected void Dispose()
        {
            token.Cancel();
            worldListener.Stop();
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace WorldServer.Network
{
    public class RealmSocket
    {
        public bool Started { get; private set; } = false;

        private CancellationTokenSource token = new CancellationTokenSource();
        private TcpListener realmListener;
        private TcpListener proxyListener;

        public bool Start()
        {
            try
            {
                realmListener = new TcpListener(IPAddress.Parse("127.0.0.1"), WorldServer.Sandbox.RealmPort);
                realmListener.Start();

                proxyListener = new TcpListener(IPAddress.Parse("127.0.0.1"), WorldServer.Sandbox.RedirectPort);
                proxyListener.Start();

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

        public void StartRealmThread() => Task.Run(AcceptRealmConnection, token.Token);

        public void StartProxyThread() => Task.Run(AcceptProxyConnection, token.Token);

        protected void AcceptRealmConnection()
        {
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(1);
                if (realmListener.Pending())
                {
                    RealmManager Realm = new RealmManager
                    {
                        RealmSocket = realmListener.AcceptSocket()
                    };
                    Task.Run(Realm.RecieveRealm, token.Token);
                }
            }
        }

        protected void AcceptProxyConnection()
        {
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(1);
                if (proxyListener.Pending())
                {
                    RealmManager Proxy = new RealmManager
                    {
                        ProxySocket = proxyListener.AcceptSocket()
                    };
                    Task.Run(Proxy.RecieveProxy, token.Token);
                }
            }
        }

        public void Dispose()
        {
            token.Cancel();

            realmListener.Stop();
            proxyListener.Stop();
        }
    }
}

using System;
using System.Net.Sockets;
using Common.Logging;
using Common.Constants;

namespace WorldServer.Network
{
    public class RealmManager
    {
        public static RealmSocket RealmSession;
        public Socket realmSocket;
        public Socket proxySocket;

        public void RecieveRealm()
        {
            WorldServer.Sandbox.AuthHandler.HandleRealmList(realmSocket);
        }

        public void RecieveProxy()
        {
            Log.Message();
            Log.Message(LogType.NORMAL, "Begin redirection to WorldServer.");

            proxySocket.SendData(WorldServer.Sandbox.AuthHandler.HandleRedirect());
            proxySocket.Close();

            Log.Message(LogType.NORMAL, "Successfully redirected to WorldServer.");
            Log.Message();
        }
    }
}

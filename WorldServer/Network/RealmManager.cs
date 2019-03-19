using System.Net.Sockets;
using Common.Constants;
using Common.Logging;

namespace WorldServer.Network
{
    public class RealmManager
    {
        public static RealmSocket RealmSession { get; set; }
        public Socket RealmSocket { get; set; }
        public Socket ProxySocket { get; set; }

        public void RecieveRealm()
        {
            WorldServer.Sandbox.AuthHandler.HandleRealmList(RealmSocket);
        }

        public void RecieveProxy()
        {
            Log.Message();
            Log.Message(LogType.NORMAL, "Begin redirection to WorldServer.");

            ProxySocket.SendData(WorldServer.Sandbox.AuthHandler.HandleRedirect());
            ProxySocket.Close();

            Log.Message(LogType.NORMAL, "Successfully redirected to WorldServer.");
            Log.Message();
        }
    }
}

using System.Net.Sockets;

namespace Common.Interfaces.Handlers
{
    public interface IAuthHandler
    {
        IPacketWriter HandleAuthChallenge();
        IPacketWriter HandleRedirect();
        void HandleRealmList(Socket socket);
        void HandleAuthSession(ref IPacketReader packet, ref IWorldManager manager);
        void HandleLogoutRequest(ref IPacketReader packet, ref IWorldManager manager);
    }
}

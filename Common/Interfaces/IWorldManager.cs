using System.Net.Sockets;
using Common.Structs;

namespace Common.Interfaces
{
    public interface IWorldManager
    {
        Account Account { get; set; }
        Socket Socket { get; set; }

        void Recieve();
        void Send(IPacketWriter packet);
    }
}

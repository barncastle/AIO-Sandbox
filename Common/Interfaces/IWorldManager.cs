using System.Net.Sockets;
using Common.Structs;

namespace Common.Interfaces
{
    public interface IWorldManager
    {
        Account Account { get; set; }
        Socket Socket { get; set; }
        ISandbox SandboxHost { get; }

        void Recieve();

        void Send(IPacketWriter packet);
    }
}

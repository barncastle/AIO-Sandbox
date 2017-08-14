using Common.Structs;
using System;
using System.Net.Sockets;

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

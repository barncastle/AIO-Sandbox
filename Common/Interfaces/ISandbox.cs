using Common.Interfaces.Handlers;

namespace Common.Interfaces
{
    public interface ISandbox
    {
        string RealmName { get; }
        int Expansion { get; }
        int Build { get; }
        int RealmPort { get; }
        int RedirectPort { get; }
        int WorldPort { get; }

        IOpcodes Opcodes { get; }
        IAuthHandler AuthHandler { get; }
        ICharHandler CharHandler { get; }
        IWorldHandler WorldHandler { get; }

        IPacketReader ReadPacket(byte[] data, bool parse = true);

        IPacketWriter WritePacket();
    }
}

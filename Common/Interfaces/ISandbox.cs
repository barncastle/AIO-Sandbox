using Common.Interfaces.Handlers;

namespace Common.Interfaces
{
    public interface ISandbox
    {
        string RealmName { get; set; }
        int Build { get; set; }
        int RealmPort { get; set; }
        int RedirectPort { get; set; }
        int WorldPort { get; set; }

        IOpcodes Opcodes { get; set; }
        IAuthHandler AuthHandler { get; set; }
        ICharHandler CharHandler { get; set; }
        IWorldHandler WorldHandler { get; set; }

        IPacketReader ReadPacket(byte[] data, bool parse = true);

        IPacketWriter WritePacket();
    }
}

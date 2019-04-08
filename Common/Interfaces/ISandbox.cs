using Common.Constants;
using Common.Interfaces.Handlers;

namespace Common.Interfaces
{
    public interface ISandbox
    {
        string RealmName { get; }
        Expansions Expansion { get; }
        int Build { get; }
        int RealmPort { get; }
        int RedirectPort { get; }
        int WorldPort { get; }

        IOpcodes Opcodes { get; }
        IAuthHandler AuthHandler { get; }
        ICharHandler CharHandler { get; }
        IWorldHandler WorldHandler { get; }

        IPacketReader ReadPacket(byte[] data, bool parse = true);
    }
}

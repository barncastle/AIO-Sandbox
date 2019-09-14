using Common.Constants;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using MoP_15464.Handlers;

namespace MoP_15464
{
    public class Sandbox : ISandbox
    {
        public static Sandbox Instance { get; } = new Sandbox();

        public string RealmName => "MoP (5.0.1.15464) Sandbox";
        public Expansions Expansion => Expansions.MoP;
        public int Build => 15464;
        public int RealmPort => 3724;
        public int RedirectPort => 9002;
        public int WorldPort => 8129;

        public IOpcodes Opcodes => new Opcodes();

        public IAuthHandler AuthHandler => new AuthHandler();
        public ICharHandler CharHandler => new CharHandler();
        public IWorldHandler WorldHandler => new WorldHandler();

        public IPacketReader ReadPacket(byte[] data, bool parse = true) => new PacketReader(data, parse);

    }
}

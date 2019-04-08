using Alpha_3494.Handlers;
using Common.Constants;
using Common.Interfaces;
using Common.Interfaces.Handlers;

namespace Alpha_3494
{
    public class Sandbox : ISandbox
    {
        public static Sandbox Instance { get; } = new Sandbox();

        public string RealmName => "Alpha (0.5.5) Sandbox";
        public Expansions Expansion => Expansions.PreRelease;
        public int Build => 3592;
        public int RealmPort => 3724;
        public int RedirectPort => 9090;
        public int WorldPort => 8086;

        public IOpcodes Opcodes => new Opcodes();

        public IAuthHandler AuthHandler => new AuthHandler();
        public ICharHandler CharHandler => new CharHandler();
        public IWorldHandler WorldHandler => new WorldHandler();

        public IPacketReader ReadPacket(byte[] data, bool parse = true) => new PacketReader(data, parse);

    }
}

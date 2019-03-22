using Alpha_3368.Handlers;
using Common.Constants;
using Common.Interfaces;
using Common.Interfaces.Handlers;

namespace Alpha_3368
{
    public class Sandbox : ISandbox
    {
        public static Sandbox Instance { get; } = new Sandbox();

        public string RealmName => "Alpha (0.5.3) Sandbox";
        public Expansions Expansion => Expansions.PreRelease;
        public int Build => 3368;
        public int RealmPort => 9100;
        public int RedirectPort => 9090;
        public int WorldPort => 8100;

        public IOpcodes Opcodes => new Opcodes();

        public IAuthHandler AuthHandler => new AuthHandler();
        public ICharHandler CharHandler => new CharHandler();
        public IWorldHandler WorldHandler => new WorldHandler();

        public IPacketReader ReadPacket(byte[] data, bool parse = true) => new PacketReader(data, parse);

        public IPacketWriter WritePacket() => new PacketWriter();
    }
}

using Common.Constants;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using WotLK_12025.Handlers;

namespace WotLK_12025
{
    public class Sandbox : ISandbox
    {
        public static Sandbox Instance { get; } = new Sandbox();

        public string RealmName => "Cata (4.0.0.12025-4.0.0.12065) Sandbox";
        public Expansions Expansion => Expansions.Cata;
        public int Build => 12025;
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

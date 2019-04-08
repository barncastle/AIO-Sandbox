using Common.Constants;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Vanilla_5428.Handlers;

namespace Vanilla_5428
{
    public class Sandbox : ISandbox
    {
        public static Sandbox Instance { get; } = new Sandbox();

        public string RealmName => "Vanilla (1.11.X) Sandbox";
        public Expansions Expansion => Expansions.Vanilla;
        public int Build => 5428;
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

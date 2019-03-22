using Beta_3892.Handlers;
using Common.Constants;
using Common.Interfaces;
using Common.Interfaces.Handlers;

namespace Beta_3892
{
    public class Sandbox : ISandbox
    {
        public static Sandbox Instance { get; } = new Sandbox();

        public string RealmName => "Beta 3 (0.10.0/0.11.0) Sandbox";
        public Expansions Expansion => Expansions.PreRelease;
        public int Build => 3892;
        public int RealmPort => 3724;
        public int RedirectPort => 9002;
        public int WorldPort => 8129;

        public IOpcodes Opcodes => new Opcodes();

        public IAuthHandler AuthHandler => new AuthHandler();
        public ICharHandler CharHandler => new CharHandler();
        public IWorldHandler WorldHandler => new WorldHandler();

        public IPacketReader ReadPacket(byte[] data, bool parse = true) => new PacketReader(data, parse);

        public IPacketWriter WritePacket() => new PacketWriter();
    }
}

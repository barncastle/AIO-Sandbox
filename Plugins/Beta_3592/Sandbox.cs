using Beta_3592.Handlers;
using Common.Interfaces;
using Common.Interfaces.Handlers;

namespace Beta_3592
{
    public class Sandbox : ISandbox
    {
        public static Sandbox Instance { get; } = new Sandbox();

        public string RealmName => "Beta 1 (0.6.0) Sandbox";
        public int Expansion => 0;
        public int Build => 3592;
        public int RealmPort => 3724;
        public int RedirectPort => 9002;
        public int WorldPort => 9001;

        public IOpcodes Opcodes => new Opcodes();

        public IAuthHandler AuthHandler => new AuthHandler();
        public ICharHandler CharHandler => new CharHandler();
        public IWorldHandler WorldHandler => new WorldHandler();

        public IPacketReader ReadPacket(byte[] data, bool parse = true) => new PacketReader(data, parse);

        public IPacketWriter WritePacket() => new PacketWriter();
    }
}

using Alpha_3494.Handlers;
using Common.Interfaces;
using Common.Interfaces.Handlers;

namespace Alpha_3494
{
    public class Sandbox : ISandbox
    {
        public static Sandbox Instance { get; } = new Sandbox();

        public string RealmName { get; set; } = "|cFF00FFFFAlpha (0.5.5) Sandbox";
        public int Build { get; set; } = 3592;
        public int RealmPort { get; set; } = 3724;
        public int RedirectPort { get; set; } = 9090;
        public int WorldPort { get; set; } = 8086;

        public IOpcodes Opcodes { get; set; } = new Opcodes();

        public IAuthHandler AuthHandler { get; set; } = new AuthHandler();
        public ICharHandler CharHandler { get; set; } = new CharHandler();
        public IWorldHandler WorldHandler { get; set; } = new WorldHandler();

        public IPacketReader ReadPacket(byte[] data, bool parse = true) => new PacketReader(data, parse);

        public IPacketWriter WritePacket() => new PacketWriter();
    }
}

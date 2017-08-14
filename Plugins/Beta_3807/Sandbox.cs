using Beta_3807.Handlers;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta_3807
{
    public class Sandbox : ISandbox
    {
        public static Sandbox Instance => _instance;
        static readonly Sandbox _instance = new Sandbox();

        public string RealmName { get; set; } = "|cFF00FFFFBeta 3 (0.9.0) Sandbox";
        public int Build { get; set; } = 3807;
        public int RealmPort { get; set; } = 3724;
        public int RedirectPort { get; set; } = 9002;
        public int WorldPort { get; set; } = 9001;

        public IOpcodes Opcodes { get; set; } = new Opcodes();

        public IAuthHandler AuthHandler { get; set; } = new AuthHandler();
        public ICharHandler CharHandler { get; set; } = new CharHandler();
        public IWorldHandler WorldHandler { get; set; } = new WorldHandler();
        public IPacketReader ReadPacket(byte[] data, bool parse = true) => new PacketReader(data, parse);
        public IPacketWriter WritePacket() => new PacketWriter();
    }
}

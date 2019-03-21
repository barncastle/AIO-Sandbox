using Common.Interfaces;
using Common.Interfaces.Handlers;

namespace WorldServer
{
    public class SandboxHost
    {
        private readonly ISandbox _instance;

        public SandboxHost(ISandbox sandbox)
        {
            _instance = sandbox;
            RealmName = _instance.RealmName[0] == '|' ? _instance.RealmName.Substring(10) : _instance.RealmName; // Remove colour code
        }

        public string RealmName { get; private set; }
        public int Expansion => _instance.Expansion;
        public int Build => _instance.Build;
        public int RealmPort => _instance.RealmPort;
        public int RedirectPort => _instance.RedirectPort;
        public int WorldPort => _instance.WorldPort;

        public IOpcodes Opcodes => _instance.Opcodes;

        public IAuthHandler AuthHandler => _instance.AuthHandler;
        public ICharHandler CharHandler => _instance.CharHandler;
        public IWorldHandler WorldHandler => _instance.WorldHandler;

        public IPacketReader ReadPacket(byte[] data, bool parse = true) => _instance.ReadPacket(data, parse);

        public IPacketWriter WritePacket() => _instance.WritePacket();
    }
}

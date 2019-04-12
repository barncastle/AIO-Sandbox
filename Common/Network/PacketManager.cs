using System.Collections.Generic;
using Common.Interfaces;

namespace Common.Network
{
    public static class PacketManager
    {
        public delegate void PacketHandler(ref IPacketReader packet, ref IWorldManager manager);

        private static Dictionary<Opcodes, PacketHandler> OpcodeHandlers;

        static PacketManager() => OpcodeHandlers = new Dictionary<Opcodes, PacketHandler>();

        public static void DefineOpcodeHandler(Opcodes opcode, PacketHandler handler)
        {
            OpcodeHandlers[opcode] = handler;
        }

        public static bool InvokeHandler(IPacketReader reader, IWorldManager manager, Opcodes opcode)
        {
            if (OpcodeHandlers.TryGetValue(opcode, out var handle))
            {
                handle.Invoke(ref reader, ref manager);
                return true;
            }

            return false;
        }
    }
}

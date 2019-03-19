using System.Collections.Generic;
using Common.Interfaces;

namespace Common.Network
{
    public static class PacketManager
    {
        public static Dictionary<Opcodes, HandlePacket> OpcodeHandlers;

        public delegate void HandlePacket(ref IPacketReader packet, ref IWorldManager manager);

        static PacketManager()
        {
            OpcodeHandlers = new Dictionary<Opcodes, HandlePacket>();
        }

        public static void DefineOpcodeHandler(Opcodes opcode, HandlePacket handler)
        {
            OpcodeHandlers[opcode] = handler;
        }

        public static bool InvokeHandler(IPacketReader reader, IWorldManager manager, Opcodes opcode)
        {
            if(OpcodeHandlers.TryGetValue(opcode, out var handle))
            {
                handle.Invoke(ref reader, ref manager);
                return true;
            }

            return false;
        }
    }
}

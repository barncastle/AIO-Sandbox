using System.Collections.Generic;
using Common.Interfaces;

namespace Common.Network
{
    public static class PacketManager
    {
        public static Dictionary<Opcodes, HandlePacket> OpcodeHandlers = new Dictionary<Opcodes, HandlePacket>();
        public delegate void HandlePacket(ref IPacketReader packet, ref IWorldManager manager);

        public static void DefineOpcodeHandler(Opcodes opcode, HandlePacket handler)
        {
            OpcodeHandlers[opcode] = handler;
        }

        public static bool InvokeHandler(IPacketReader reader, IWorldManager manager, Opcodes opcode)
        {
            if (OpcodeHandlers.ContainsKey(opcode))
            {
                OpcodeHandlers[opcode].Invoke(ref reader, ref manager);
                return true;
            }
            else
                return false;
        }
    }
}

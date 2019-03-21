using Common.Network;

namespace Alpha_3368
{
    public class PacketReader : BasePacketReader
    {
        public PacketReader(byte[] data, bool parse = true) : base(data)
        {
            if (parse)
            {
                // Packet header (0.5.3.3368): Size: 2 bytes + Cmd: 4 bytes
                Size = (uint)((ReadUInt16() / 0x100) + 2);
                Opcode = ReadUInt32();
            }
        }
    }
}

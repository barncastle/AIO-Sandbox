using System.IO;
using Common.Network;

namespace Alpha_3368
{
    public class PacketWriter : BasePacketWriter
    {
        public PacketWriter() : base() => PreAuth = true;

        public PacketWriter(uint opcode, string name) : base()
        {
            Name = name;
            Opcode = opcode;
            WritePacketHeader(opcode);
        }

        public override void WritePacketHeader(uint opcode)
        {
            // Packet header (0.5.3.3368): Size: 2 bytes + Cmd: 2 bytes
            // Packet header after SMSG_AUTH_CHALLENGE (0.5.3.3368): Size: 2 bytes + Cmd: 4 bytes
            WriteUInt16(0);
            WriteUInt8((byte)(opcode % 0x100));
            WriteUInt8((byte)(opcode / 0x100));

            if (opcode != 0x1DD)
            {
                WriteUInt8(0);
                WriteUInt8(0);
            }
        }

        public override byte[] ReadDataToSend()
        {
            byte[] data = new byte[BaseStream.Length];
            Seek(0, SeekOrigin.Begin);

            BaseStream.Read(data, 0, (int)BaseStream.Length);

            Size = (ushort)(data.Length - 2);
            if (!PreAuth)
            {
                data[0] = (byte)(Size / 0x100);
                data[1] = (byte)(Size % 0x100);
            }
            return data;
        }
    }
}

using System.IO;
using Common.Network;

namespace Alpha_3494
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
            WriteUInt16(0);
            WriteUInt32(opcode);
        }

        public override byte[] ReadDataToSend()
        {
            byte[] data = new byte[BaseStream.Length];
            Seek(0, SeekOrigin.Begin);

            BaseStream.Read(data, 0, (int)BaseStream.Length);

            Size = (ushort)(data.Length - 2);

            if (!PreAuth)
            {
                Size = (ushort)((Size >> 8) + ((Size & 0xFF) << 8));
                data[0] = (byte)(Size & 0xFF);
                data[1] = (byte)(Size >> 8);
            }

            return data;
        }
    }
}

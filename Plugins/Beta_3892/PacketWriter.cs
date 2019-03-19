using System.IO;
using Common.Network;

namespace Beta_3892
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
            WriteUInt16((ushort)opcode);
        }

        public override byte[] ReadDataToSend()
        {
            byte[] data = new byte[BaseStream.Length];
            Seek(0, SeekOrigin.Begin);

            BaseStream.Read(data, 0, (int)BaseStream.Length);

            Size = (ushort)(data.Length - 2);

            if (!PreAuth)
            {
                // Size = (ushort)((Size >> 8) + ((Size & 0xFF) << 8));
                data[0] = (byte)(Size >> 8);
                data[1] = (byte)(Size & 255);
            }
            return data;
        }
    }
}

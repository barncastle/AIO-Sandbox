using System.IO;
using Common.Cryptography;
using Common.Network;

namespace Vanilla_4341
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
                data[0] = (byte)(Size >> 8);
                data[1] = (byte)(Size & 255);
                Encode(ref data);
            }
            return data;
        }
    }
}

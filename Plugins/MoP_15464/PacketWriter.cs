using System.IO;
using Common.Cryptography;
using Common.Network;

namespace MoP_15464
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
                if (Authenticator.PacketCrypt.Initialised)
                {
                    uint header = ((Size - 2) << 12) | (Opcode & 0xFFF);
                    data[0] = (byte)(header & 0xFF);
                    data[1] = (byte)((header >> 8) & 0xFF);
                    data[2] = (byte)((header >> 16) & 0xFF);
                    data[3] = (byte)((header >> 24) & 0xFF);

                    Encode(ref data);
                }
                else
                {
                    data[0] = (byte)(Size & 0xFF);
                    data[1] = (byte)((Size >> 8) & 0xFF);
                }
            }

            return data;
        }


        public void WriteGUIDByte(ulong guid, int index)
        {
            byte value = (byte)((guid >> (index * 8)) & 0xFF);
            if (value != 0)
                Write((byte)(value ^ 1));
        }
    }
}

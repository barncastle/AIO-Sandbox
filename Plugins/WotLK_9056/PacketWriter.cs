using System.IO;
using Common.Cryptography;
using Common.Network;

namespace WotLK_9056
{
    public class PacketWriter : BasePacketWriter
    {
        private const int SHA_DIGEST_LENGTH = 20;

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

        private void Encode(ref byte[] data)
        {
            if (!ClientAuth.Encode || data.Length < 4)
                return;

            for (int i = 0; i < 4; i++)
            {
                ClientAuth.Key[3] %= SHA_DIGEST_LENGTH;
                byte x = (byte)((data[i] ^ ClientAuth.SS_Hash[ClientAuth.Key[3]]) + ClientAuth.Key[2]);
                ++ClientAuth.Key[3];
                data[i] = ClientAuth.Key[2] = x;
            }
        }
    }
}

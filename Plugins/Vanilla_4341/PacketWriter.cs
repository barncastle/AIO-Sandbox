using System.IO;
using System.Text;
using Common.Cryptography;
using Common.Interfaces;

namespace Vanilla_4341
{
    public class PacketWriter : BinaryWriter, IPacketWriter
    {
        public string Name { get; set; }
        public uint Opcode { get; set; }
        public uint Size { get; set; }
        public bool PreAuth { get; set; } = false;

        private const int SHA_DIGEST_LENGTH = 40;


        public PacketWriter() : base(new MemoryStream())
        {
            PreAuth = true;
        }

        public PacketWriter(uint opcode, string name) : base(new MemoryStream())
        {
            Name = name;
            Opcode = opcode;
            WritePacketHeader(opcode);
        }


        public void WritePacketHeader(uint opcode)
        {
            WriteUInt16(0);
            WriteUInt16((ushort)opcode);
        }

        public byte[] ReadDataToSend()
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

        public void WriteInt8(sbyte data)
        {
            base.Write(data);
        }

        public void WriteInt16(short data)
        {
            base.Write(data);
        }

        public void WriteInt32(int data)
        {
            base.Write(data);
        }

        public void WriteInt64(long data)
        {
            base.Write(data);
        }

        public void WriteUInt8(byte data)
        {
            base.Write(data);
        }

        public void WriteUInt16(ushort data)
        {
            base.Write(data);
        }

        public void WriteUInt32(uint data)
        {
            base.Write(data);
        }

        public void WriteUInt64(ulong data)
        {
            base.Write(data);
        }

        public void WriteFloat(float data)
        {
            base.Write(data);
        }

        public void WriteDouble(double data)
        {
            base.Write(data);
        }

        public void WriteString(string data)
        {
            byte[] sBytes = Encoding.ASCII.GetBytes(data);
            WriteBytes(sBytes);
            base.Write((byte)0);    // String null terminated
        }

        public void WriteBytes(byte[] data)
        {
            base.Write(data);
        }
    }
}

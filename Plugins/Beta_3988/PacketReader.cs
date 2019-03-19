using System;
using System.IO;
using System.Text;
using Common.Cryptography;
using Common.Interfaces;

namespace Beta_3988
{
    public class PacketReader : BinaryReader, IPacketReader
    {
        public uint Opcode { get; set; }
        public uint Size { get; set; }
        public long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        private const int SHA_DIGEST_LENGTH = 40;

        public PacketReader(byte[] data, bool parse = true) : base(new MemoryStream(data))
        {
            if (parse)
            {
                Decode(ref data);
                ushort size = ReadUInt16();
                Size = (ushort)((size >> 8) + ((size & 0xFF) << 8) + 2);
                Opcode = ReadUInt32();
            }
        }

        private void Decode(ref byte[] data)
        {
            if (!ClientAuth.Encode || data.Length < 6)
                return;

            for (int i = 0; i < 6; i++)
            {
                ClientAuth.Key[1] %= SHA_DIGEST_LENGTH;
                byte x = (byte)((data[i] - ClientAuth.Key[0]) ^ ClientAuth.SS_Hash[ClientAuth.Key[1]]);
                ++ClientAuth.Key[1];
                ClientAuth.Key[0] = data[i];
                data[i] = x;
            }
        }

        public sbyte ReadInt8()
        {
            return base.ReadSByte();
        }

        public new short ReadInt16()
        {
            return base.ReadInt16();
        }

        public new int ReadInt32()
        {
            return base.ReadInt32();
        }

        public new long ReadInt64()
        {
            return base.ReadInt64();
        }

        public byte ReadUInt8()
        {
            return base.ReadByte();
        }

        public new ushort ReadUInt16()
        {
            return base.ReadUInt16();
        }

        public new uint ReadUInt32()
        {
            return base.ReadUInt32();
        }

        public new ulong ReadUInt64()
        {
            return base.ReadUInt64();
        }

        public float ReadFloat()
        {
            return base.ReadSingle();
        }

        public new double ReadDouble()
        {
            return base.ReadDouble();
        }

        public string ReadString(byte terminator = 0)
        {
            StringBuilder tmpString = new StringBuilder();
            char tmpChar = base.ReadChar();
            char tmpEndChar = Convert.ToChar(terminator);

            while (tmpChar != tmpEndChar)
            {
                tmpString.Append(tmpChar);
                tmpChar = base.ReadChar();
            }

            return tmpString.ToString();
        }

        public new string ReadString()
        {
            return ReadString(0);
        }

        public new byte[] ReadBytes(int count)
        {
            return base.ReadBytes(count);
        }

        public byte[] ReadToEnd()
        {
            return base.ReadBytes((int)(BaseStream.Length - BaseStream.Position));
        }

        public string ReadStringFromBytes(int count)
        {
            byte[] stringArray = base.ReadBytes(count);
            Array.Reverse(stringArray);

            return Encoding.ASCII.GetString(stringArray);
        }

        public void SkipBytes(int count)
        {
            base.BaseStream.Position += count;
        }
    }
}

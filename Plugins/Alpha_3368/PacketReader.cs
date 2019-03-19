using System;
using System.IO;
using System.Text;
using Common.Interfaces;

namespace Alpha_3368
{
    public class PacketReader : BinaryReader, IPacketReader
    {
        public uint Opcode { get; set; }
        public uint Size { get; set; }
        public long Position { get { return BaseStream.Position; } set { BaseStream.Position = value; } }


        public PacketReader(byte[] data, bool parse = true) : base(new MemoryStream(data))
        {
            if (parse)
            {
                //Packet header (0.5.3.3368): Size: 2 bytes + Cmd: 4 bytes
                Size = (uint)((this.ReadUInt16() / 0x100) + 2);
                Opcode = this.ReadUInt32();
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

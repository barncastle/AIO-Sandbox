using System;
using System.IO;
using System.Text;
using Common.Interfaces;

namespace Common.Network
{
    public abstract class BasePacketReader : BinaryReader, IPacketReader
    {
        public uint Opcode { get; protected set; }
        public uint Size { get; protected set; }
        public long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        public BasePacketReader(byte[] data) : base(new MemoryStream(data))
        {
        }

        public sbyte ReadInt8() => base.ReadSByte();

        public new short ReadInt16() => base.ReadInt16();

        public new int ReadInt32() => base.ReadInt32();

        public new long ReadInt64() => base.ReadInt64();

        public byte ReadUInt8() => base.ReadByte();

        public new ushort ReadUInt16() => base.ReadUInt16();

        public new uint ReadUInt32() => base.ReadUInt32();

        public new ulong ReadUInt64() => base.ReadUInt64();

        public float ReadFloat() => base.ReadSingle();

        public new double ReadDouble() => base.ReadDouble();

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

        public new string ReadString() => ReadString(0);

        public new byte[] ReadBytes(int count) => base.ReadBytes(count);

        public byte[] ReadToEnd() => base.ReadBytes((int)(BaseStream.Length - BaseStream.Position));

        public string ReadStringFromBytes(int count)
        {
            byte[] stringArray = base.ReadBytes(count);
            Array.Reverse(stringArray);

            return Encoding.ASCII.GetString(stringArray);
        }

        public void SkipBytes(int count) => base.BaseStream.Position += count;
    }
}

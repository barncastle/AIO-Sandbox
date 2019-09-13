using System.IO;
using System.Text;
using Common.Cryptography;
using Common.Interfaces;

namespace Common.Network
{
    public abstract class BasePacketReader : BinaryReader, IPacketReader
    {
        public uint Opcode { get; protected set; }
        public uint Size { get; protected set; }
        public long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        public BasePacketReader(byte[] data) : base(new MemoryStream(data)) { }

        public bool ReadBool() => base.ReadBoolean();

        public sbyte ReadInt8() => base.ReadSByte();

        public byte ReadUInt8() => base.ReadByte();

        public float ReadFloat() => base.ReadSingle();

        public new string ReadString()
        {
            StringBuilder sb = new StringBuilder(0x20);
            byte b;
            while (true)
            {
                b = ReadByte();
                if (b == 0)
                    break;

                sb.Append((char)b);
            }

            return sb.ToString();
        }

        public string ReadString(int size)
        {
            return Encoding.UTF8.GetString(ReadBytes(size));
        }

        public byte[] ReadToEnd() => base.ReadBytes((int)(Size - Position));

        public ulong ReadPackedGUID()
        {
            byte mask = ReadByte();
            ulong guid = 0;

            for (int i = 0; i < 8; i++)
                if ((mask & (1 << i)) != 0)
                    guid |= (ulong)ReadByte() << (i * 8);

            return guid;
        }


        protected virtual void Decode(ref byte[] buffer, int count = 6) => Authenticator.PacketCrypt.Decode(buffer, count);
    }
}

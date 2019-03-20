using System.IO;
using System.Text;
using Common.Interfaces;

namespace Common.Network
{
    public abstract class BasePacketWriter : BinaryWriter, IPacketWriter
    {
        public string Name { get; set; }
        public uint Opcode { get; set; }
        public uint Size { get; set; }
        public bool PreAuth { get; set; } = false;

        public BasePacketWriter() : base(new MemoryStream())
        {
        }

        public BasePacketWriter(uint opcode, string name) : base(new MemoryStream())
        {
            Name = name;
            Opcode = opcode;
            WritePacketHeader(opcode);
        }

        public abstract void WritePacketHeader(uint opcode);

        public abstract byte[] ReadDataToSend();

        public void WriteInt8(sbyte data) => base.Write(data);

        public void WriteInt16(short data) => base.Write(data);

        public void WriteInt32(int data) => base.Write(data);

        public void WriteInt64(long data) => base.Write(data);

        public void WriteUInt8(byte data) => base.Write(data);

        public void WriteUInt16(ushort data) => base.Write(data);

        public void WriteUInt32(uint data) => base.Write(data);

        public void WriteUInt64(ulong data) => base.Write(data);

        public void WriteFloat(float data) => base.Write(data);

        public void WriteDouble(double data) => base.Write(data);

        public void WriteString(string data)
        {
            byte[] sBytes = Encoding.ASCII.GetBytes(data);
            Write(sBytes);
            base.Write((byte)0);    // String null terminated
        }
    }
}

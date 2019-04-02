using System.IO;
using System.Text;
using Common.Interfaces;

namespace Common.Network
{
    public abstract class BasePacketWriter : BinaryWriter, IPacketWriter
    {
        public string Name { get; protected set; }
        public uint Opcode { get; protected set; }
        public uint Size { get; protected set; }
        public bool PreAuth { get; set; } = false;

        public BasePacketWriter() : base(new MemoryStream())
        {
        }

        public abstract void WritePacketHeader(uint opcode);

        public abstract byte[] ReadDataToSend();

        public void WriteBool(bool data) => base.Write(data);

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

        public void WritePackedGUID(ulong guid)
        {
            byte[] packed = new byte[9];

            int count = 0;
            while (guid > 0)
            {
                byte bit = (byte)guid;
                if (bit != 0)
                {
                    packed[0] |= (byte)(1 << count);
                    packed[++count] = bit;
                }
                guid >>= 8;
            }

            base.Write(packed, 0, count + 1);
        }
    }
}

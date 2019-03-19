using System.IO;
using System.Text;
using Common.Interfaces;

namespace Beta_3807
{
    public class PacketWriter : BinaryWriter, IPacketWriter
    {
        public string Name { get; set; }
        public uint Opcode { get; set; }
        public uint Size { get; set; }
        public bool PreAuth { get; set; } = false;


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
                //Size = (ushort)((Size >> 8) + ((Size & 0xFF) << 8));
                data[0] = (byte)(Size >> 8);
                data[1] = (byte)(Size & 255);
            }
            return data;
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
            this.WriteBytes(sBytes);
            base.Write((byte)0);    //String null terminated
        }

        public void WriteBytes(byte[] data)
        {
            base.Write(data);
        }
    }
}

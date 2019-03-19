using System.IO;
using System.Text;
using Common.Interfaces;

namespace Alpha_3368
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
            //Packet header (0.5.3.3368): Size: 2 bytes + Cmd: 2 bytes
            //Packet header after SMSG_AUTH_CHALLENGE (0.5.3.3368): Size: 2 bytes + Cmd: 4 bytes
            WriteUInt16(0);
            WriteUInt8((byte)(opcode % 0x100));
            WriteUInt8((byte)(opcode / 0x100));

            if (opcode != 0x1DD)
            {
                WriteUInt8(0);
                WriteUInt8(0);
            }
        }

        public byte[] ReadDataToSend()
        {
            byte[] data = new byte[BaseStream.Length];
            Seek(0, SeekOrigin.Begin);

            BaseStream.Read(data, 0, (int)BaseStream.Length);

            Size = (ushort)(data.Length - 2);
            if (!PreAuth)
            {
                data[0] = (byte)(Size / 0x100);
                data[1] = (byte)(Size % 0x100);
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

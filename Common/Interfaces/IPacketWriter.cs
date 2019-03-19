namespace Common.Interfaces
{
    public interface IPacketWriter
    {
        string Name { get; set; }
        uint Opcode { get; set; }
        uint Size { get; set; }


        void WritePacketHeader(uint opcode);
        byte[] ReadDataToSend();


        void WriteInt8(sbyte data);
        void WriteInt16(short data);
        void WriteInt32(int data);
        void WriteInt64(long data);
        void WriteUInt8(byte data);
        void WriteUInt16(ushort data);
        void WriteUInt32(uint data);
        void WriteUInt64(ulong data);
        void WriteFloat(float data);
        void WriteDouble(double data);
        void WriteString(string data);
        void WriteBytes(byte[] data);
    }
}

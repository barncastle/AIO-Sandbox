using System;
using Common.Interfaces;

namespace Common.Network
{
    public class BitPacker
    {
        private readonly IPacketWriter _packetWriter;
        private byte Position = 8;
        private byte Buffer = 0;

        public BitPacker(IPacketWriter packetWriter) => _packetWriter = packetWriter;


        public void Write<T>(T bit)
        {
            --Position;

            if (Convert.ToBoolean(bit))
                Buffer |= (byte)(1 << (Position));

            if (Position == 0)
            {
                Position = 8;
                _packetWriter.WriteUInt8(Buffer);
                Buffer = 0;
            }
        }

        public void Write<T>(T bit, int count)
        {
            checked
            {
                for (int i = count - 1; i >= 0; --i)
                    Write((T)Convert.ChangeType(((Convert.ToInt32(bit) >> i) & 1), typeof(T)));
            }
        }


        public void Flush()
        {
            if (Position == 8)
                return;

            _packetWriter.WriteUInt8(Buffer);
            Buffer = 0;
            Position = 8;
        }
    }
}

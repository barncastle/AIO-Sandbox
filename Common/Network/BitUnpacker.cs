using System;
using Common.Interfaces;

namespace Common.Network
{
    public class BitUnpacker
    {
        private readonly IPacketReader _packetReader;

        private byte Position = 8;
        private byte Buffer = 0;

        public BitUnpacker(IPacketReader packetReader)
        {
            _packetReader = packetReader;
        }


        public bool GetBit()
        {
            if (Position == 8)
            {
                Buffer = _packetReader.ReadByte();
                Position = 0;
            }

            int value = Buffer;
            Buffer = (byte)(2 * value);
            Position++;

            return (value >> 7) != 0;
        }

        public T GetBits<T>(byte bitCount)
        {
            checked
            {
                int value = 0;
                for (int i = bitCount - 1; i >= 0; --i)
                    value = GetBit() ? (1 << i) | value : value;

                return (T)Convert.ChangeType(value, typeof(T));
            }
        }

        public ulong ReadPackedGuid(byte[] mask, byte[] bytes)
        {
            bool[] valueMask = new bool[mask.Length];
            for (int i = 0; i < valueMask.Length; i++)
                valueMask[mask[i]] = GetBit();

            byte[] valueBytes = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
                if (valueMask[i])
                    valueBytes[bytes[i]] = (byte)(_packetReader.ReadByte() ^ 1);

            return BitConverter.ToUInt64(valueBytes, 0);
        }
    }
}

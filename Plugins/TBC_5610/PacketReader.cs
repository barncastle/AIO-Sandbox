using System;
using Common.Cryptography;
using Common.Network;

namespace TBC_5610
{
    public class PacketReader : BasePacketReader
    {
        private const int SHA_DIGEST_LENGTH = 40;

        public PacketReader(byte[] data, bool parse = true) : base(data)
        {
            if (parse)
            {
                Decode(ref data);
                ushort size = BitConverter.ToUInt16(data, 0);
                Size = (ushort)((size >> 8) + ((size & 0xFF) << 8) + 2);
                Opcode = BitConverter.ToUInt32(data, 2);

                Position = 6;
            }
        }

        private void Decode(ref byte[] data)
        {
            if (!ClientAuth.Encode || data.Length < 6)
                return;

            for (int i = 0; i < 6; i++)
            {
                ClientAuth.Key[1] %= SHA_DIGEST_LENGTH;
                byte x = (byte)((data[i] - ClientAuth.Key[0]) ^ ClientAuth.SS_Hash[ClientAuth.Key[1]]);
                ++ClientAuth.Key[1];
                ClientAuth.Key[0] = data[i];
                data[i] = x;
            }
        }
    }
}

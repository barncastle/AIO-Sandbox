using System;
using Common.Cryptography;
using Common.Network;

namespace WotLK_9614
{
    public class PacketReader : BasePacketReader
    {
        private const int SHA_DIGEST_LENGTH = 20;

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

            ClientAuth.PacketCrypt.Decode(data, 6);
        }
    }
}

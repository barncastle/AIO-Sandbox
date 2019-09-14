using System;
using Common.Cryptography;
using Common.Network;

namespace MoP_15464
{
    public class PacketReader : BasePacketReader
    {
        public PacketReader(byte[] data, bool parse = true) : base(data)
        {
            if (parse)
            {
                if (Authenticator.PacketCrypt.Initialised)
                {
                    Decode(ref data, 4);

                    uint header = BitConverter.ToUInt32(data, 0);
                    Size = (header >> 12) + 4u;
                    Opcode = header & 0xFFF;
                }
                else
                {
                    Size = BitConverter.ToUInt16(data, 0) + 2u;
                    Opcode = BitConverter.ToUInt16(data, 2);
                }

                Position = 4;
            }
        }
    }
}

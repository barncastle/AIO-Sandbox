using System;
using System.Security.Cryptography;

namespace Common.Cryptography
{
    public static class HMAC
    {
        public static byte[] ComputeHash(byte[] key, byte[] data)
        {
            byte[] ipad = new byte[64 + data.Length]; // 64 + data
            byte[] opad = new byte[64 + 20]; // 64 + digest

            // fill 64 bytes of same value
            for (int i = 0; i < 64; i++)
            {
                ipad[i] = 0x36;
                opad[i] = 0x5C;
            }

            for (int i = 0; i < 16; i++)
            {
                ipad[i] = (byte)(ipad[i] ^ key[i]);
                opad[i] = (byte)(opad[i] ^ key[i]);
            }

            using (var sha1 = new SHA1Managed())
            {
                Buffer.BlockCopy(data, 0, ipad, 64, data.Length);
                Buffer.BlockCopy(sha1.ComputeHash(ipad), 0, opad, 64, 20);
                return sha1.ComputeHash(opad);
            }
        }
    }
}

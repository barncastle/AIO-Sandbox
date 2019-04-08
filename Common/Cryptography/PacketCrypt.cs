using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Cryptography
{
    public class PacketCrypt
    {
        public byte DigestSize { get; set; } = 20;
        public bool Initialised { get; set; } = false;
        public Action<byte[], int> Encode { get; private set; }
        public Action<byte[], int> Decode { get; private set; } 
        

        private ARC4 ARC4Encrypt;
        private ARC4 ARC4Decrypt;
        private byte[] SessionKey;
        private readonly byte[] Key;

        public PacketCrypt(byte[] sessionkey, uint build)
        {
            SessionKey = sessionkey;
            Key = new byte[4];
            Encode = EncryptImpl;
            Decode = DecryptImpl;

            switch (true)
            {
                case true when build >= 8606 && build < 9614:
                    ApplyHMACKey(sessionkey);
                    break;
                case true when build >= 9614:
                    InitCryptors(sessionkey);
                    break;
            }
        }

        #region Initialisers

        /// <summary>
        /// HMAC traffic key 2.4.3-3.0.9
        /// </summary>
        /// <param name="sessionkey"></param>
        private void ApplyHMACKey(byte[] sessionkey)
        {
            sessionkey = HMAC.ComputeHash(HMAC_Key, sessionkey);
            Array.Resize(ref sessionkey, 40);
            SessionKey = sessionkey;
        }

        /// <summary>
        /// ARC4 encryption 3.1.0+
        /// </summary>
        /// <param name="sessionkey"></param>
        private void InitCryptors(byte[] sessionkey)
        {
            var (EncoderKey, DecoderKey) = LoadKeys();

            ARC4Encrypt = new ARC4();
            ARC4Decrypt = new ARC4();

            ARC4Encrypt.SetKey(HMAC.ComputeHash(EncoderKey, sessionkey));
            ARC4Decrypt.SetKey(HMAC.ComputeHash(DecoderKey, sessionkey));

            // drop 1024 bytes
            ARC4Encrypt.Process(new byte[0x400], 0x400);
            ARC4Decrypt.Process(new byte[0x400], 0x400);

            Encode = ARC4Encrypt.Process;
            Decode = ARC4Decrypt.Process;
        }

        #endregion

        #region Pre-ARC4 Encryption/Decryption

        private void EncryptImpl(byte[] data, int count = 4)
        {
            if (!Initialised || data.Length < count)
                return;

            for (int i = 0; i < count; i++)
            {
                Key[3] %= DigestSize;
                byte x = (byte)((data[i] ^ SessionKey[Key[3]]) + Key[2]);
                ++Key[3];
                data[i] = Key[2] = x;
            }
        }

        private void DecryptImpl(byte[] data, int count = 6)
        {
            if (!Initialised || data.Length < count)
                return;

            for (int i = 0; i < count; i++)
            {
                Key[1] %= DigestSize;
                byte x = (byte)((data[i] - Key[0]) ^ SessionKey[Key[1]]);
                ++Key[1];
                Key[0] = data[i];
                data[i] = x;
            }
        }

        #endregion

        #region Helpers

        private (byte[] EncoderKey, byte[] DecoderKey) LoadKeys()
        {
            var build = ClientAuth.ClientBuild;
            var builds = Keys.Keys.ToArray();

            // 9614 was the first build to utilise this
            if (build < 9614)
                throw new NotSupportedException();

            // find the closest previous build
            for (int i = 1; i < builds.Length; i++)
                if (builds[i] > build)
                    return Keys[builds[i - 1]];

            // use last key pair
            return Keys[builds[builds.Length - 1]];
        }

        #endregion

        #region Encryption Keys

        /// <summary>
        /// TBC hardcoded 16 byte Key located at 0x0088FB3C
        /// </summary>
        private readonly byte[] HMAC_Key = new byte[] { 0x38, 0xA7, 0x83, 0x15, 0xF8, 0x92, 0x25, 0x30, 0x71, 0x98, 0x67, 0xB1, 0x8C, 0x04, 0xE2, 0xAA };

        private readonly Dictionary<uint, (byte[] EncoderKey, byte[] DecoderKey)> Keys = new Dictionary<uint, (byte[], byte[])>()
        {
            // 3.0.1
            [09614] = (new byte[] { 0x22, 0xBE, 0xE5, 0xCF, 0xBB, 0x07, 0x64, 0xD9, 0x00, 0x45, 0x1B, 0xD0, 0x24, 0xB8, 0xD5, 0x45 },
                       new byte[] { 0xF4, 0x66, 0x31, 0x59, 0xFC, 0x83, 0x6E, 0x31, 0x31, 0x02, 0x51, 0xD5, 0x44, 0x31, 0x67, 0x98 }),
            // 3.3.3
            [11643] = (new byte[] { 0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA, 0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57 },
                       new byte[] { 0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5, 0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE }),
        };

        #endregion
    }
}

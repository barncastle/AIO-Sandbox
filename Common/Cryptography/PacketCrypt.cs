using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Cryptography
{
    public class PacketCrypt
    {
        private delegate void CryptHandler(byte[] buffer, int count);

        public byte DigestSize { get; set; } = 20;
        public bool Initialised { get; set; } = false;

        private readonly byte[] SessionKey = new byte[40];
        private readonly byte[] Key = new byte[4];

        private ARC4 ARC4Encrypt;
        private ARC4 ARC4Decrypt;        
        private CryptHandler EncodeHandler;
        private CryptHandler DecodeHandler;

        public PacketCrypt(byte[] sessionkey, uint build)
        {
            sessionkey.CopyTo(SessionKey, 0);

            EncodeHandler = EncodeImpl;
            DecodeHandler = DecodeImpl;

            if (build >= 8606 && build < 9614)
                ApplyHMACKey();
            else if (build >= 9614)
                InitCryptors();
        }

        #region Initialisers

        /// <summary>
        /// HMAC traffic key 2.4.3-3.0.9
        /// </summary>
        /// <param name="sessionkey"></param>
        private void ApplyHMACKey()
        {
            var sessionkey = HMAC.ComputeHash(HMAC_Key, SessionKey);
            Array.Resize(ref sessionkey, 40);
            sessionkey.CopyTo(SessionKey, 0);
        }

        /// <summary>
        /// ARC4 encryption 3.1.0+
        /// </summary>
        /// <param name="sessionkey"></param>
        private void InitCryptors()
        {
            var (EncoderKey, DecoderKey) = LoadKeys();

            ARC4Encrypt = new ARC4();
            ARC4Decrypt = new ARC4();

            ARC4Encrypt.SetKey(HMAC.ComputeHash(EncoderKey, SessionKey));
            ARC4Decrypt.SetKey(HMAC.ComputeHash(DecoderKey, SessionKey));

            // drop 1024 bytes
            ARC4Encrypt.Process(new byte[0x400], 0x400);
            ARC4Decrypt.Process(new byte[0x400], 0x400);

            EncodeHandler = ARC4Encrypt.Process;
            DecodeHandler = ARC4Decrypt.Process;
        }

        #endregion

        #region Encryption/Decryption Methods

        public void Encode(byte[] buffer, int count)
        {
            if (!Initialised || buffer.Length < count)
                return;

            EncodeHandler(buffer, count);
        }

        public void Decode(byte[] buffer, int count)
        {
            if (!Initialised || buffer.Length < count)
                return;

            DecodeHandler(buffer, count);
        }


        /// <summary>
        /// Pre-ARC4 encoding
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        private void EncodeImpl(byte[] data, int count = 4)
        {
            for (int i = 0; i < count; i++)
            {
                Key[3] %= DigestSize;
                byte x = (byte)((data[i] ^ SessionKey[Key[3]]) + Key[2]);
                ++Key[3];
                data[i] = Key[2] = x;
            }
        }
        /// <summary>
        /// Pre-ARC4 decoding
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        private void DecodeImpl(byte[] data, int count = 6)
        {
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

        public void Clear()
        {
            Array.Fill(Key, (byte)0);
            Initialised = false;
        }

        private (byte[] EncoderKey, byte[] DecoderKey) LoadKeys()
        {
            var build = Authenticator.ClientBuild;
            var builds = Keys.Keys.ToArray();

            // 9614 was the first build to utilise this
            if (build < 9614)
                throw new NotSupportedException();

            // conflicting keys hack fix
            switch (build)
            {
                // Cata 4.3.4 conflicts with MoP 5.0.1
                case 15499:
                case 15531:
                case 15595:
                    build = 15354; // 4.3.3
                    break;
            }

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
        private readonly byte[] HMAC_Key = { 0x38, 0xA7, 0x83, 0x15, 0xF8, 0x92, 0x25, 0x30, 0x71, 0x98, 0x67, 0xB1, 0x8C, 0x04, 0xE2, 0xAA };

        /// <summary>
        /// ARC4 Keys per build
        /// </summary>
        private readonly Dictionary<uint, (byte[] EncoderKey, byte[] DecoderKey)> Keys = new Dictionary<uint, (byte[], byte[])>()
        {
            // 3.0.1
            [09614] = (new byte[] { 0x22, 0xBE, 0xE5, 0xCF, 0xBB, 0x07, 0x64, 0xD9, 0x00, 0x45, 0x1B, 0xD0, 0x24, 0xB8, 0xD5, 0x45 },
                       new byte[] { 0xF4, 0x66, 0x31, 0x59, 0xFC, 0x83, 0x6E, 0x31, 0x31, 0x02, 0x51, 0xD5, 0x44, 0x31, 0x67, 0x98 }),
            // 3.3.3
            [11643] = (new byte[] { 0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA, 0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57 },
                       new byte[] { 0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5, 0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE }),

            // 5.0.1
            [15464] = (new byte[] { 0x08, 0xF1, 0x95, 0x9F, 0x47, 0xE5, 0xD2, 0xDB, 0xA1, 0x3D, 0x77, 0x8F, 0x3F, 0x3E, 0xE7, 0x00 },
                       new byte[] { 0x40, 0xAA, 0xD3, 0x92, 0x26, 0x71, 0x43, 0x47, 0x3A, 0x31, 0x08, 0xA6, 0xE7, 0xDC, 0x98, 0x2A }),
        };

        #endregion
    }
}

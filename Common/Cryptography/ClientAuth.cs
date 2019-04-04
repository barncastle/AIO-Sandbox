using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Common.Interfaces;

namespace Common.Cryptography
{
    public static class ClientAuth
    {
        /// <summary>
        /// Login password
        /// </summary>
        public static string Password { get; } = "admin";
        /// <summary>
        /// Preferred account expansion access. 0 = Vanilla, 1 = TBC etc
        /// </summary>
        public static byte ExpansionLevel { get; set; } = 1;

        public static uint ClientBuild { get; set; }
        public static bool Encode { get; set; } = false;
        public static byte[] SS_Hash { get; private set; }
        public static byte[] Key { get; private set; } = new byte[4];

        public static readonly byte[] Reconnect_Challenge =
        {
            0x02, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
            0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x01, 0x02, 0x03, 0x04,
            0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10
        };

        #region Private Vars

        private static readonly byte[] N =
        {
            0x89, 0x4B, 0x64, 0x5E, 0x89, 0xE1, 0x53, 0x5B,
            0xBD, 0xAD, 0x5B, 0x8B, 0x29, 0x06, 0x50, 0x53,
            0x08, 0x01, 0xB1, 0x8E, 0xBF, 0xBF, 0x5E, 0x8F,
            0xAB, 0x3C, 0x82, 0x87, 0x2A, 0x3E, 0x9B, 0xB7
        };

        private static readonly byte[] Salt =
        {
            0xAD, 0xD0, 0x3A, 0x31, 0xD2, 0x71, 0x14, 0x46,
            0x75, 0xF2, 0x70, 0x7E, 0x50, 0x26, 0xB6, 0xD2,
            0xF1, 0x86, 0x59, 0x99, 0x76, 0x02, 0x50, 0xAA,
            0xB9, 0x45, 0xE0, 0x9E, 0xDD, 0x2A, 0xA3, 0x45,
        };

        private static readonly byte[] RN = N.Reverse().ToArray();

        private static BigInteger B;
        private static BigInteger V;
        private static byte[] RB;
        private static BigInteger K;
        private static BigInteger G;
        private static byte[] BUsername;

        #endregion Private Vars

        public static void Clear()
        {
            Key = new byte[4];
            Encode = false;
        }

        public static byte[] LogonChallenge(IPacketReader packet)
        {
            packet.Position = 11;
            ClientBuild = packet.ReadUInt16();

            packet.Position = 33; // Skip to username
            BUsername = packet.ReadBytes(packet.ReadByte()); // Read username
            string username = Encoding.ASCII.GetString(BUsername);

            byte[] x;
            using (SHA1 sha = new SHA1CryptoServiceProvider())
            {
                byte[] user = Encoding.ASCII.GetBytes(username.ToUpper() + ":" + Password.ToUpper());
                byte[] res = Salt.Concat(sha.ComputeHash(user, 0, user.Length)).ToArray();
                x = sha.ComputeHash(res, 0, res.Length).Reverse().ToArray();
            }

            byte[] b = new byte[20];
            new Random().NextBytes(b);
            RB = b.Reverse().ToArray();

            G = new BigInteger(new byte[] { 7 });
            V = G.ModPow(new BigInteger(x), new BigInteger(RN));

            K = new BigInteger(new byte[] { 3 });
            BigInteger temp = (K * V) + G.ModPow(new BigInteger(RB), new BigInteger(RN));
            B = temp % new BigInteger(RN);

            int size = ClientBuild < 5428 ? 118 : 119;

            byte[] result = new byte[size];
            Array.Copy(B.GetBytes(32).Reverse().ToArray(), 0, result, 3, 32);
            result[35] = 1;
            result[36] = 7;
            result[37] = 32;
            Array.Copy(N, 0, result, 38, N.Length);
            Array.Copy(Salt, 0, result, 70, Salt.Length);
            return result;
        }

        public static byte[] LogonProof(IPacketReader packet)
        {
            byte[] A = packet.ReadBytes(32);
            byte[] kM1 = packet.ReadBytes(20);
            byte[] rA = A.Reverse().ToArray();
            byte[] AB = A.Concat(B.GetBytes(32).Reverse()).ToArray();

            if (new BigInteger(A) % new BigInteger(N) == 0)
                return new byte[1];

            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] rU = sha1.ComputeHash(AB).Reverse().ToArray();

            // SS_Hash
            BigInteger s = V.ModPow(new BigInteger(rU), new BigInteger(RN));
            s *= new BigInteger(rA);
            s = s.ModPow(new BigInteger(RB), new BigInteger(RN));

            byte[] S1 = new byte[16];
            byte[] S2 = new byte[16];
            byte[] S = s.GetBytes(32);
            byte[] rS = S.Reverse().ToArray();
            for (int t = 0; t < 16; t++)
            {
                S1[t] = rS[t * 2];
                S2[t] = rS[(t * 2) + 1];
            }

            byte[] hashS1 = sha1.ComputeHash(S1);
            byte[] hashS2 = sha1.ComputeHash(S2);
            SS_Hash = new byte[hashS1.Length + hashS2.Length];
            for (int t = 0; t < hashS1.Length; t++)
            {
                SS_Hash[t * 2] = hashS1[t];
                SS_Hash[(t * 2) + 1] = hashS2[t];
            }

            // calc M1
            byte[] M1;
            byte[] NHash = sha1.ComputeHash(N);
            byte[] GHash = sha1.ComputeHash(G.GetBytes());
            byte[] NG_Hash = new byte[20];

            for (int t = 0; t < 20; t++)
                NG_Hash[t] = (byte)(NHash[t] ^ GHash[t]);

            var tmp = NG_Hash.Concat(sha1.ComputeHash(BUsername))
                             .Concat(Salt)
                             .Concat(A)
                             .Concat(B.GetBytes(32).Reverse())
                             .Concat(SS_Hash);
            M1 = sha1.ComputeHash(tmp.ToArray());

            // calc M2
            byte[] M2;
            tmp = A.Concat(M1).Concat(SS_Hash);
            M2 = sha1.ComputeHash(tmp.ToArray());
            sha1.Dispose();

            // calculate the network hash, 2.4.3+
            CalculateNetworkKey();

            int extradata = 0;
            switch (true)
            {
                case true when ClientBuild < 6178 || ClientBuild == 6180:
                    extradata = 4; // uint unk
                    break;
                case true when ClientBuild < 8089:
                    extradata = 6; // uint unk, ushort unkFlags
                    break;
                default:
                    extradata = 10; // uint account flag, uint surveyId, ushort unkFlags
                    break;
            }

            byte[] result = new byte[22 + extradata];
            result[0] = 1;
            Array.Copy(M2, 0, result, 2, M2.Length);
            return result;
        }

        public static void CalculateNetworkKey()
        {
            if (ClientBuild < 8606)
                return;

            byte[] opad = new byte[64];
            byte[] ipad = new byte[64];

            // hardcoded 16 byte Key located at 0x0088FB3C
            byte[] key = new byte[] { 0x38, 0xA7, 0x83, 0x15, 0xF8, 0x92, 0x25, 0x30, 0x71, 0x98, 0x67, 0xB1, 0x8C, 0x4, 0xE2, 0xAA };
            
            // fill 64 bytes of same value
            for (int i = 0; i < 64; i++)
            {
                opad[i] = 0x5C;
                ipad[i] = 0x36;
            }

            for (int i = 0; i < 16; i++)
            {
                opad[i] = (byte)(opad[i] ^ key[i]);
                ipad[i] = (byte)(ipad[i] ^ key[i]);
            }

            using(var sha1 = new SHA1Managed())
            {
                byte[] digest = sha1.ComputeHash(ipad.Concat(SS_Hash).ToArray());
                var ss_hash = sha1.ComputeHash(opad.Concat(digest).ToArray());
                Array.Resize(ref ss_hash, 40);

                SS_Hash = ss_hash;
            }
        }
    }
}

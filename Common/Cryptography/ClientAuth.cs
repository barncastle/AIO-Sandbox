using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Common.Interfaces;

namespace Common.Cryptography
{
    public static class ClientAuth
    {
        public static string Password { get; set; }
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
        static readonly byte[] N =
        {
            0x89, 0x4B, 0x64, 0x5E, 0x89, 0xE1, 0x53, 0x5B,
            0xBD, 0xAD, 0x5B, 0x8B, 0x29, 0x06, 0x50, 0x53,
            0x08, 0x01, 0xB1, 0x8E, 0xBF, 0xBF, 0x5E, 0x8F,
            0xAB, 0x3C, 0x82, 0x87, 0x2A, 0x3E, 0x9B, 0xB7
        };

        static readonly byte[] Salt =
        {
            0xAD, 0xD0, 0x3A, 0x31, 0xD2, 0x71, 0x14, 0x46,
            0x75, 0xF2, 0x70, 0x7E, 0x50, 0x26, 0xB6, 0xD2,
            0xF1, 0x86, 0x59, 0x99, 0x76, 0x02, 0x50, 0xAA,
            0xB9, 0x45, 0xE0, 0x9E, 0xDD, 0x2A, 0xA3, 0x45,
        };

        static readonly byte[] RN = N.Reverse().ToArray();

        static BigInteger B;
        static BigInteger V;
        static byte[] RB;
        static BigInteger K;
        static BigInteger G;
        static byte[] BUsername;
        #endregion

        public static void Clear()
        {
            Key = new byte[4];
            Encode = false;
        }

        public static byte[] LogonChallenge(IPacketReader packet)
        {
            packet.Position = 11;
            uint build = packet.ReadUInt16();

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

            IEnumerable<byte> result = new byte[3]; // Opcode, 0, Success
            result = result.Concat(B.GetBytes(32).Reverse());
            result = result.Concat(new byte[] { 1, 7, 32 }); // 1, G, 32
            result = result.Concat(N);
            result = result.Concat(Salt);
            result = result.Concat(new byte[(build < 5875 ? 16 : 17)]); // unknown, Security Flag (version?)
            return result.ToArray();
        }

        public static byte[] LogonProof(IPacketReader packet)
        {
            byte[] A = packet.ReadBytes(32);
            byte[] kM1 = packet.ReadBytes(20);
            byte[] rA = A.Reverse().ToArray();
            byte[] AB = A.Concat(B.GetBytes(32).Reverse()).ToArray();

            if (new BigInteger(A) % new BigInteger(N) == 0)
                return new byte[1];

            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] rU = sha.ComputeHash(AB).Reverse().ToArray();

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

            byte[] hashS1 = sha.ComputeHash(S1);
            byte[] hashS2 = sha.ComputeHash(S2);
            SS_Hash = new byte[hashS1.Length + hashS2.Length];
            for (int t = 0; t < hashS1.Length; t++)
            {
                SS_Hash[t * 2] = hashS1[t];
                SS_Hash[(t * 2) + 1] = hashS2[t];
            }

            // calc M1
            byte[] M1;
            byte[] NHash = sha.ComputeHash(N);
            byte[] GHash = sha.ComputeHash(G.GetBytes());
            byte[] NG_Hash = new byte[20];
            for (int t = 0; t < 20; t++)
                NG_Hash[t] = (byte)(NHash[t] ^ GHash[t]);

            IEnumerable<byte> tmp = NG_Hash.Concat(sha.ComputeHash(BUsername));
            tmp = tmp.Concat(Salt);
            tmp = tmp.Concat(A);
            tmp = tmp.Concat(B.GetBytes(32).Reverse());
            tmp = tmp.Concat(SS_Hash);
            M1 = sha.ComputeHash(tmp.ToArray());

            // calc M2
            byte[] M2;
            tmp = A.Concat(M1);
            tmp = tmp.Concat(SS_Hash);
            M2 = sha.ComputeHash(tmp.ToArray());

            sha.Dispose();

            IEnumerable<byte> result = new byte[] { 1, 0 };
            result = result.Concat(M2);
            result = result.Concat(new byte[4]);
            return result.ToArray();
        }
    }
}

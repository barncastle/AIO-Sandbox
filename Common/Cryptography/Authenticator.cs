using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Common.Interfaces;

namespace Common.Cryptography
{
    public static class Authenticator
    {
        /// <summary>
        /// Login password
        /// </summary>
        public static string Password { get; private set; } = "admin";
        /// <summary>
        /// Preferred account expansion access. 0 = Vanilla, 1 = TBC etc
        /// </summary>
        public static byte ExpansionLevel { get; private set; } = 0;
        /// <summary>
        /// Build as sent by the client
        /// </summary>
        public static uint ClientBuild { get; set; }
        /// <summary>
        /// Packet coder/crypt handler
        /// </summary>
        public static PacketCrypt PacketCrypt { get; private set; }

        /// <summary>
        /// TODO fix this at some point
        /// </summary>
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

        #region Methods

        public static void Clear() => PacketCrypt.Clear();

        public static byte[] LogonChallenge(IPacketReader packet)
        {
            packet.Position = 11;
            ClientBuild = packet.ReadUInt16();

            packet.Position = 33; // Skip to username
            BUsername = packet.ReadBytes(packet.ReadByte()); // Read username

            byte[] credshash;
            using (SHA1 sha = new SHA1CryptoServiceProvider())
            {
                string username = Encoding.ASCII.GetString(BUsername);
                byte[] credentials = Encoding.ASCII.GetBytes(username.ToUpper() + ":" + Password.ToUpper());
                byte[] tmp = Salt.Concat(sha.ComputeHash(credentials)).ToArray();
                credshash = sha.ComputeHash(tmp).Reverse().ToArray();
            }

            RB = new byte[20];
            new Random().NextBytes(RB);

            G = new BigInteger(new byte[] { 7 });
            V = G.ModPow(new BigInteger(credshash), new BigInteger(RN));

            K = new BigInteger(new byte[] { 3 });
            B = ((K * V) + G.ModPow(new BigInteger(RB), new BigInteger(RN))) % new BigInteger(RN);

            // create packet data
            byte[] result = new byte[GetLogonChallengeSize()];
            Array.Copy(B.GetBytes(32).Reverse().ToArray(), 0, result, 3, 32);
            result[35] = 1;
            result[36] = 7;
            result[37] = 32;
            Array.Copy(N, 0, result, 38, N.Length);
            Array.Copy(Salt, 0, result, 70, Salt.Length);

            if (result.Length > 0x77)
            {
                result[118] = 2; // security_flags MATRIX_CARD
                result[119] = MatrixCard.Width;
                result[120] = MatrixCard.Height;
                result[121] = MatrixCard.DigitCount;
                result[122] = MatrixCard.ChallengeCount;
                Array.Copy(RB, 0, result, 123, 8); // seed[8]- anything random, I'm arbitrarily using RB
            }

            return result;
        }

        public static byte[] LogonProof(IPacketReader packet)
        {
            byte[] A = packet.ReadBytes(32);
            byte[] kM1 = packet.ReadBytes(20);
            byte[] rA = A.Reverse().ToArray();
            byte[] AB = A.Concat(B.GetBytes(32).Reverse()).ToArray();

            packet.Position = 74;
            byte securityFlags = packet.ReadByte();

            if (new BigInteger(A) % new BigInteger(N) == 0)
                return new byte[1];

            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                // SS_Hash
                byte[] rU = sha1.ComputeHash(AB).Reverse().ToArray();
                var s = V.ModPow(new BigInteger(rU), new BigInteger(RN)) * new BigInteger(rA);
                s = s.ModPow(new BigInteger(RB), new BigInteger(RN));

                byte[] S1 = new byte[16], S2 = new byte[16];
                byte[] rS = s.GetBytes(32).Reverse().ToArray();
                for (int t = 0; t < 16; t++)
                {
                    S1[t] = rS[t * 2];
                    S2[t] = rS[(t * 2) + 1];
                }

                byte[] hashS1 = sha1.ComputeHash(S1), hashS2 = sha1.ComputeHash(S2);
                byte[] ss_hash = new byte[hashS1.Length + hashS2.Length];
                for (int t = 0; t < hashS1.Length; t++)
                {
                    ss_hash[t * 2] = hashS1[t];
                    ss_hash[(t * 2) + 1] = hashS2[t];
                }

                // matrix card                
                if ((securityFlags & 2) == 2)
                {
                    var clientProof = packet.ReadBytes(20).AsSpan();
                    var seed = BitConverter.ToUInt64(RB);

                    var matrix = new MatrixCard();
                    var serverProof = matrix.GenerateServerProof(seed, ss_hash);

                    if (!clientProof.SequenceEqual(serverProof))
                    {
                        return new byte[]
                        {
                            0x1, // LOGIN_PROOF 
                            0x5, // FAIL_UNKNOWN_ACCOUNT
                            0x0, // padding
                            0x0
                        };
                    }
                }

                // calc M1 & M2
                byte[] NHash = sha1.ComputeHash(N);
                byte[] GHash = sha1.ComputeHash(G.GetBytes());

                var tmp = Enumerable.Range(0, 20)
                                    .Select(t => (byte)(NHash[t] ^ GHash[t]))
                                    .Concat(sha1.ComputeHash(BUsername))
                                    .Concat(Salt)
                                    .Concat(A)
                                    .Concat(B.GetBytes(32).Reverse())
                                    .Concat(ss_hash);

                byte[] M1 = sha1.ComputeHash(tmp.ToArray());
                byte[] M2 = sha1.ComputeHash(A.Concat(M1).Concat(ss_hash).ToArray());

                // instantiate coders/cryptors
                PacketCrypt = new PacketCrypt(ss_hash, ClientBuild);

                // create packet data
                byte[] result = new byte[GetLogonProofSize()];
                result[0] = 1;
                Array.Copy(M2, 0, result, 2, M2.Length);
                return result;
            }
        }

        public static void LoadConfig()
        {
            var parser = new INIParser("settings.conf");

            if (parser.TryGetValue("Settings", "Expansion", out byte exp))
                ExpansionLevel = exp;

            if (parser.TryGetValue("Settings", "Password", out string pass))
                Password = pass;
        }

        #endregion

        #region Helpers

        private static int GetLogonChallengeSize()
        {
            return ClientBuild switch
            {
                < 5428 => 0x76, // 1.11.0
                < 5991 => 0x77, // 2.0.0
                6005 => 0x77,   // 1.12.2
                6141 => 0x77,   // 1.12.3
                6180 => 0x77,   // 2.0.1 TBC prepatch
                _ => 0x83
            };
        }

        private static int GetLogonProofSize()
        {
            if (ClientBuild < 6178 || ClientBuild == 6180)
                return 0x16 + 0x4; // + uint unk
            else if (ClientBuild < 8089)
                return 0x16 + 0x6; // + uint unk, ushort unkFlags
            else
                return 0x16 + 0xA; // + uint account flag, uint surveyId, ushort unkFlags
        }

        #endregion
    }
}

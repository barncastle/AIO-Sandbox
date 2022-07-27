using System;
using System.Security.Cryptography;

namespace Common.Cryptography
{
#pragma warning disable IDE0059 // Unnecessary assignment of a value
    public class MatrixCard
    {
        public const byte Width = 8; // SECURITYMATRIX_NUM_COLUMNS in SecurityMatrix.lua
        public const byte Height = 10; // SECURITYMATRIX_NUM_ROWS in SecurityMatrix.lua
        public const byte DigitCount = 2; // explicitly stated as 2 in WoD's SecurityMatrix.lua
        public const byte ChallengeCount = 4; // arbitarily set
    
        private uint[] Coordinates;

        private readonly SHA1 _SHA1 = SHA1.Create();
        private readonly ARC4 _ARC4 = new();
        private readonly byte[] _Key = new byte[16];
        private readonly byte[] _IPad = new byte[64];
        private readonly byte[] _OPad = new byte[64];        
        private readonly byte[] _Buffer = new byte[1];

        public byte[] GenerateServerProof(ulong seed, byte[] sessionKey)
        {
            SetMatrixInfo(seed, sessionKey);

            for(byte i = 0; i < ChallengeCount; i++)
            {
                GetMatrixCoordinates(i, out var x, out var y);

                // at this point on a proper server some process
                // would be used to return/calculate the digits from
                // a server-side copy of the matrix card for [x, y]
                //
                // I'm arbitarily chosing the round number as the code
                // i.e. 00, 11, 22...

                for (byte j = 0; j < DigitCount; j++)
                {
                    // call EnterMatrix on each digit
                    EnterMatrix(i);
                }
            }

            return FinalizeMatrix();
        }

        public void SetMatrixInfo(ulong seed, byte[] sessionKey)
        {
            CalculateCoordinates(seed);

            // md5(seed + sessionKey)
            using var md5 = MD5.Create();
            md5.TransformBlock(BitConverter.GetBytes(seed), 0, 8, null, 0);
            md5.TransformFinalBlock(sessionKey, 0, sessionKey.Length);

            // assign our local key
            Array.Copy(md5.Hash, _Key, md5.Hash.Length);

            // initialise SARC4
            _ARC4.SetKey(_Key);

            // HMAC
            Array.Fill<byte>(_IPad, 0x36);
            Array.Fill<byte>(_OPad, 0x5C);

            for (var i = 0; i < _Key.Length; i++)
            {
                _IPad[i] ^= _Key[i];
                _OPad[i] ^= _Key[i];
            }

            // transform our input
            _SHA1.Initialize();
            _SHA1.TransformBlock(_IPad, 0, _IPad.Length, null, 0);
        }

        public bool GetMatrixCoordinates(uint round, out uint x, out uint y)
        {
            x = y = 0;

            if (round >= ChallengeCount)
                return false;

            var coord = Coordinates[round];
            x = coord % Width;
            y = coord / Width;

            if (y >= Height)
                return false;

            return true;
        }

        public void EnterMatrix(byte value)
        {
            // ARC4 encrypt our input and
            // append the output to our SHA hash
            _Buffer[0] = value;
            _ARC4.Process(_Buffer, 1);
            _SHA1.TransformBlock(_Buffer, 0, 1, null, 0);
        }

        public byte[] FinalizeMatrix()
        {
            // finalise the current hash
            _SHA1.TransformFinalBlock(_Buffer, 0, 0);

            var hash = _SHA1.Hash;

            // compute our proof
            _SHA1.Initialize();
            _SHA1.TransformBlock(_OPad, 0, _OPad.Length, null, 0);
            _SHA1.TransformFinalBlock(hash, 0, hash.Length);

            return _SHA1.Hash;
        }

        private void CalculateCoordinates(ulong seed)
        {
            Coordinates = new uint[ChallengeCount];

            var matrixSize = Width * Height;
            var matrixIndicies = new uint[matrixSize];

            // populate the indicies
            for (var i = 1u; i < matrixSize; i++)
                matrixIndicies[i] = i;

            for (var i = 0u; i < ChallengeCount; i++)
            {
                // calculate our index from the seed
                // and remaining values
                var count = (uint)matrixSize - i;
                var index = (uint)(seed % count);

                Coordinates[i] = matrixIndicies[index];

                // "pop" the selected entry to prevent re-use
                //  e.g. i(1) : [0, 1, 2] => [0, 2, 2]
                for (var j = index; j < count - 1; j++)
                    matrixIndicies[j] = matrixIndicies[j + 1];

                seed /= count;
            }
        }
    }
}

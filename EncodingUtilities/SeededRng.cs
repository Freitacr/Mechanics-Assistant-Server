using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace EncodingUtilities
{
    public class SeededRng
    {
        private ICryptoTransform CurrentAesEncryptor;
        private SHA512 SHA512;
        private byte[] PrevState;
        private byte CurrIndex = 0;

        public SeededRng(byte[] keyIn)
        {
            SHA512 = SHA512.Create();
            byte[] hash = SHA512.ComputeHash(keyIn);
            byte[] upperHash = new byte[32];
            byte[] middleHash = new byte[16];
            byte[] lowerHash = new byte[16];
            PrevState = middleHash;
            int index = 0;
            for (int i = 0; i < upperHash.Length; i++, index++)
                upperHash[i] = hash[index];
            for (int i = 0; i < middleHash.Length; i++, index++)
                middleHash[i] = hash[index];
            for (int i = 0; i < lowerHash.Length; i++, index++)
                lowerHash[i] = hash[index];
            CurrentAesEncryptor = Aes.Create().CreateEncryptor(upperHash, lowerHash);
            UpdateState();
        }

        private void UpdateState()
        {
            byte[] toTrans = new byte[16];
            for (byte b = 0; b < 16; b++)
                toTrans[b] = (byte)(CurrIndex + b);
            byte[] ret = CurrentAesEncryptor.TransformFinalBlock(toTrans, 0, toTrans.Length); //gives a 32 byte value
            byte[] hash = SHA512.ComputeHash(PrevState);
            byte[] middle = new byte[16];
            byte[] lower = new byte[16];
            int index = 32;
            for (int i = 0; i < middle.Length; i++, index++)
                middle[i] = hash[index];
            for (int i = 0; i < lower.Length; i++, index++)
                lower[i] = hash[index];
            PrevState = middle;
            CurrentAesEncryptor = Aes.Create().CreateEncryptor(ret, lower);
        }

        public uint Next(uint maxExclusive)
        {
            byte[] generated = CurrentAesEncryptor.TransformFinalBlock(PrevState, 0, PrevState.Length);
            byte[] long1 = new byte[8];
            byte[] long2 = new byte[8];
            int index = 0;
            for (int i = 0; i < long1.Length; i++, index++)
                long1[i] = generated[index];
            index += 8;
            for(int i = 0; i < long2.Length; i++, index++)
                long2[i] = generated[index];
            ulong val = 0;
            for(int i = 0; i < 8; i++)
            {
                int shift = 56 - (8 * i);
                val |= ((ulong)(long1[i] ^ long2[i]) << shift);
            }
            UpdateState();
            return (uint)(val % maxExclusive);
        }



        public void test()
        {
            
        }
    }
}

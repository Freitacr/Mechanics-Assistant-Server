using System;

namespace OMISSecLib
{
    public class SecuritySchemaLib
    {
        public Func<byte[], byte[]> HashFunction;

        public SecuritySchemaLib(Func<byte[], byte[]> hashFunction)
        {
            HashFunction = hashFunction;
        }

        public byte[] ConstructDerivedSecurityToken(byte[] a, byte[] b)
        {
            byte[] aHash = HashFunction(a);
            byte[] bHash = HashFunction(b);
            int length = aHash.Length;
            PadTokens(a, b, length, out byte[] aPad, out byte[] bPad);
            byte[] ret = new byte[length];
            for(int i = 0; i < length; i++)
            {
                bool flag = ((aPad[i] ^ bPad[i]) & 1) == 1;
                if (flag)
                    ret[i] = (byte)(aHash[i] * bHash[i]);
                else
                    ret[i] = (byte)(aHash[i] + bHash[i]);
            }
            return ret;
        }


        private static void PadTokens(byte[] a, byte[] b, int length, out byte[] aPad, out byte[] bPad)
        {
            aPad = new byte[length];
            bPad = new byte[length];
            int aLength = a.Length;
            int bLength = b.Length;
            for(int i = 0; i < length; i++)
            {
                aPad[i] = a[i % aLength];
                bPad[i] = b[i % bLength];
            }
        }

        public byte[] ConstructUserEncryptionKey(byte[] derivedSecurityToken, byte[] c)
        {
            byte[] cHash = HashFunction(c);
            int length = derivedSecurityToken.Length;
            byte[] ret = new byte[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = (byte)(derivedSecurityToken[i] + cHash[i]);
            }
            return ret;
        }
    }
}

using NUnit.Framework;
using System.Security.Cryptography;
using OMISSecLib;

namespace Tests
{
    public class SecuritySchemaLibTest
    {

        private static byte[] A = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        private static byte[] B = new byte[] { 11, 13, 15, 17, 19, 21, 23, 25, 27 };
        private static byte[] C = new byte[] { 8, 2, 4, 6, 10, 12, 73 };
        private static byte[] ExpectedDerivedToken = new byte[]
        {
            50, 92, 106, 87, 241, 129, 66, 42, 91, 52, 150,
            248, 6, 59, 172, 30, 24, 54, 240, 94, 167, 101,
            45, 208, 21, 49, 5, 141, 134, 206, 141, 125
        };

        private static byte[] ExpectedUserEncryptionKey = new byte[]
        {
            33, 196, 149, 163, 136,  58, 128,  18, 139, 185, 219,  88,  52,
            110, 153, 237, 182, 169,  39, 142, 189,   0,  86, 244, 227, 235,
            76,   5, 222, 120,  67, 215
        };

        private SecuritySchemaLib Lib; 

        [SetUp]
        public void Setup()
        {
            Lib = new SecuritySchemaLib(SHA256.Create().ComputeHash);
        }

        [Test]
        public void TestCalculateDerivedToken()
        {
            var ActualDerivedToken = Lib.ConstructDerivedSecurityToken(A, B);
            Assert.AreEqual(ActualDerivedToken.Length, ExpectedDerivedToken.Length);
            for(int i = 0; i < ActualDerivedToken.Length; i++)
                Assert.AreEqual(ExpectedDerivedToken[i], ActualDerivedToken[i]);
        }

        [Test]
        public void TestCalculateUserEncryptionKey()
        {
            var ActualEncyrptionKey = Lib.ConstructUserEncryptionKey(ExpectedDerivedToken, C);
            Assert.AreEqual(ExpectedUserEncryptionKey.Length, ActualEncyrptionKey.Length);
            for (int i = 0; i < ActualEncyrptionKey.Length; i++)
                Assert.AreEqual(ExpectedUserEncryptionKey[i], ActualEncyrptionKey[i]);
        }
    }
}
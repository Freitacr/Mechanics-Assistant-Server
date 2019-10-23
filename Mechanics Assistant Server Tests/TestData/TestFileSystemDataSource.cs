using System;
using System.Collections.Generic;
using EncodingUtilities;
using ANSEncodingLib;
using System.IO;
using OldManinTheShopServer.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MechanicsAssistantServerTests.TestData
{
    [TestClass]
    public class TestFileSystemDataSource
    {

        private static readonly string KeywordDataFilePath = "keywords.ans";
        private static readonly string MechanicQueryFilePath = "queries.ans";
        private FileSystemDataSource Src;

        [ClassCleanup]
        public static void ClassClean()
        {
            File.Delete(KeywordDataFilePath);
            File.Delete(MechanicQueryFilePath);
        }

        private void WriteInitialFiles()
        {
            byte[] memoryStore = new byte[] { 91, 93 };
            MemoryStream toWrite = new MemoryStream(memoryStore);
            BitWriter writerOut = new BitWriter(
                new FileStream(KeywordDataFilePath, FileMode.Create, FileAccess.Write)
            );
            AnsBlockEncoder encoder = new AnsBlockEncoder(1048576, writerOut);
            encoder.EncodeStream(toWrite, 8);
            writerOut.Flush();
            writerOut.Close();
            toWrite = new MemoryStream(memoryStore);
            writerOut = new BitWriter(
                new FileStream(MechanicQueryFilePath, FileMode.Create, FileAccess.Write)
            );
            encoder = new AnsBlockEncoder(1048576, writerOut);
            encoder.EncodeStream(toWrite, 8);
            writerOut.Flush();
            writerOut.Close();
        }

        [TestInitialize]
        public void Init()
        {
            WriteInitialFiles();
            Src = new FileSystemDataSource()
            {
                KeywordDataFilePath = KeywordDataFilePath,
                MechanicQueryFilePath = MechanicQueryFilePath
            };
        }

        [TestMethod]
        public void TestReadInitial()
        {
            var examples =  Src.LoadKeywordTrainingExamples();
            Assert.AreEqual(examples.Count, 0);
            var queries = Src.LoadMechanicQueries();
            Assert.AreEqual(queries.Count, 0);
        }
        
        [TestMethod]
        public void TestAddMechanicQuery()
        {
            MechanicQuery q = new MechanicQuery("autocar", "xpeditor", "bad icm", null, "runs rough");
            Assert.IsTrue(Src.AddData(q));
            var queries = Src.LoadMechanicQueries();
            Assert.AreEqual(queries.Count, 1);
            Assert.IsTrue(q.Equals(queries[0]));
        }

        [TestMethod]
        public void TestAddKeywordExample()
        {
            KeywordTrainingExample.KeywordPair p1 = new KeywordTrainingExample.KeywordPair("bad", "adj");
            KeywordTrainingExample q = new KeywordTrainingExample(new List<KeywordTrainingExample.KeywordPair>() { p1 }, true);
            Assert.IsTrue(Src.AddKeywordExample(q));
            var examples = Src.LoadKeywordTrainingExamples();
            Assert.AreEqual(examples.Count, 1);
            Assert.IsTrue(q.Equals(examples[0]));
        }
    }
}

using System;
using System.Collections.Generic;
using MechanicsAssistantServer.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MechanicsAssistantServerTests.TestData
{
    [TestClass]
    public class TestHttpUri
    {


        [TestMethod]
        public void TestNoPortSpecification()
        {
            string uri = "localhost/";
            var http = new HttpUri("http://" + uri);
            Assert.AreEqual(http.Hostname, "localhost");
            Assert.AreEqual(http.Location, "/");
            Assert.AreEqual(http.Port, "80");
            Assert.IsTrue(http.UsedDefaultPort);
            Assert.AreEqual(http.Protocol, "http://");

            var https = new HttpUri("https://" + uri);
            Assert.AreEqual(https.Hostname, "localhost");
            Assert.AreEqual(https.Location, "/");
            Assert.AreEqual(https.Port, "443");
            Assert.IsTrue(https.UsedDefaultPort);
            Assert.AreEqual(https.Protocol, "https://");
        }

        [TestMethod]
        public void TestNonTopLevel()
        {
            string uri = "localhost/query/adj";
            var http = new HttpUri("http://" + uri);
            Assert.AreEqual(http.Hostname, "localhost");
            Assert.AreEqual(http.Location, "/query/adj/");
            Assert.AreEqual(http.Port, "80");
            Assert.IsTrue(http.UsedDefaultPort);
            Assert.AreEqual(http.Protocol, "http://");

            var https = new HttpUri("https://" + uri);
            Assert.AreEqual(https.Hostname, "localhost");
            Assert.AreEqual(https.Location, "/query/adj/");
            Assert.AreEqual(https.Port, "443");
            Assert.IsTrue(https.UsedDefaultPort);
            Assert.AreEqual(https.Protocol, "https://");
        }

        [TestMethod]
        public void TestPortSpecification()
        {
            string uri = "localhost:43/query/adj";
            var http = new HttpUri("http://" + uri);
            Assert.AreEqual(http.Hostname, "localhost");
            Assert.AreEqual(http.Location, "/query/adj/");
            Assert.AreEqual(http.Port, "43");
            Assert.IsTrue(!http.UsedDefaultPort);
            Assert.AreEqual(http.Protocol, "http://");

            var https = new HttpUri("https://" + uri);
            Assert.AreEqual(https.Hostname, "localhost");
            Assert.AreEqual(https.Location, "/query/adj/");
            Assert.AreEqual(https.Port, "43");
            Assert.IsTrue(!https.UsedDefaultPort);
            Assert.AreEqual(https.Protocol, "https://");
        }

        [TestMethod]
        public void TestWildcardHostname()
        {
            string uri = "+/query/adj";
            var http = new HttpUri("http://" + uri);
            Assert.AreEqual(http.Hostname, "+");
            Assert.AreEqual(http.Location, "/query/adj/");
            Assert.AreEqual(http.Port, "80");
            Assert.IsTrue(http.UsedDefaultPort);
            Assert.AreEqual(http.Protocol, "http://");

            var https = new HttpUri("https://" + uri);
            Assert.AreEqual(https.Hostname, "+");
            Assert.AreEqual(https.Location, "/query/adj/");
            Assert.AreEqual(https.Port, "443");
            Assert.IsTrue(https.UsedDefaultPort);
            Assert.AreEqual(https.Protocol, "https://");
        }

        [TestMethod]
        public void TestGeneratePrefix()
        {
            var http = new HttpUri("http://+:80/query/adj");
            var http2 = new HttpUri("http://+/query/adj");

            Assert.AreEqual("http://+/query/adj/", http2.Prefix);
            Assert.AreEqual("http://+:80/query/adj/", http.Prefix);
        }


        [TestMethod]
        public void TestInvalidProtocol()
        {
            try
            {
                HttpUri uri = new HttpUri("localhost:80/");

                Assert.Fail("Expected a UriFormatException, did not receive one.");
            } catch (UriFormatException)
            {
                //all ok
            }

            try
            {
                HttpUri uri = new HttpUri("html://localhost:80/");

                Assert.Fail("Expected a UriFormatException, did not receive one.");
            }
            catch (UriFormatException)
            {
                //all ok
            }

            try
            {
                HttpUri uri = new HttpUri("https:/localhost:80/");

                Assert.Fail("Expected a UriFormatException, did not receive one.");
            }
            catch (UriFormatException)
            {
                //all ok
            }
        }



        [TestMethod]
        public void TestIsPrefix()
        {
            HttpUri uri1 = new HttpUri("https://localhost:80/");
            HttpUri uri2 = new HttpUri("https://localhost/");
            HttpUri uri3 = new HttpUri("https://localhost:80/os");
            HttpUri uri4 = new HttpUri("https://localhost:80/os/av");
            HttpUri uri5 = new HttpUri("http://localhost/");
            HttpUri uri6 = new HttpUri("http://localhost/os");

            Assert.IsTrue(uri1.IsPrefixOf(uri3));
            Assert.IsTrue(uri1.IsPrefixOf(uri4));
            Assert.IsTrue(uri3.IsPrefixOf(uri4));
            Assert.IsFalse(uri3.IsPrefixOf(uri1));
            Assert.IsFalse(uri4.IsPrefixOf(uri1));
            Assert.IsFalse(uri2.IsPrefixOf(uri1));
            Assert.IsTrue(uri5.IsPrefixOf(uri6));
            Assert.IsTrue(uri6.IsPrefixOf(uri6));
            Assert.IsFalse(uri2.IsPrefixOf(uri5));
        }
    }
}

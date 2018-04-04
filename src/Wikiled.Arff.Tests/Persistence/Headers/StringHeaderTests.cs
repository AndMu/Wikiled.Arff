using System.IO;
using NUnit.Framework;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Tests.Persistence.Headers
{
    [TestFixture]
    public class StringHeaderTests
    {
        private StringHeader header;

        [SetUp]
        public void Setup()
        {
            header = new StringHeader(1, "Test");
        }

        [Test]
        public void Test()
        {
            Assert.AreEqual("Test", header.Name);
            Assert.AreEqual("@ATTRIBUTE Test STRING", header.ToString());
            Assert.AreEqual(1, header.Index);
        }

         [Test]
        public void CheckSupport()
        {
            header.CheckSupport("Test");
            Assert.Throws<InvalidDataException>(() => header.CheckSupport(1));
        }

        [Test]
        public void Clone()
        {
            var result = header.Clone();
            Assert.AreNotSame(result, header);
            Assert.IsInstanceOf<StringHeader>(result);
        }
    }
}

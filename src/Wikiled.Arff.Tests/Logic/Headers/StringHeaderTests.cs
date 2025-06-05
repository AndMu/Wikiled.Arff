using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Wikiled.Arff.Logic.Headers;

namespace Wikiled.Arff.Tests.Logic.Headers
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
            ClassicAssert.AreEqual("Test", header.Name);
            ClassicAssert.AreEqual("@ATTRIBUTE Test STRING", header.ToString());
            ClassicAssert.AreEqual(1, header.Index);
        }

         [Test]
        public void CheckSupport()
        {
            header.CheckSupport("Test");
            ClassicAssert.Throws<InvalidDataException>(() => header.CheckSupport(1));
        }

        [Test]
        public void Clone()
        {
            var result = header.Clone();
            ClassicAssert.AreNotSame(result, header);
            ClassicAssert.IsInstanceOf<StringHeader>(result);
        }
    }
}

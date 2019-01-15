using System;
using System.IO;
using NUnit.Framework;
using Wikiled.Arff.Logic;
using Wikiled.Arff.Logic.Headers;

namespace Wikiled.Arff.Tests.Logic.Headers
{
    [TestFixture]
    public class NominalHeaderTests
    {
        private NominalHeader header;

        private DataRecord record;

        [SetUp]
        public void Setup()
        {
            header = new NominalHeader(1, "Test", new[] { "one", "two" });
            record = new DataRecord(header);
        }

        [Test]
        public void Test()
        {
            Assert.AreEqual("Test", header.Name);
            Assert.AreEqual("@ATTRIBUTE Test {one, two}", header.ToString());
            Assert.AreEqual(1, header.Index);
        }

        [TestCase("one", 0)]
        [TestCase("two", 1)]
        public void ReadClassIdValue(string value, int expected)
        {
            Assert.Throws<ArgumentNullException>(() => header.ReadClassIdValue(null));
            record.Value = value;
            int result = header.ReadClassIdValue(record);
            Assert.AreEqual(expected, result);
        }

        [TestCase(0, "one")]
        [TestCase(1, "two")]
        public void GetValueByClassId(int value, string expected)
        {
            var result = header.GetValueByClassId(value);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CheckSupport()
        {
            header.CheckSupport("one");
            Assert.Throws<InvalidDataException>(() => header.CheckSupport("test"));
        }

        [Test]
        public void Clone()
        {
            var result = header.Clone();
            Assert.AreNotSame(result, header);
            Assert.IsInstanceOf<NominalHeader>(result);
        }
    }
}

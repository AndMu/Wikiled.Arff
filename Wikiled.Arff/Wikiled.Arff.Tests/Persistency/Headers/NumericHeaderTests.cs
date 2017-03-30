using System;
using System.IO;
using NUnit.Framework;
using Wikiled.Arff.Persistence;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Tests.Persistency.Headers
{
    [TestFixture]
    public class NumericHeaderTests
    {
        private NumericHeader header;

        private DataRecord record;

        [SetUp]
        public void Setup()
        {
            header = new NumericHeader(1, "Test");
            record = new DataRecord(header);
        }

        [Test]
        public void Test()
        {
            Assert.AreEqual("Test", header.Name);
            Assert.AreEqual("@ATTRIBUTE Test NUMERIC", header.ToString());
            Assert.AreEqual(1, header.Index);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void ReadClassIdValue(int value)
        {
            Assert.Throws<ArgumentNullException>(() => header.ReadClassIdValue(null));
            record.Value = value;
            int result = header.ReadClassIdValue(record);
            Assert.AreEqual(value, result);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void GetValueByClassId(int value)
        {
            var result = header.GetValueByClassId(value);
            Assert.AreEqual(value, result);
        }

        [Test]
        public void CheckSupport()
        {
            header.CheckSupport(1);
            Assert.Throws<InvalidDataException>(() => header.CheckSupport("test"));
        }

        [Test]
        public void Clone()
        {
            var result = header.Clone();
            Assert.AreNotSame(result, header);
            Assert.IsInstanceOf<NumericHeader>(result);
        }
    }
}

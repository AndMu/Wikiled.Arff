using System;
using System.IO;
using NUnit.Framework;
using Wikiled.Arff.Logic;
using Wikiled.Arff.Logic.Headers;

namespace Wikiled.Arff.Tests.Logic.Headers
{
    [TestFixture]
    public class EnumNominalHeaderTests
    {
        private EnumNominalHeader header;

        private DataRecord record;

        [SetUp]
        public void Setup()
        {
            header = new EnumNominalHeader(1, "Test", typeof(PositivityType));
            record = new DataRecord(header);
        }

        [Test]
        public void Test()
        {
            Assert.AreEqual("Test", header.Name);
            Assert.AreEqual("@ATTRIBUTE Test {Negative, Neutral, Positive}", header.ToString());
            Assert.AreEqual(1, header.Index);
        }

        [TestCase(PositivityType.Negative, -1)]
        [TestCase(PositivityType.Neutral, 0)]
        [TestCase(PositivityType.Positive, 1)]
        public void ReadClassIdValue(PositivityType value, int expected)
        {
            Assert.Throws<ArgumentNullException>(() => header.ReadClassIdValue(null));
            record.Value = value;
            int result = header.ReadClassIdValue(record);
            Assert.AreEqual(expected, result);
        }

        [TestCase(0, PositivityType.Neutral)]
        [TestCase(1, PositivityType.Positive)]
        [TestCase(-1, PositivityType.Negative)]
        public void GetValueByClassId(int value, PositivityType expected)
        {
            var result = header.GetValueByClassId(value);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CheckSupport()
        {
            header.CheckSupport(PositivityType.Neutral);
            Assert.Throws<InvalidDataException>(() => header.CheckSupport("test"));
        }

        [Test]
        public void Clone()
        {
            var result = header.Clone();
            Assert.AreNotSame(result, header);
            Assert.IsInstanceOf<EnumNominalHeader>(result);
        }
    }
}

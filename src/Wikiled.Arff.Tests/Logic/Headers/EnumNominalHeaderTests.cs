using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            ClassicAssert.AreEqual("Test", header.Name);
            ClassicAssert.AreEqual("@ATTRIBUTE Test {Negative, Neutral, Positive}", header.ToString());
            ClassicAssert.AreEqual(1, header.Index);
        }

        [TestCase(PositivityType.Negative, -1)]
        [TestCase(PositivityType.Neutral, 0)]
        [TestCase(PositivityType.Positive, 1)]
        public void ReadClassIdValue(PositivityType value, int expected)
        {
            ClassicAssert.Throws<ArgumentNullException>(() => header.ReadClassIdValue(null));
            record.Value = value;
            int result = header.ReadClassIdValue(record);
            ClassicAssert.AreEqual(expected, result);
        }

        [TestCase(0, PositivityType.Neutral)]
        [TestCase(1, PositivityType.Positive)]
        [TestCase(-1, PositivityType.Negative)]
        public void GetValueByClassId(int value, PositivityType expected)
        {
            var result = header.GetValueByClassId(value);
            ClassicAssert.AreEqual(expected, result);
        }

        [Test]
        public void CheckSupport()
        {
            header.CheckSupport(PositivityType.Neutral);
            ClassicAssert.Throws<InvalidDataException>(() => header.CheckSupport("test"));
        }

        [Test]
        public void Clone()
        {
            var result = header.Clone();
            ClassicAssert.AreNotSame(result, header);
            ClassicAssert.IsInstanceOf<EnumNominalHeader>(result);
        }
    }
}

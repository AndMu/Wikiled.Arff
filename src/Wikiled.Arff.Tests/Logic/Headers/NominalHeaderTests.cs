using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            ClassicAssert.AreEqual("Test", header.Name);
            ClassicAssert.AreEqual("@ATTRIBUTE Test {one, two}", header.ToString());
            ClassicAssert.AreEqual(1, header.Index);
        }

        [TestCase("one", 0)]
        [TestCase("two", 1)]
        public void ReadClassIdValue(string value, int expected)
        {
            ClassicAssert.Throws<ArgumentNullException>(() => header.ReadClassIdValue(null));
            record.Value = value;
            int result = header.ReadClassIdValue(record);
            ClassicAssert.AreEqual(expected, result);
        }

        [TestCase(0, "one")]
        [TestCase(1, "two")]
        public void GetValueByClassId(int value, string expected)
        {
            var result = header.GetValueByClassId(value);
            ClassicAssert.AreEqual(expected, result);
        }

        [Test]
        public void CheckSupport()
        {
            header.CheckSupport("one");
            ClassicAssert.Throws<InvalidDataException>(() => header.CheckSupport("test"));
        }

        [Test]
        public void Clone()
        {
            var result = header.Clone();
            ClassicAssert.AreNotSame(result, header);
            ClassicAssert.IsInstanceOf<NominalHeader>(result);
        }
    }
}

using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Wikiled.Arff.Logic;
using Wikiled.Arff.Logic.Headers;

namespace Wikiled.Arff.Tests.Logic.Headers
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
            ClassicAssert.AreEqual("Test", header.Name);
            ClassicAssert.AreEqual("@ATTRIBUTE Test NUMERIC", header.ToString());
            ClassicAssert.AreEqual(1, header.Index);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void ReadClassIdValue(int value)
        {
            ClassicAssert.Throws<ArgumentNullException>(() => header.ReadClassIdValue(null));
            record.Value = value;
            int result = header.ReadClassIdValue(record);
            ClassicAssert.AreEqual(value, result);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void GetValueByClassId(int value)
        {
            var result = header.GetValueByClassId(value);
            ClassicAssert.AreEqual(value, result);
        }

        [TestCase(0, true, "")]
        [TestCase(0, false, "0")]
        [TestCase(1, true, "1")]
        [TestCase(1, false, "1")]
        [TestCase(null, true, "")]
        [TestCase(null, false, "")]
        public void ReadValue(int? value, bool isSparse, string expected)
        {
            DataRecord record = new DataRecord(header);
            record.Value = value;
            header.IsSparse = isSparse;
            var result = header.ReadValue(record);
            ClassicAssert.AreEqual(expected, result);
        }

        [TestCase(0, true, "")]
        [TestCase(0, false, "0")]
        [TestCase(1, true, "1")]
        [TestCase(1, false, "1")]
        public void ReadCountValue(int value, bool isSparse, string expected)
        {
            DataRecord record = new DataRecord(header);
            header.IsSparse = isSparse;
            header.UseCount = true;
            for (int i = 0; i < value; i++)
            {
                record.Increment();
            }

            var result = header.ReadValue(record);
            ClassicAssert.AreEqual(expected, result);
        }

        [Test]
        public void CheckSupport()
        {
            header.CheckSupport(1);
            ClassicAssert.Throws<InvalidDataException>(() => header.CheckSupport("test"));
        }

        [Test]
        public void Clone()
        {
            var result = header.Clone();
            ClassicAssert.AreNotSame(result, header);
            ClassicAssert.IsInstanceOf<NumericHeader>(result);
        }
    }
}

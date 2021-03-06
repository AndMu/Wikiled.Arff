﻿using System;
using System.IO;
using NUnit.Framework;
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
            Assert.AreEqual(expected, result);
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
            Assert.AreEqual(expected, result);
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

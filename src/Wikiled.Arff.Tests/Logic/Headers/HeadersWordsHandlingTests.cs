using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Wikiled.Arff.Logic.Headers;

namespace Wikiled.Arff.Tests.Logic.Headers
{
    [TestFixture]
    public class HeadersWordsHandlingTests
    {
        private HeadersWordsHandling handler;

        [SetUp]
        public void Setup()
        {
            handler = new HeadersWordsHandling();
        }

        [Test]
        public void Clone()
        {
            handler.RegisterNumericClass();
            handler.RegisterNominal("Another", "One", "Two");
            var result = (HeadersWordsHandling)handler.Clone();
            ClassicAssert.AreEqual(2, result.Total);
            ClassicAssert.IsInstanceOf<NumericHeader>(result.Class);
            ClassicAssert.AreNotSame(handler.Class, result.Class);
        }

        [Test]
        public void RegisterNumeric()
        {
            var value = handler.RegisterNumeric("1 2 3 Lie's");
            ClassicAssert.AreEqual("1 2 3 Lie's", value.Name);
            ClassicAssert.AreEqual(1, handler.Total);
        }

        [Test]
        public void NonRegisterNumeric()
        {
            handler.Register = false;
            var value = handler.RegisterNumeric("1");
            ClassicAssert.AreEqual("1", value.Name);
            ClassicAssert.AreEqual(0, handler.Total);
        }

        [Test]
        public void RegisterNumericClass()
        {
            var value = handler.RegisterNumericClass();
            ClassicAssert.AreEqual("CLASS", value.Name);
        }

        [Test]
        public void RegisterNominalClass()
        {
            var value = handler.RegisterNominalClass("1", "2");
            ClassicAssert.AreEqual("CLASS", value.Name);
            ClassicAssert.AreEqual(2, value.Nominals.Length);
        }

        [Test]
        public void RegisterNominal()
        {
            var value = handler.RegisterNominal("x", "1", "2");
            ClassicAssert.AreEqual("x", value.Name);
            ClassicAssert.AreEqual(2, value.Nominals.Length);
            ClassicAssert.AreEqual("1", value.Nominals[0]);
            ClassicAssert.AreEqual("2", value.Nominals[1]);
        }

        [Test]
        public void Multiple()
        {
            var value = handler.RegisterNumeric("1");
            ClassicAssert.AreEqual("1", value.Name);
            var value2 = handler.RegisterNumeric("1");
            ClassicAssert.AreEqual("1", value.Name);
            ClassicAssert.AreSame(value, value2);
        }

        [Test]
        public void Parse()
        {
            var line = handler.Parse("@ATTRIBUTE Test NUMERIC");
            ClassicAssert.IsInstanceOf<NumericHeader>(line);
            ClassicAssert.AreEqual("Test", line.Name);

            NominalHeader lineTwo = (NominalHeader)handler.Parse("@ATTRIBUTE Test2 {One, Two}");
            ClassicAssert.IsInstanceOf<NominalHeader>(lineTwo);
            ClassicAssert.AreEqual("Test2", lineTwo.Name);
            ClassicAssert.AreEqual(2, lineTwo.Nominals.Length);
            ClassicAssert.AreEqual("One", lineTwo.Nominals[0]);
            ClassicAssert.AreEqual("Two", lineTwo.Nominals[1]);

            line = handler.Parse("@ATTRIBUTE class NUMERIC");
            ClassicAssert.AreEqual("CLASS", line.Name);
        }

        [Test]
        public void ParseSameLineTwice()
        {
            var line = handler.Parse("@ATTRIBUTE Test NUMERIC");
            ClassicAssert.IsInstanceOf<NumericHeader>(line);
            ClassicAssert.AreEqual("Test", line.Name);
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => handler.Parse("@ATTRIBUTE Test {One, Two}"));
        }

        [Test]
        public void ParseQuote()
        {
            var line = handler.Parse("@ATTRIBUTE 'Test duo' NUMERIC");
            ClassicAssert.IsInstanceOf<NumericHeader>(line);
            ClassicAssert.AreEqual("Test duo", line.Name);
        }

        [Test]
        public void Total()
        {
            ClassicAssert.AreEqual(0, handler.Total);
            handler.RegisterNumeric("a");
            ClassicAssert.AreEqual(1, handler.Total);
            handler.RegisterNumeric("b");
            ClassicAssert.AreEqual(2, handler.Total);
        }

        [Test]
        public void GetByIndex()
        {
            handler.RegisterNumeric("a");
            IHeader header = handler.GetByIndex(0);
            ClassicAssert.AreEqual("a", header.Name);
        }

        [Test]
        public void Enumerator()
        {
            handler.RegisterNumeric("a");
            handler.RegisterNumeric("b");
            int i = handler.Count();
            ClassicAssert.AreEqual(2, i);
        }
    }
}

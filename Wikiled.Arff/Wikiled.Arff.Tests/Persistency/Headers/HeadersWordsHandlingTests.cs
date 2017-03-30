using System;
using System.Linq;
using NUnit.Framework;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Tests.Persistency.Headers
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
            Assert.AreEqual(2, result.Total);
            Assert.IsInstanceOf<NumericHeader>(result.Class);
            Assert.AreNotSame(handler.Class, result.Class);
        }

        [Test]
        public void RegisterNumeric()
        {
            var value = handler.RegisterNumeric("1 2 3 Lie's");
            Assert.AreEqual("1 2 3 Lie's", value.Name);
            Assert.AreEqual(1, handler.Total);
        }

        [Test]
        public void NonRegisterNumeric()
        {
            handler.Register = false;
            var value = handler.RegisterNumeric("1");
            Assert.AreEqual("1", value.Name);
            Assert.AreEqual(0, handler.Total);
        }

        [Test]
        public void RegisterNumericClass()
        {
            var value = handler.RegisterNumericClass();
            Assert.AreEqual("class", value.Name);
        }

        [Test]
        public void RegisterNominalClass()
        {
            var value = handler.RegisterNominalClass("1", "2");
            Assert.AreEqual("class", value.Name);
            Assert.AreEqual(2, value.Nominals.Length);
        }

        [Test]
        public void RegisterNominal()
        {
            var value = handler.RegisterNominal("x", "1", "2");
            Assert.AreEqual("x", value.Name);
            Assert.AreEqual(2, value.Nominals.Length);
            Assert.AreEqual("1", value.Nominals[0]);
            Assert.AreEqual("2", value.Nominals[1]);
        }

        [Test]
        public void Multiple()
        {
            var value = handler.RegisterNumeric("1");
            Assert.AreEqual("1", value.Name);
            var value2 = handler.RegisterNumeric("1");
            Assert.AreEqual("1", value.Name);
            Assert.AreSame(value, value2);
        }

        [Test]
        public void Parse()
        {
            var line = handler.Parse("@ATTRIBUTE Test NUMERIC");
            Assert.IsInstanceOf<NumericHeader>(line);
            Assert.AreEqual("Test", line.Name);

            NominalHeader lineTwo = (NominalHeader)handler.Parse("@ATTRIBUTE Test2 {One, Two}");
            Assert.IsInstanceOf<NominalHeader>(lineTwo);
            Assert.AreEqual("Test2", lineTwo.Name);
            Assert.AreEqual(2, lineTwo.Nominals.Length);
            Assert.AreEqual("One", lineTwo.Nominals[0]);
            Assert.AreEqual("Two", lineTwo.Nominals[1]);

            line = handler.Parse("@ATTRIBUTE class NUMERIC");
            Assert.AreEqual("class", line.Name);
        }

        [Test]
        public void ParseSameLineTwice()
        {
            var line = handler.Parse("@ATTRIBUTE Test NUMERIC");
            Assert.IsInstanceOf<NumericHeader>(line);
            Assert.AreEqual("Test", line.Name);
            Assert.Throws<ArgumentOutOfRangeException>(() => handler.Parse("@ATTRIBUTE Test {One, Two}"));
        }

        [Test]
        public void ParseQuote()
        {
            var line = handler.Parse("@ATTRIBUTE 'Test duo' NUMERIC");
            Assert.IsInstanceOf<NumericHeader>(line);
            Assert.AreEqual("Test duo", line.Name);
        }

        [Test]
        public void Total()
        {
            Assert.AreEqual(0, handler.Total);
            handler.RegisterNumeric("a");
            Assert.AreEqual(1, handler.Total);
            handler.RegisterNumeric("b");
            Assert.AreEqual(2, handler.Total);
        }

        [Test]
        public void GetByIndex()
        {
            handler.RegisterNumeric("a");
            IHeader header = handler.GetByIndex(0);
            Assert.AreEqual("a", header.Name);
        }

        [Test]
        public void Enumerator()
        {
            handler.RegisterNumeric("a");
            handler.RegisterNumeric("b");
            int i = handler.Count();
            Assert.AreEqual(2, i);
        }
    }
}

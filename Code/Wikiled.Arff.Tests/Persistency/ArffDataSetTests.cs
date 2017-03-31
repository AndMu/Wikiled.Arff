using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Wikiled.Arff.Normalization;
using Wikiled.Arff.Persistence;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Tests.Persistency
{
    [TestFixture]
    public class ArffDataHolderTests
    {
        [Test]
        public void ReservedWordAdded()
        {
            var header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c", "class" });
            Assert.AreEqual("@RELATION Data\r\n" +
                            "@ATTRIBUTE a NUMERIC\r\n" +
                            "@ATTRIBUTE b NUMERIC\r\n" +
                            "@ATTRIBUTE c NUMERIC\r\n" +
                            "@ATTRIBUTE class_word NUMERIC\r\n" +
                            "@ATTRIBUTE class {Negative, Neutral, Positive}\r\n" +
                            "@DATA", header.ToString());
            Assert.AreEqual(5, header.Header.Total);
        }

        [Test]
        public void Dublicates()
        {
            var header = ArffDataSet.Create<PositivityType>("Test");
            var item = header.AddDocument();
            item.AddRecord("a");
            item.AddRecord("a");
            item.AddRecord("b's");
            item.AddRecord("b's");
            Assert.AreEqual("@RELATION Test\r\n" +
                            "@ATTRIBUTE a NUMERIC\r\n" +
                            "@ATTRIBUTE \"b's\" NUMERIC\r\n" +
                            "@ATTRIBUTE class {Negative, Neutral, Positive}\r\n" +
                            "@DATA", header.ToString());
            Assert.AreEqual(3, header.Header.Total);
        }

        [Test]
        public void CreateSimple()
        {
            var data = ArffDataSet.CreateSimple("Test");
            var item = data.AddDocument();
            item.AddRecord("a");
            item.AddRecord("a");
            Assert.AreEqual(1, data.Header.Total);
        }

        [Test]
        public void TestToString()
        {
            var header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            header.UseTotal = true;
            var item = header.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Negative;
            Assert.AreEqual("{0 1,3 Negative}", item.ToString());
            item.AddRecord("a").Value = 0;
            header.UseTotal = false;
            Assert.AreEqual("{3 Negative}", item.ToString());
            item.AddRecord("a").Value = 2;
            Assert.AreEqual("{0 2,3 Negative}", item.ToString());
        }

        [Test]
        public void Other()
        {
            var header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            var item = header.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Positive;
            Assert.AreEqual(PositivityType.Positive, item.Class.Value);
            item.Class.Value = PositivityType.Negative;
            Assert.AreEqual(PositivityType.Negative, item.Class.Value);
        }

        [Test]
        public void Resolve()
        {
            var header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            var item = header.AddDocument();
            var resolve = item.Resolve(new NumericHeader(0, "a"));
            resolve.Value = 3;
            resolve = item.Resolve(new NumericHeader(0, "a"));
            Assert.AreEqual(3, resolve.Value);
            Assert.AreEqual("{0 3,3 Neutral}", item.ToString());
        }

        [Test]
        public void AddingDynamicaly()
        {
            var docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            var item = docsDataHolder.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Negative;
            Assert.AreEqual("{0 1,3 Negative}", item.ToString());
            docsDataHolder.Header.RegisterNumeric("d");
            var dWord = item.AddRecord("d");
            Assert.AreEqual("{0 1,3 1,4 Negative}", item.ToString());
            item.SetRecord(new DataRecord(dWord.Header) { Total = 4, Value = 10 });
            Assert.AreEqual("{0 1,3 10,4 Negative}", item.ToString());
        }

        [Test]
        public void InvalidDateType()
        {
            var docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            var item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterDate("a");
            var record = item.AddRecord("a");
            Assert.Throws<InvalidDataException>(() => record.Value = "test");
        }

        [Test]
        public void TestDate()
        {
            var docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            var item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterDate("a");
            var record = item.AddRecord("a");
            record.Value = new DateTime(2012, 02, 12);
            Assert.AreEqual("{0 2012-02-12,1 Neutral}", item.ToString());
        }

        [Test]
        public void TestNominal()
        {
            var docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            var item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterNominal("a", new[] { "1", "2" });
            var record = item.AddRecord("a");
            record.Value = "1";
            Assert.AreEqual("{0 1,1 Neutral}", item.ToString());
        }

        [Test]
        public void InvalidStringType()
        {
            var docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            var item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterString("a");
            var record = item.AddRecord("a");
            Assert.Throws<InvalidDataException>(() => record.Value = 1);
        }

        [Test]
        public void InvalidNumericType()
        {
            var docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            var item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterNumeric("a");
            var record = item.AddRecord("a");
            Assert.Throws<InvalidDataException>(() => record.Value = "1");
        }

        [Test]
        public void InvalidNominalType()
        {
            var docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            var item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterNominal("a", new[] { "1", "2" });
            var record = item.AddRecord("a");
            Assert.Throws<InvalidDataException>(() => record.Value = "3");
        }

        [Test]
        public void Normalize()
        {
            var docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            var item = docsDataHolder.AddDocument();
            var first = item;
            item.AddRecord("a").Value = 2;
            item.AddRecord("b").Value = 4;
            item = docsDataHolder.AddDocument();
            item.AddRecord("a").Value = 20;
            item.AddRecord("b").Value = 40;
            item = docsDataHolder.AddDocument();
            item.AddRecord("a").Value = 30;
            item.AddRecord("c").Value = 60;
            item = docsDataHolder.AddDocument();
            item.AddRecord("a").Value = 30;
            item.AddRecord("b").Value = 30;
            item.AddRecord("c").Value = 30;
            item = docsDataHolder.AddDocument();
            item.AddRecord("c").Value = 100;
            docsDataHolder.Normalize(NormalizationType.L2);
            Assert.AreEqual("{0 0.4472135955,1 0.894427191,3 Neutral}", first.ToString());
        }

        [Test]
        public void Class()
        {
            var docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            Assert.AreEqual("class", docsDataHolder.Header.Class.Name);
        }

        [Test]
        public void AddingClass()
        {
            var docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c", "class" });
            var item = docsDataHolder.AddDocument();
            var header = item.AddRecord("class");
            Assert.AreEqual("class_word", header.Header.Name);
        }

        [Test]
        public void SaveLoad()
        {
            var docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            var item = docsDataHolder.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Negative;
            Assert.AreEqual("{0 1,3 Negative}", item.ToString());
            docsDataHolder.Header.RegisterDate("Date");
            docsDataHolder.Header.RegisterNominal("Test", new[] { "Yes", "No" });
            docsDataHolder.Header.RegisterString("Comment");
            item.AddRecord("Date").Value = new DateTime(2012, 02, 12);
            item.AddRecord("Test").Value = "Yes";
            item.AddRecord("Comment").Value = "Added new record";
            Assert.AreEqual(7, docsDataHolder.Header.Total);
            Assert.AreEqual("{0 1,3 2012-02-12,4 Yes,5 Added new record,6 Negative}", item.ToString());
            docsDataHolder.Save("Test.arff");
            var loaded = ArffDataSet.Load<PositivityType>("Test.arff");
            Assert.AreEqual(7, loaded.Header.Total);
            Assert.AreEqual(1, loaded.Documents.Count());
            Assert.AreEqual("{0 1,3 2012-02-12,4 Yes,5 Added new record,6 Negative}", loaded.Documents.First().ToString());

            loaded = ArffDataSet.LoadSimple("Test.arff");
            Assert.AreEqual(7, loaded.Header.Total);
            Assert.AreEqual(1, loaded.Documents.Count());
            Assert.AreEqual("{0 1,3 2012-02-12,4 Yes,5 Added new record,6 Negative}", loaded.Documents.First().ToString());
        }


        [Test]
        public void SaveLoadSimple()
        {
            var docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            var item = docsDataHolder.AddDocument();
            item.AddRecord("c");
            item.Class.Value = PositivityType.Negative;
            docsDataHolder.Save("Test.arff");
            var loaded = ArffDataSet.Load<PositivityType>("Test.arff");
            Assert.AreEqual(4, loaded.Header.Total);
            Assert.AreEqual(1, loaded.Documents.Count());
            Assert.AreEqual("{2 1,3 Negative}", loaded.Documents.First().ToString());
        }

        [Test]
        public void SaveLoadStars()
        {
            var docsDataHolder = ArffDataSet.CreateDataRecord<StarType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            var item = docsDataHolder.AddDocument();
            item.AddRecord("a");
            item.Class.Value = StarType.Three;
            Assert.AreEqual("{0 1,3 Three}", item.ToString());
            Assert.AreEqual(4, docsDataHolder.Header.Total);
            docsDataHolder.Save("Test.arff");
            var loaded = ArffDataSet.Load<StarType>("Test.arff");
            Assert.AreEqual(4, loaded.Header.Total);
            Assert.AreEqual(1, loaded.Documents.Count());
            Assert.AreEqual("{0 1,3 Three}", loaded.Documents.First().ToString());
            Assert.AreEqual(StarType.Three, loaded.Documents.First().Class.Value);
        }

        [Test]
        public void Create()
        {
            var header = ArffDataSet.CreateDataRecord<StarType>(new[] { "1", "2", "3" });
            Assert.AreEqual(4, header.Header.Total);
        }

        [Test]
        public void Createdoc()
        {
            var header = ArffDataSet.CreateDataRecord<StarType>(new[] { "1", "2", "3" });
            Assert.AreEqual(0, header.TotalDocuments);
            header.GetDocument(1);
            Assert.AreEqual(1, header.TotalDocuments);
            header.GetDocument(2);
            Assert.AreEqual(2, header.TotalDocuments);
            header.GetDocument(2);
            Assert.AreEqual(2, header.TotalDocuments);
            Assert.AreEqual(2, header.Documents.Count());
            var document = header.Documents.First();
            document.Class.Value = StarType.Four;
            Assert.AreEqual(StarType.Four, document.Class.Value);
        }

        [Test]
        public void Save()
        {
            var header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "1", "2", "3" });
            header.UseTotal = true;
            var doc = header.GetDocument(1);
            doc.AddRecord("1");
            doc.AddRecord("2");
            doc.Class.Value = PositivityType.Positive;
            doc = header.GetDocument(2);
            doc.AddRecord("2");
            doc.AddRecord("3");
            doc.Class.Value = PositivityType.Negative;
            Assert.AreEqual(2, header.TotalDocuments);
            var docs = header.Documents.ToArray();
            Assert.AreEqual("{0 1,1 1,3 Positive}", docs[0].ToString());
            Assert.AreEqual("{1 1,2 1,3 Negative}", docs[1].ToString());
            Assert.AreEqual(string.Format("@RELATION Data{0}" +
                            "@ATTRIBUTE 1 NUMERIC{0}" +
                            "@ATTRIBUTE 2 NUMERIC{0}" +
                            "@ATTRIBUTE 3 NUMERIC{0}" +
                            "@ATTRIBUTE class {{Negative, Neutral, Positive}}{0}" +
                            "@DATA", "\r\n"), header.ToString());
        }
    }
}

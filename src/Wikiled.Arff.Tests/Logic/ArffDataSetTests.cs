using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Wikiled.Arff.Logic;
using Wikiled.Arff.Logic.Headers;

namespace Wikiled.Arff.Tests.Logic
{
    [TestFixture]
    public class ArffDataHolderTests
    {
        private string fileName;

        [SetUp]
        public void Setup()
        {
            fileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "Test.arff");
        }

        [Test]
        public void ReservedWordAdded()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c", "class" });
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
            IArffDataSet header = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = header.AddDocument();
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
            IArffDataSet data = ArffDataSet.CreateSimple("Test");
            IArffDataRow item = data.AddDocument();
            item.AddRecord("a");
            item.AddRecord("a");
            Assert.AreEqual(1, data.Header.Total);
        }

        [Test]
        public void TestToString()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            header.UseTotal = true;
            IArffDataRow item = header.AddDocument();
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
            IArffDataSet header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            IArffDataRow item = header.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Positive;
            Assert.AreEqual(PositivityType.Positive, item.Class.Value);
            item.Class.Value = PositivityType.Negative;
            Assert.AreEqual(PositivityType.Negative, item.Class.Value);
        }

        [Test]
        public void Resolve()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            IArffDataRow item = header.AddDocument();
            DataRecord resolve = item.AddRecord(new NumericHeader(0, "a"));
            resolve.Value = 3;
            resolve = item.AddRecord(new NumericHeader(0, "a"));
            Assert.AreEqual(3, resolve.Value);
            Assert.AreEqual("{0 3,3 Neutral}", item.ToString());
        }

        [Test]
        public void AddingDynamicaly()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            IArffDataRow item = docsDataHolder.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Negative;
            Assert.AreEqual("{0 1,3 Negative}", item.ToString());
            docsDataHolder.Header.RegisterNumeric("d");
            DataRecord dWord = item.AddRecord("d");
            Assert.AreEqual("{0 1,3 1,4 Negative}", item.ToString());
            item.SetRecord(new DataRecord(dWord.Header) { Total = 4, Value = 10 });
            Assert.AreEqual("{0 1,3 10,4 Negative}", item.ToString());
        }

        [Test]
        public void InvalidDateType()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterDate("a");
            DataRecord record = item.AddRecord("a");
            Assert.Throws<InvalidDataException>(() => record.Value = "test");
        }

        [Test]
        public void TestDate()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterDate("a");
            DataRecord record = item.AddRecord("a");
            record.Value = new DateTime(2012, 02, 12);
            Assert.AreEqual("{0 2012-02-12,1 Neutral}", item.ToString());
        }

        [Test]
        public void TestNominal()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterNominal("a", new[] { "1", "2" });
            DataRecord record = item.AddRecord("a");
            record.Value = "1";
            Assert.AreEqual("{0 1,1 Neutral}", item.ToString());
        }

        [Test]
        public void InvalidStringType()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterString("a");
            DataRecord record = item.AddRecord("a");
            Assert.Throws<InvalidDataException>(() => record.Value = 1);
        }

        [Test]
        public void InvalidNumericType()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterNumeric("a");
            DataRecord record = item.AddRecord("a");
            Assert.Throws<InvalidDataException>(() => record.Value = "1");
        }

        [Test]
        public void InvalidNominalType()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterNominal("a", new[] { "1", "2" });
            DataRecord record = item.AddRecord("a");
            Assert.Throws<InvalidDataException>(() => record.Value = "3");
        }

        [Test]
        public void Class()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            Assert.AreEqual("class", docsDataHolder.Header.Class.Name);
        }

        [Test]
        public void AddingClass()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c", "class" });
            IArffDataRow item = docsDataHolder.AddDocument();
            DataRecord header = item.AddRecord("class");
            Assert.AreEqual("class_word", header.Header.Name);
        }

        [Test]
        public void SaveLoad()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            IArffDataRow item = docsDataHolder.AddDocument();
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
            Assert.AreEqual("{0 2012-02-12,1 1,4 Yes,5 Added new record,6 Negative}", item.ToString());
            docsDataHolder.Save(fileName);
            IArffDataSet loaded = ArffDataSet.Load<PositivityType>(fileName);
            Assert.AreEqual(7, loaded.Header.Total);
            Assert.AreEqual(1, loaded.Documents.Count());
            Assert.AreEqual("{0 2012-02-12,1 1,4 Yes,5 Added new record,6 Negative}", loaded.Documents.First().ToString());

            loaded = ArffDataSet.LoadSimple(fileName);
            Assert.AreEqual(7, loaded.Header.Total);
            Assert.AreEqual(1, loaded.Documents.Count());
            Assert.AreEqual("{0 2012-02-12,1 1,4 Yes,5 Added new record,6 Negative}", loaded.Documents.First().ToString());
        }


        [Test]
        public void SaveLoadSimple()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            IArffDataRow item = docsDataHolder.AddDocument();
            item.AddRecord("c");
            item.Class.Value = PositivityType.Negative;
            docsDataHolder.Save(fileName);
            IArffDataSet loaded = ArffDataSet.Load<PositivityType>(fileName);
            Assert.AreEqual(4, loaded.Header.Total);
            Assert.AreEqual(1, loaded.Documents.Count());
            Assert.AreEqual("{2 1,3 Negative}", loaded.Documents.First().ToString());
        }

        [Test]
        public void SaveLoadStars()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<StarType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            IArffDataRow item = docsDataHolder.AddDocument();
            item.AddRecord("a");
            item.Class.Value = StarType.Three;
            Assert.AreEqual("{0 1,3 Three}", item.ToString());
            Assert.AreEqual(4, docsDataHolder.Header.Total);
            docsDataHolder.Save(fileName);
            IArffDataSet loaded = ArffDataSet.Load<StarType>(fileName);
            Assert.AreEqual(4, loaded.Header.Total);
            Assert.AreEqual(1, loaded.Documents.Count());
            Assert.AreEqual("{0 1,3 Three}", loaded.Documents.First().ToString());
            Assert.AreEqual(StarType.Three, loaded.Documents.First().Class.Value);
            docsDataHolder.SaveCsv(Path.Combine(TestContext.CurrentContext.TestDirectory, "Test.csv"));
        }

        [Test]
        public void Create()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<StarType>(new[] { "1", "2", "3" });
            Assert.AreEqual(4, header.Header.Total);
        }

        [Test]
        public void Createdoc()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<StarType>(new[] { "1", "2", "3" });
            Assert.AreEqual(0, header.TotalDocuments);
            header.GetOrCreateDocument("1");
            Assert.AreEqual(1, header.TotalDocuments);
            header.GetOrCreateDocument("2");
            Assert.AreEqual(2, header.TotalDocuments);
            header.GetOrCreateDocument("2");
            Assert.AreEqual(2, header.TotalDocuments);
            Assert.AreEqual(2, header.Documents.Count());
            IArffDataRow document = header.Documents.First();
            document.Class.Value = StarType.Four;
            Assert.AreEqual(StarType.Four, document.Class.Value);
        }

        [Test]
        public void Save()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "1", "2", "3" });
            header.UseTotal = true;
            IArffDataRow doc = header.GetOrCreateDocument("1");
            doc.AddRecord("1");
            doc.AddRecord("2");
            doc.Class.Value = PositivityType.Positive;
            doc = header.GetOrCreateDocument("2");
            doc.AddRecord("2");
            doc.AddRecord("3");
            doc.Class.Value = PositivityType.Negative;
            Assert.AreEqual(2, header.TotalDocuments);
            IArffDataRow[] docs = header.Documents.ToArray();
            Assert.AreEqual("{0 1,1 1,3 Positive}", docs[0].ToString());
            Assert.AreEqual("{1 1,2 1,3 Negative}", docs[1].ToString());
            Assert.AreEqual(string.Format("@RELATION Data{0}" +
                            "@ATTRIBUTE 1 NUMERIC{0}" +
                            "@ATTRIBUTE 2 NUMERIC{0}" +
                            "@ATTRIBUTE 3 NUMERIC{0}" +
                            "@ATTRIBUTE class {{Negative, Neutral, Positive}}{0}" +
                            "@DATA", "\r\n"), header.ToString());
        }

        [Test]
        public void SaveWithId()
        {
            IArffDataSet header = ArffDataSet.Create<PositivityType>("Data");
            header.UseTotal = true;
            header.HasId = true;
            IArffDataRow doc = header.GetOrCreateDocument("1");
            doc.AddRecord("1");
            doc.Class.Value = PositivityType.Positive;
            doc = header.GetOrCreateDocument("2");
            doc.AddRecord("3");
            doc.Class.Value = PositivityType.Negative;
            Assert.AreEqual(2, header.TotalDocuments);
            IArffDataRow[] docs = header.Documents.ToArray();
            Assert.AreEqual("{0 1,1 1,3 Positive}", docs[0].ToString());
            Assert.AreEqual("{0 2,2 1,3 Negative}", docs[1].ToString());
            Assert.AreEqual(string.Format("@RELATION Data{0}" +
                                          "@ATTRIBUTE DOCUMENT_ID_FIELD STRING{0}" +
                                          "@ATTRIBUTE 1 NUMERIC{0}" +
                                          "@ATTRIBUTE 3 NUMERIC{0}" +
                                          "@ATTRIBUTE class {{Negative, Neutral, Positive}}{0}" +
                                          "@DATA", "\r\n"), header.ToString());
        }
    }
}

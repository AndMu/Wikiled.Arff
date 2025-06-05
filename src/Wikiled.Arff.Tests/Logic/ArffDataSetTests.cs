using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using NUnit.Framework.Legacy;
using Wikiled.Arff.Extensions;
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
            ClassicAssert.AreEqual("@RELATION Data\r\n" +
                            "@ATTRIBUTE a NUMERIC\r\n" +
                            "@ATTRIBUTE b NUMERIC\r\n" +
                            "@ATTRIBUTE c NUMERIC\r\n" +
                            "@ATTRIBUTE class_word NUMERIC\r\n" +
                            "@ATTRIBUTE CLASS {Negative, Neutral, Positive}\r\n" +
                            "@DATA", header.ToString());
            ClassicAssert.AreEqual(5, header.Header.Total);
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
            ClassicAssert.AreEqual("@RELATION Test\r\n" +
                            "@ATTRIBUTE a NUMERIC\r\n" +
                            "@ATTRIBUTE \"b's\" NUMERIC\r\n" +
                            "@ATTRIBUTE CLASS {Negative, Neutral, Positive}\r\n" +
                            "@DATA", header.ToString());
            ClassicAssert.AreEqual(3, header.Header.Total);
        }

        [Test]
        public void CreateSimple()
        {
            IArffDataSet data = ArffDataSet.CreateSimple("Test");
            IArffDataRow item = data.AddDocument();
            item.AddRecord("a");
            item.AddRecord("a");
            ClassicAssert.AreEqual(1, data.Header.Total);
        }

        [Test]
        public void TestToString()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            header.UseTotal = true;
            IArffDataRow item = header.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Negative;
            ClassicAssert.AreEqual("{0 1,3 Negative}", item.ToString());
            item.AddRecord("a").Value = 0;
            header.UseTotal = false;
            ClassicAssert.AreEqual("{3 Negative}", item.ToString());
            item.AddRecord("a").Value = 2;
            ClassicAssert.AreEqual("{0 2,3 Negative}", item.ToString());
        }

        [Test]
        public void Other()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            IArffDataRow item = header.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Positive;
            ClassicAssert.AreEqual(PositivityType.Positive, item.Class.Value);
            item.Class.Value = PositivityType.Negative;
            ClassicAssert.AreEqual(PositivityType.Negative, item.Class.Value);
        }

        [Test]
        public void Resolve()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            IArffDataRow item = header.AddDocument();
            DataRecord resolve = item.AddRecord(new NumericHeader(0, "a"));
            resolve.Value = 3;
            resolve = item.AddRecord(new NumericHeader(0, "a"));
            ClassicAssert.AreEqual(3, resolve.Value);
            ClassicAssert.AreEqual("{0 3,3 Neutral}", item.ToString());
        }

        [Test]
        public void AddingDynamicaly()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            IArffDataRow item = docsDataHolder.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Negative;
            ClassicAssert.AreEqual("{0 1,3 Negative}", item.ToString());
            docsDataHolder.Header.RegisterNumeric("d");
            DataRecord dWord = item.AddRecord("d");
            ClassicAssert.AreEqual("{0 1,3 1,4 Negative}", item.ToString());
            item.SetRecord(new DataRecord(dWord.Header) { Total = 4, Value = 10 });
            ClassicAssert.AreEqual("{0 1,3 10,4 Negative}", item.ToString());
        }

        [Test]
        public void InvalidDateType()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterDate("a");
            DataRecord record = item.AddRecord("a");
            ClassicAssert.Throws<InvalidDataException>(() => record.Value = "test");
        }

        [Test]
        public void TestDate()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterDate("a");
            DataRecord record = item.AddRecord("a");
            record.Value = new DateTime(2012, 02, 12);
            ClassicAssert.AreEqual("{0 2012-02-12,1 Neutral}", item.ToString());
        }

        [Test]
        public void TestNominal()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterNominal("a", new[] { "1", "2" });
            DataRecord record = item.AddRecord("a");
            record.Value = "1";
            ClassicAssert.AreEqual("{0 1,1 Neutral}", item.ToString());
        }

        [Test]
        public void InvalidStringType()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterString("a");
            DataRecord record = item.AddRecord("a");
            ClassicAssert.Throws<InvalidDataException>(() => record.Value = 1);
        }

        [Test]
        public void InvalidNumericType()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterNumeric("a");
            DataRecord record = item.AddRecord("a");
            ClassicAssert.Throws<InvalidDataException>(() => record.Value = "1");
        }

        [Test]
        public void InvalidNominalType()
        {
            IArffDataSet docsDataHolder = ArffDataSet.Create<PositivityType>("Test");
            IArffDataRow item = docsDataHolder.AddDocument();
            docsDataHolder.Header.RegisterNominal("a", new[] { "1", "2" });
            DataRecord record = item.AddRecord("a");
            ClassicAssert.Throws<InvalidDataException>(() => record.Value = "3");
        }

        [Test]
        public void Class()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            ClassicAssert.AreEqual("CLASS", docsDataHolder.Header.Class.Name);
        }

        [Test]
        public void AddingClass()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c", "class" });
            IArffDataRow item = docsDataHolder.AddDocument();
            DataRecord header = item.AddRecord("CLASS");
            ClassicAssert.AreEqual("class_word", header.Header.Name);
        }

        [Test]
        public void SaveLoad()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<PositivityType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            IArffDataRow item = docsDataHolder.AddDocument();
            item.AddRecord("a");
            item.Class.Value = PositivityType.Negative;
            ClassicAssert.AreEqual("{0 1,3 Negative}", item.ToString());
            docsDataHolder.Header.RegisterDate("Date");
            docsDataHolder.Header.RegisterNominal("Test", new[] { "Yes", "No" });
            docsDataHolder.Header.RegisterString("Comment");
            item.AddRecord("Date").Value = new DateTime(2012, 02, 12);
            item.AddRecord("Test").Value = "Yes";
            item.AddRecord("Comment").Value = "Added new record";
            ClassicAssert.AreEqual(7, docsDataHolder.Header.Total);
            ClassicAssert.AreEqual("{0 2012-02-12,1 1,4 Yes,5 Added new record,6 Negative}", item.ToString());
            docsDataHolder.Save(fileName);
            IArffDataSet loaded = ArffDataSet.Load<PositivityType>(fileName);
            ClassicAssert.AreEqual(7, loaded.Header.Total);
            ClassicAssert.AreEqual(1, loaded.Documents.Count());
            ClassicAssert.AreEqual("{0 2012-02-12,1 1,4 Yes,5 Added new record,6 Negative}", loaded.Documents.First().ToString());

            loaded = ArffDataSet.LoadSimple(fileName);
            ClassicAssert.AreEqual(7, loaded.Header.Total);
            ClassicAssert.AreEqual(1, loaded.Documents.Count());
            ClassicAssert.AreEqual("{0 2012-02-12,1 1,4 Yes,5 Added new record,6 Negative}", loaded.Documents.First().ToString());
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
            ClassicAssert.AreEqual(4, loaded.Header.Total);
            ClassicAssert.AreEqual(1, loaded.Documents.Count());
            ClassicAssert.AreEqual("{2 1,3 Negative}", loaded.Documents.First().ToString());
        }

        [Test]
        public void SaveLoadStars()
        {
            IArffDataSet docsDataHolder = ArffDataSet.CreateDataRecord<StarType>(new[] { "a", "b", "c" });
            docsDataHolder.UseTotal = true;
            IArffDataRow item = docsDataHolder.AddDocument();
            item.AddRecord("a");
            item.Class.Value = StarType.Three;
            ClassicAssert.AreEqual("{0 1,3 Three}", item.ToString());
            ClassicAssert.AreEqual(4, docsDataHolder.Header.Total);
            docsDataHolder.Save(fileName);
            IArffDataSet loaded = ArffDataSet.Load<StarType>(fileName);
            ClassicAssert.AreEqual(4, loaded.Header.Total);
            ClassicAssert.AreEqual(1, loaded.Documents.Count());
            ClassicAssert.AreEqual("{0 1,3 Three}", loaded.Documents.First().ToString());
            ClassicAssert.AreEqual(StarType.Three, loaded.Documents.First().Class.Value);
            docsDataHolder.SaveCsv(Path.Combine(TestContext.CurrentContext.TestDirectory, "Test.csv"));
        }

        [Test]
        public void Create()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<StarType>(new[] { "1", "2", "3" });
            ClassicAssert.AreEqual(4, header.Header.Total);
        }

        [Test]
        public void Createdoc()
        {
            IArffDataSet header = ArffDataSet.CreateDataRecord<StarType>(new[] { "1", "2", "3" });
            ClassicAssert.AreEqual(0, header.TotalDocuments);
            header.GetOrCreateDocument("1");
            ClassicAssert.AreEqual(1, header.TotalDocuments);
            header.GetOrCreateDocument("2");
            ClassicAssert.AreEqual(2, header.TotalDocuments);
            header.GetOrCreateDocument("2");
            ClassicAssert.AreEqual(2, header.TotalDocuments);
            ClassicAssert.AreEqual(2, header.Documents.Count());
            IArffDataRow document = header.Documents.First();
            document.Class.Value = StarType.Four;
            ClassicAssert.AreEqual(StarType.Four, document.Class.Value);
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
            ClassicAssert.AreEqual(2, header.TotalDocuments);
            IArffDataRow[] docs = header.Documents.ToArray();
            ClassicAssert.AreEqual("{0 1,1 1,3 Positive}", docs[0].ToString());
            ClassicAssert.AreEqual("{1 1,2 1,3 Negative}", docs[1].ToString());
            ClassicAssert.AreEqual(string.Format("@RELATION Data{0}" +
                            "@ATTRIBUTE 1 NUMERIC{0}" +
                            "@ATTRIBUTE 2 NUMERIC{0}" +
                            "@ATTRIBUTE 3 NUMERIC{0}" +
                            "@ATTRIBUTE CLASS {{Negative, Neutral, Positive}}{0}" +
                            "@DATA", "\r\n"), header.ToString());
        }

        [Test]
        public void SaveWithId()
        {
            IArffDataSet header = ArffDataSet.Create<PositivityType>("Data");
            header.UseTotal = true;
            header.HasId = true;
            header.HasDate = true;
            IArffDataRow doc = header.GetOrCreateDocument("1");
            ClassicAssert.IsNull(doc.Date);
            doc.Date = new DateTime(2012, 02, 02);
            ClassicAssert.AreEqual(new DateTime(2012, 02, 02), doc.Date);
            doc.AddRecord("1");
            doc.Class.Value = PositivityType.Positive;
            doc = header.GetOrCreateDocument("2");
            doc.AddRecord("3");
            doc.Class.Value = PositivityType.Negative;
            ClassicAssert.AreEqual(2, header.TotalDocuments);
            IArffDataRow[] docs = header.Documents.ToArray();
            ClassicAssert.AreEqual("{0 2012-02-02,1 1,2 1,4 Positive}", docs[0].ToString());
            ClassicAssert.AreEqual("{1 2,3 1,4 Negative}", docs[1].ToString());
            var sorterd = header.Sort();
            ClassicAssert.AreEqual(string.Format("@RELATION Data{0}" +
                                          "@ATTRIBUTE DATE DATE yyyy-MM-dd{0}" +
                                          "@ATTRIBUTE ID STRING{0}" +
                                          "@ATTRIBUTE 1 NUMERIC{0}" +
                                          "@ATTRIBUTE 3 NUMERIC{0}" +
                                          "@ATTRIBUTE CLASS {{Negative, Neutral, Positive}}{0}" +
                                          "@DATA", "\r\n"), sorterd.ToString());

            var file = Path.Combine(TestContext.CurrentContext.TestDirectory, "Test.arff");
            sorterd.Save(file);
            var data = ArffDataSet.Load<PositivityType>(file);
            data.SaveCsv("Data.csv");
        }
    }
}

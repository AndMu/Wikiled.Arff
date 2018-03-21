using System.IO;
using System.Linq;
using NUnit.Framework;
using Wikiled.Arff.Persistence;

namespace Wikiled.Arff.Tests.Persistence
{
    [TestFixture]
    public class ArffDataSetExtensionTests
    {
        [Test]
        public void GetData()
        {
            var dataSet = ArffDataSet.LoadSimple(Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", @"problem.arff"));
            var data = dataSet.GetData().ToArray();
            Assert.AreEqual(9371, data.Length);
            Assert.AreEqual(8, data[0].X.Length);
            Assert.AreEqual(0, data[0].Y);
        }

        [Test]
        public void GetData2()
        {
            var dataSet = ArffDataSet.LoadSimple(Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", @"Data.arff"));
            var data = dataSet.GetData().ToArray();
            Assert.AreEqual(7215, data.Length);
            Assert.AreEqual(456, data[0].X.Length);
            Assert.AreEqual(2, data[0].Y);
        }

        [Test]
        public void GetDataNoClass()
        {
            var dataset = ArffDataSet.CreateSimple("Test");
            var doc = dataset.AddDocument();
            doc.AddRecord("Test1");
            doc.AddRecord("Test2");
            var data = dataset.GetData().ToArray();
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual(2, data[0].X.Length);
            Assert.IsNull(data[0].Y);
        }

        [Test]
        public void GetSimpleData()
        {
            var dataset = ArffDataSet.CreateSimple("Test");
            dataset.Header.RegisterDate("Date");
            dataset.Header.RegisterNumericClass();
            var doc = dataset.AddDocument();
            doc.AddRecord("Test1");
            doc.AddRecord("Test2");
            doc.Class.Value = 1;
            var data = dataset.GetData().ToArray();
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual(2, data[0].X.Length);
            Assert.AreEqual(1, data[0].Y);
        }
    }
}
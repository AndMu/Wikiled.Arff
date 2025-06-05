using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Wikiled.Arff.Extensions;
using Wikiled.Arff.Logic;

namespace Wikiled.Arff.Tests.Extensions
{
    [TestFixture]
    public class ArffExtensionsTests
    {
        [Test]
        public void RemoveClass()
        {
            var dataSet = ArffDataSet.CreateSimple("Test");
            dataSet.Header.RegisterNominalClass("One", "Two", "Three");
            var review = dataSet.AddDocument();
            review.Class.Value = "One";
            review = dataSet.AddDocument();
            review.Class.Value = "Two";
            review = dataSet.AddDocument();
            review.Class.Value = "Three";
            dataSet.RemoveClass("Two");
            ClassicAssert.AreEqual(2, dataSet.Documents.Count());
            ClassicAssert.AreEqual("@RELATION Test\r\n@ATTRIBUTE CLASS {One, Three}\r\n@DATA", dataSet.ToString());
        }

        [Test]
        public async Task CompactHeader()
        {
            var dataSet = ArffDataSet.CreateSimple("Test");
            dataSet.Header.RegisterNumericClass();
            dataSet.Header.CreateHeader = true;
            for (int i = 0; i < 10; i++)
            {
                var review = dataSet.AddDocument();
                review.AddRecord("One");
            }

            var review2 = dataSet.AddDocument();
            review2.AddRecord("Two");
            ClassicAssert.AreEqual(3, dataSet.Header.Total);
            ClassicAssert.AreEqual(11, dataSet.Documents.Count());
            await dataSet.CompactHeader(5).ConfigureAwait(false);
            ClassicAssert.AreEqual(2, dataSet.Header.Total);
            ClassicAssert.AreEqual(10, dataSet.Documents.Count());
            ClassicAssert.AreEqual(10, dataSet.Header["One"].InDocuments);
        }

        [Test]
        public void IsSparse()
        {
            var dataSet = ArffDataSet.CreateSimple("Test");
            var header = dataSet.Header.RegisterNumericClass();
            ClassicAssert.IsTrue(dataSet.IsSparse);
            ClassicAssert.IsTrue(header.IsSparse);
            dataSet.IsSparse = false;
            ClassicAssert.IsFalse(dataSet.IsSparse);
            ClassicAssert.IsFalse(header.IsSparse);
        }

        [Test]
        public void CompactReviews()
        {
            var dataSet = ArffDataSet.CreateSimple("Test");
            dataSet.UseTotal = true;
            dataSet.Header.RegisterNumericClass();
            dataSet.Header.CreateHeader = true;
            for (int i = 0; i < 10; i++)
            {
                var review = dataSet.AddDocument();
                review.AddRecord("One");
            }

            var review2 = dataSet.AddDocument();
            review2.AddRecord("Two");
            review2.AddRecord("Three");
            review2.AddRecord("Three2");
            ClassicAssert.AreEqual(5, dataSet.Header.Total);
            ClassicAssert.AreEqual(11, dataSet.Documents.Count());
            dataSet.CompactReviews(2);
            ClassicAssert.AreEqual(5, dataSet.Header.Total);
            ClassicAssert.AreEqual(1, dataSet.Documents.Count());
            ClassicAssert.AreEqual("{1 1,2 1,3 1}", dataSet.Documents.First().ToString());
            ClassicAssert.AreEqual(1, dataSet.Header["Two"].InDocuments);
            ClassicAssert.AreEqual(0, dataSet.Header["One"].InDocuments);
        }

        [Test]
        public void CompactClass()
        {
            var dataSet = ArffDataSet.CreateSimple("Test");
            dataSet.Header.RegisterNominalClass("One", "Two", "Three");
            var review = dataSet.AddDocument();
            review.Class.Value = "One";
            review = dataSet.AddDocument();
            review.Class.Value = "Two";
            review = dataSet.AddDocument();
            review.Class.Value = "Two";
            dataSet.CompactClass(1);
            ClassicAssert.AreEqual(2, dataSet.Documents.Count());
        }

        [Test]
        public void CopyDataSet()
        {
            var dataSet = ArffDataSet.LoadSimple(Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", @"problem.arff"));
            var copy = dataSet.CopyDataSet(dataSet.Header, "Test");

            var orgininalDocs = dataSet.Documents.ToArray();
            var resultDocs = copy.Documents.ToArray();
            for (int i = 0; i < dataSet.TotalDocuments; i++)
            {
                ClassicAssert.AreEqual(resultDocs[i].Class.Value, orgininalDocs[i].Class.Value);
            }
        }
    }
}

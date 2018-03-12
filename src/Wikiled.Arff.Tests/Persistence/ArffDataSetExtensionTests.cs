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
        }

        [Test]
        public void GetData2()
        {
            var dataSet = ArffDataSet.LoadSimple(Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", @"Data.arff"));
            var data = dataSet.GetData().ToArray();
            Assert.AreEqual(7215, data.Length);
        }
    }
}
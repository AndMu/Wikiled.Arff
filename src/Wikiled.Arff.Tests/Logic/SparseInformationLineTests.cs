using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Wikiled.Arff.Logic;

namespace Wikiled.Arff.Tests.Logic
{
    [TestFixture]
    public class SparseInformationLineTests
    {
        [Test]
        public void Add()
        {
            SparseInformationLine line = new SparseInformationLine();
            line.Add(null);
            line.Add("1");
            line.Add("1");
            ClassicAssert.AreEqual(3, line.Index);
            ClassicAssert.AreEqual("{1 1,2 1}", line.GenerateLine());
        }

        [Test]
        public void MoveIndex()
        {
            SparseInformationLine line = new SparseInformationLine();
            line.Add(null);
            ClassicAssert.AreEqual(1, line.Index);
            line.MoveIndex(10);
            ClassicAssert.AreEqual(10, line.Index);
            line.MoveIndex(1);
            ClassicAssert.AreEqual(10, line.Index);
        }

        [Test]
        public void AddIndex()
        {
            SparseInformationLine line = new SparseInformationLine();
            line.Add(3, "1");
            ClassicAssert.AreEqual(3, line.Index);
            ClassicAssert.AreEqual("{3 1}", line.GenerateLine());
        }

        [Test]
        public void AddIndexOutOfRance()
        {
            SparseInformationLine line = new SparseInformationLine();
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => line.Add(-1, "1"));
        }

        [Test]
        public void AddIndexExisting()
        {
            SparseInformationLine line = new SparseInformationLine();
            line.Add(3, "1");
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => line.Add(3, "1"));
        }
    }
}

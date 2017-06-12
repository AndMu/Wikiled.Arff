using System;
using System.Linq;
using Wikiled.Arff.Persistence;
using Wikiled.Arff.Persistence.Headers;
using Wikiled.Core.Utility.Arguments;

namespace Wikiled.Arff.Extensions
{
    public static class ArffExtensions
    {
        public static void CompactClass(this IArffDataSet dataSet, int minimum)
        {
            Guard.NotNull(() => dataSet, dataSet);
            var classItem = dataSet.Header.Class as NominalHeader;
            if (classItem == null)
            {
                throw new InvalidOperationException("Class not supported");
            }

            foreach (var nominal in classItem.Nominals)
            {
                var count = dataSet.Documents.Count(item => item.Class.Header.ReadValue(item.Class) == nominal);
                if (count <= minimum)
                {
                    dataSet.RemoveClass(nominal);
                }
            }
        }

        public static void RemoveClass(this IArffDataSet dataSet, string name)
        {
            Guard.NotNull(() => dataSet, dataSet);
            Guard.NotNullOrEmpty(() => name, name);
            var classItem = dataSet.Header.Class as NominalHeader;
            if (classItem == null)
            {
                throw new InvalidOperationException("Class not supported");
            }

            var reviews = dataSet.Documents.ToArray();
            foreach (var review in reviews)
            {
                if (review.Class.Header.ReadValue(review.Class) == name)
                {
                    dataSet.RemoveDocument(review.Id);
                }
            }

            classItem.Nominals = classItem.Nominals.Where(item => item != name).ToArray();
        }

        public static void CompactHeader(this IArffDataSet dataSet, int minOccurences)
        {
            Guard.NotNull(() => dataSet, dataSet);
            foreach (var header in dataSet.Header.ToArray())
            {
                if (header == dataSet.Header.Class)
                {
                    continue;
                }

                if (header.InDocuments <= minOccurences)
                {
                    dataSet.Header.Remove(header);
                }
            }

            dataSet.CompactReviews(0);
        }

        public static void CompactReviews(this IArffDataSet dataSet, int minReviewSize)
        {
            Guard.NotNull(() => dataSet, dataSet);
            foreach (var review in dataSet.Documents.ToArray())
            {
                if (review.Count <= minReviewSize)
                {
                    dataSet.RemoveDocument(review.Id);
                }
            }
        }
    }
}

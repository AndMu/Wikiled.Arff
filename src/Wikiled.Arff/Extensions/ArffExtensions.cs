using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Wikiled.Arff.Persistence;
using Wikiled.Arff.Persistence.Headers;
using Wikiled.Common.Arguments;

namespace Wikiled.Arff.Extensions
{
    public static class ArffExtensions
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static void CompactClass(this IArffDataSet dataSet, int minimum)
        {
            Guard.NotNull(() => dataSet, dataSet);
            log.Debug("CompactClass: {0}", minimum);
            if (!(dataSet.Header.Class is NominalHeader classItem))
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
            log.Debug("RemoveClass: {0}", name);
            Guard.NotNullOrEmpty(() => name, name);
            if (!(dataSet.Header.Class is NominalHeader classItem))
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

        public static async Task CompactHeader(this IArffDataSet dataSet, int minOccurences)
        {
            Guard.NotNull(() => dataSet, dataSet);
            log.Debug("CompactHeader: {0}", minOccurences);
            List<Task> tasks = new List<Task>();
            foreach (var header in dataSet.Header.ToArray())
            {
                if (header == dataSet.Header.Class)
                {
                    continue;
                }

                if (header.InDocuments <= minOccurences)
                {
                    tasks.Add(Task.Run(() => dataSet.Header.Remove(header)));
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            dataSet.CompactReviews(0);
        }

        public static void CompactReviews(this IArffDataSet dataSet, int minReviewSize)
        {
            Guard.NotNull(() => dataSet, dataSet);
            log.Debug("CompactReviews: {0}", minReviewSize);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Arff.Logic;
using Wikiled.Arff.Logic.Headers;
using Wikiled.Common.Logging;

namespace Wikiled.Arff.Extensions
{
    public static class ArffExtensions
    {
        private static readonly ILogger log = ApplicationLogging.CreateLogger("ArffExtensions");

        public static IArffDataSet CreateDataSet(this IArffDataSet baseDataSet, string name)
        {
            if (baseDataSet == null)
            {
                throw new ArgumentNullException(nameof(baseDataSet));
            }

            var result = ArffDataSet.CreateFixed((IHeadersWordsHandling)baseDataSet.Header.Clone(), name);
            return result;
        }

        public static IArffDataSet Sort(this IArffDataSet arff)
        {
            return arff.CopyDataSet(arff.Header.CopyHeader(true), arff.Name);
        }
        
        public static IArffDataSet CopyDataSet(this IArffDataSet dataSetSource, IHeadersWordsHandling headers, string name)
        {
            if (dataSetSource == null)
            {
                throw new ArgumentNullException(nameof(dataSetSource));
            }

            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            var dataSet = ArffDataSet.CreateFixed((IHeadersWordsHandling)headers.Clone(), name);
            dataSet.HasId = dataSetSource.HasId;
            dataSet.HasDate = dataSet.HasDate;
            foreach (var review in dataSetSource.Documents.OrderBy(item => item.Id))
            {
                var newReview = dataSet.GetOrCreateDocument(review.Id);
                foreach (var word in review.GetRecords())
                {
                    var addedWord = newReview.AddRecord(word.Header);
                    if (addedWord == null)
                    {
                        continue;
                    }

                    addedWord.Value = word.Value;
                }

                newReview.Class.Value = review.Class.Value;
            }

            return dataSet;
        }

        public static void CompactClass(this IArffDataSet dataSet, int minimum)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            log.LogDebug("CompactClass: {0}", minimum);
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
            log.LogDebug("RemoveClass: {0}", name);
            if (!(dataSet.Header.Class is NominalHeader classItem))
            {
                throw new InvalidOperationException("Class not supported");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
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
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            log.LogDebug("CompactHeader: {0}", minOccurences);
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
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            log.LogDebug("CompactReviews: {0}", minReviewSize);
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

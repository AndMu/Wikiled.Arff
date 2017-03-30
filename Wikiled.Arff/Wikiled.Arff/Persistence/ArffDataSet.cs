using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using Wikiled.Arff.Normalization;
using Wikiled.Arff.Persistence.Headers;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Core.Utility.Extensions;

namespace Wikiled.Arff.Persistence
{
    public class ArffDataSet : IArffDataSet
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<int, IArffDataRow> reviews = new ConcurrentDictionary<int, IArffDataRow>();

        private int internalReviewsOffset = 100000;

        private Func<IEnumerable<IArffDataRow>, IEnumerable<IArffDataRow>> sort;

        private bool useTotal;

        private readonly string name;

        private ArffDataSet(IHeadersWordsHandling header, string name)
        {
            Guard.NotNull(() => header, header);
            Normalization = NormalizationType.None;
            Header = header;
            Header.Removed += HeaderWordsRemoved;
            Header.Added += HeaderWordsOnAdded;
            this.name = string.IsNullOrEmpty(name) ? "DATA" : name;
        }

        public Func<IEnumerable<IArffDataRow>, IEnumerable<IArffDataRow>> Sort
        {
            get
            {
                if (sort == null)
                {
                    return item => item;
                }

                return sort;
            }
            set { sort = value; }
        }

        public IArffDataRow[] Reviews => (from item in reviews select item.Value).ToArray();

        public IHeadersWordsHandling Header { get; }

        public NormalizationType Normalization { get; private set; }

        public int TotalReviews => reviews.Count;

        public bool UseTotal
        {
            get => useTotal;
            set
            {
                foreach (NumericHeader header in Header.Where(item => item is NumericHeader))
                {
                    header.UseCount = value;
                }

                useTotal = value;
            }
        }

        public void Clear()
        {
            Normalization = NormalizationType.None;
            reviews.Clear();
        }

        public void Normalize(NormalizationType type)
        {
            if (Normalization != NormalizationType.None)
            {
                throw new ArgumentOutOfRangeException("type", "Data is already normalized");
            }

            if (type == NormalizationType.None)
            {
                return;
            }

            Normalization = type;
            foreach (var review in Reviews)
            {
                var words = review.GetRecords()
                    .Where(item => item.Header is NumericHeader)
                    .ToArray();

                var normalized = words
                    .Select(item => Convert.ToDouble(item.Value))
                    .Normalize(type)
                    .GetNormalized
                    .ToArray();

                for (var i = 0; i < words.Length; i++)
                {
                    words[i].Value = normalized[i];
                }
            }
        }

        public void RemoveReview(int reviewId)
        {
            var review = GetReview(reviewId);
            if (review != null)
            {
                foreach (var headers in review.Headers)
                {
                    headers.Remove(review.ReviewId);
                }

                reviews.TryRemove(reviewId, out review);
            }
            else
            {
                log.Warn("Review not found: {0}", reviewId);
            }
        }

        public IArffDataRow AddReview()
        {
            if (Normalization != NormalizationType.None)
            {
                throw new ArgumentException("Can't add new review to normalized dataset");
            }

            Interlocked.Increment(ref internalReviewsOffset);
            return GetReview(internalReviewsOffset);
        }

        public void Save(string fileName)
        {
            using (var file = new StreamWriter(fileName))
            {
                Save(file);
            }
        }

        public void Save(StreamWriter stream)
        {
            stream.WriteLine(ToString());
            foreach (var review in Reviews)
            {
                var text = review.ToString();
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                stream.WriteLine(text);
            }
        }

        public IArffDataRow GetReview(int reviewId)
        {
            IArffDataRow review;
            if (!reviews.TryGetValue(reviewId, out review))
            {
                review = new ArffDataRow(reviewId, this);
                reviews[reviewId] = review;
            }

            return review;
        }

        public static IArffDataSet Create<T>(string name)
        {
            var dataSet = new ArffDataSet(new HeadersWordsHandling(), name);
            dataSet.Header.CreateHeader = true;
            dataSet.Header.RegisterEnumClass<T>();
            return dataSet;
        }

        public static IArffDataSet CreateSimple(string name)
        {
            var dataSet = new ArffDataSet(new HeadersWordsHandling(), name);
            dataSet.Header.CreateHeader = true;
            return dataSet;
        }

        public static IArffDataSet CreateDataRecord<T>(string[] types)
        {
            var reviewHolder = Create<T>("Data");
            foreach (var word in types)
            {
                reviewHolder.Header.RegisterNumeric(word);
            }

            return reviewHolder;
        }

        public static IArffDataSet CreateFixed(IHeadersWordsHandling header, string name)
        {
            var dataSet = new ArffDataSet(header, name);
            dataSet.Header.CreateHeader = false;
            return dataSet;
        }

        public static IArffDataSet LoadSimple(string fileName)
        {
            Guard.NotNullOrEmpty(() => fileName, fileName);
            using (var reader = new StreamReader(fileName))
            {
                return LoadSimple(reader);
            }
        }

        public static IArffDataSet Load<T>(string fileName)
        {
            Guard.NotNullOrEmpty(() => fileName, fileName);
            using (var reader = new StreamReader(fileName))
            {
                return Load<T>(reader);
            }
        }

        public static IArffDataSet LoadSimple(StreamReader streamReader)
        {
            Guard.NotNull(() => streamReader, streamReader);
            return LoadInternal(streamReader, CreateSimple);
        }

        public static IArffDataSet Load<T>(StreamReader streamReader)
        {
            Guard.NotNull(() => streamReader, streamReader);
            return LoadInternal(streamReader, Create<T>);
        }

        private static IArffDataSet LoadInternal(StreamReader streamReader, Func<string, IArffDataSet> factory)
        {
            var headers = new List<string>();
            var name = string.Empty;
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                if (line.IndexOf("@data", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    break;
                }

                if (line.IndexOf("@RELATION", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var array = line.Split(' ');
                    if (array.Length < 2)
                    {
                        throw new ArgumentOutOfRangeException("fileName", "Failed to parse name");
                    }

                    name = array[1];
                }
                else if (line.IndexOf("@attribute", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    headers.Add(line);
                }
            }

            var reviewHolder = factory(name);
            reviewHolder.Header.CreateHeader = false;
            foreach (var headerItem in headers)
            {
                reviewHolder.Header.Parse(headerItem);
            }

            while ((line = streamReader.ReadLine()) != null)
            {
                var currentLine = line;
                ProcessLine(reviewHolder, currentLine);
            }

            return reviewHolder;
        }

        private static void ProcessLine(IArffDataSet reviewHolder, string line)
        {
            line = line.Replace("{", string.Empty);
            line = line.Replace("}", string.Empty);
            var review = reviewHolder.AddReview();
            foreach (var item in line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var itemBlocks = item.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (itemBlocks.Length < 2)
                {
                    log.Error("Can't process line: " + line);
                    throw new ArgumentOutOfRangeException("Can't process line: " + line);
                }

                var index = int.Parse(itemBlocks[0]);
                var header = reviewHolder.Header.GetByIndex(index);
                var wordItem = review.Resolve(header);
                wordItem.Value = header.Parse(itemBlocks.Skip(1).AccumulateItems(" "));
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("@RELATION {0}\r\n", name);

            foreach (var item in Header)
            {
                builder.AppendFormat("{0}{1}", item, Environment.NewLine);
            }

            builder.Append("@DATA");
            return builder.ToString();
        }

        private void HeaderWordsOnAdded(object sender, HeaderEventArgs headerEventArgs)
        {
            var header = headerEventArgs.Header as NumericHeader;
            if (header != null)
            {
                header.UseCount = UseTotal;
            }
        }

        private void HeaderWordsRemoved(object sender, HeaderEventArgs e)
        {
            foreach (var review in Reviews)
            {
                review.Remove(e.Header);
            }
        }
    }
}
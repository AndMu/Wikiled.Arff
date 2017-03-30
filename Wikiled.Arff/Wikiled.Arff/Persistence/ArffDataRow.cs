using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wikiled.Arff.Data;
using Wikiled.Arff.Persistence.Headers;
using Wikiled.Core.Utility.Arguments;

namespace Wikiled.Arff.Persistence
{
    public class ArffDataRow : IArffDataRow
    {
        private readonly DataRecord classRecord;

        private readonly IArffDataSet dataSet;

        private readonly ConcurrentDictionary<IHeader, DataRecord> records = new ConcurrentDictionary<IHeader, DataRecord>();

        internal ArffDataRow(int reviewId, IArffDataSet dataSet)
        {
            ReviewId = reviewId;
            this.dataSet = dataSet;
            if (dataSet.Header.Class != null)
            {
                classRecord = new DataRecord(dataSet.Header.Class);
            }
        }

        public int Count => records.Count;

        public DataRecord Class
        {
            get
            {
                if (classRecord == null)
                {
                    throw new ArgumentOutOfRangeException("No class defined");
                }

                if (classRecord.Header != dataSet.Header.Class)
                {
                    throw new InvalidOperationException("Can't change dataset class");
                }

                return classRecord;
            }
        }

        public IHeader[] Headers => records.Keys.ToArray();

        public int ReviewId { get; }

        public DataRecord this[string word]
        {
            get
            {
                IHeader header = dataSet.Header[word];
                if (header == null)
                {
                    throw new ArgumentNullException("word", "Unknown header " + word);
                }

                return records.ContainsKey(header) ? records[header] : null;
            }
        }

        public DataRecord Resolve(IHeader header)
        {
            IHeader existing = dataSet.Header[header.Name];
            if (existing == null)
            {
                return null;
            }

            DataRecord data = GetCreateRecord(existing);
            if (data.Header != dataSet.Header.Class)
            {
                data.Increment();
            }

            if (!existing.Contains(ReviewId))
            {
                existing.Add(ReviewId);
            }

            return data;
        }

        public DataRecord AddRecord(string name)
        {
            IHeader header = GetHeader(name);
            return header == null ? null : Resolve(header);
        }

        public bool Contains(IHeader header)
        {
            return records.ContainsKey(header);
        }

        public DataRecord[] GetRecords()
        {
            return records.Select(item => item.Value).ToArray();
        }

        public void ProcessLine(DataLine line)
        {
            var indexes = new Dictionary<int, double>();
            foreach (var wordsData in records)
            {
                if (wordsData.Value.Header is DateHeader)
                {
                    continue;
                }

                int index = dataSet.Header.GetIndex(wordsData.Key);
                double value = 1;
                if (wordsData.Value.Value != null)
                {
                    value = Convert.ToDouble(wordsData.Value.Value);
                }

                indexes[index] = value;
            }

            foreach (var index in indexes.OrderBy(item => item.Key))
            {
                line.SetValue(index.Key, index.Value);
            }
        }

        public void Remove(IHeader header)
        {
            DataRecord data;
            records.TryRemove(header, out data);
        }

        public DataRecord SetRecord(DataRecord oldRecord)
        {
            Guard.NotNull(() => oldRecord, oldRecord);
            IHeader header = dataSet.Header[oldRecord.Header.Name] ?? dataSet.Header.RegisterHeader(oldRecord.Header);
            DataRecord value = GetCreateRecord(header);
            value.Total = oldRecord.Total;
            value.Value = oldRecord.Value;
            return value;
        }

        public IEnumerable<DataRecord> GetFullView()
        {
            for (int i = 0; i < dataSet.Header.Total; i++)
            {
                IHeader header = dataSet.Header.GetByIndex(i);
                DataRecord record;
                if (records.TryGetValue(header, out record))
                {
                    yield return record;
                }
                else
                {
                    yield return new DataRecord(header);
                }
            }
        }

        public override string ToString()
        {
            var line = new SparseInformationLine();

            var wordItems = records.Select(
                item => new
                {
                    Item = item,
                    Index = dataSet.Header.GetIndex(item.Key)
                })
                .OrderBy(item => item.Index);

            bool saved = false;
            foreach (var value in wordItems)
            {
                saved = true;
                if (value.Index < 0)
                {
                    continue;
                }

                line.Add(value.Index, value.Item.Key.ReadValue(value.Item.Value));
            }

            if (!saved)
            {
                return string.Empty;
            }

            if (classRecord != null)
            {
                line.Add(dataSet.Header.Total - 1, classRecord.Header.ReadValue(classRecord));
            }

            return line.GenerateLine();
        }

        private DataRecord GetCreateRecord(IHeader header)
        {
            DataRecord data;
            if (records.TryGetValue(header, out data))
            {
                return data;
            }

            if (header == dataSet.Header.Class)
            {
                return Class;
            }

            data = new DataRecord(header);
            records[header] = data;
            return data;
        }

        private IHeader GetHeader(string word)
        {
            word = HeadersWordsHandling.GetRegularWord(word);
            Guard.NotNull(() => word, word);
            IHeader header = dataSet.Header[word];
            if (header == null &&
                dataSet.Header.CreateHeader)
            {
                header = dataSet.Header.RegisterNumeric(word);
            }

            return header;
        }
    }
}
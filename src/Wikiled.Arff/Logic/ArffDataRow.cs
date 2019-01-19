using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Wikiled.Arff.Logic.Headers;

namespace Wikiled.Arff.Logic
{
    public class ArffDataRow : IArffDataRow
    {
        private readonly DataRecord classRecord;

        private readonly ConcurrentDictionary<IHeader, DataRecord> records = new ConcurrentDictionary<IHeader, DataRecord>();

        private DateTime? date;

        internal ArffDataRow(string docId, IArffDataSet dataSet)
        {
            Id = docId;
            Owner = dataSet ?? throw new ArgumentNullException(nameof(dataSet));
            if (dataSet.Header.Class != null)
            {
                classRecord = new DataRecord(dataSet.Header.Class);
            }

            if (dataSet.HasId)
            {
                var idField = dataSet.Header[Constants.IdField];
                AddRecord(idField).Value = docId;
            }
        }

        public IArffDataSet Owner { get; }

        public int Count => records.Count;

        public DataRecord Class
        {
            get
            {
                if (classRecord == null)
                {
                    throw new ArgumentOutOfRangeException("No class defined");
                }

                if (classRecord.Header != Owner.Header.Class)
                {
                    throw new InvalidOperationException("Can't change dataset class");
                }

                return classRecord;
            }
        }

        public IDictionary<IHeader, DataRecord> HeadersTable => records;

        public IHeader[] Headers => records.Keys.ToArray();

        public string Id { get; }

        public DateTime? Date
        {
            get
            {
                var field = Owner.Header[Constants.DATE];
                if (field == null)
                {
                    return null;
                }

                if (!HeadersTable.TryGetValue(field, out var value))
                {
                    return null;
                }

                return value.Value as DateTime?;
            }
            set
            {
                if (!Owner.HasDate)
                {
                    throw new InvalidOperationException("Please enable date on the dataset");
                }

                var field = Owner.Header[Constants.DATE];
                AddRecord(field).Value = value;
            }
        }

        public DataRecord this[string word]
        {
            get
            {
                IHeader header = Owner.Header[word];
                if (header == null)
                {
                    throw new ArgumentNullException(nameof(word), "Unknown header " + word);
                }

                return records.ContainsKey(header) ? records[header] : null;
            }
        }

        public DataRecord AddRecord(IHeader header)
        {
            IHeader existing = Owner.Header[header.Name];
            if (existing == null)
            {
                return null;
            }

            DataRecord data = GetCreateRecord(existing);
            if (data.Header != Owner.Header.Class)
            {
                data.Increment();
            }

            if (!existing.Contains(Id))
            {
                existing.Add(Id);
            }

            return data;
        }

        public DataRecord AddRecord(string name)
        {
            IHeader header = GetHeader(name);
            return header == null ? null : AddRecord(header);
        }

        public bool Contains(IHeader header)
        {
            return records.ContainsKey(header);
        }

        public DataRecord[] GetRecords()
        {
            return records.Select(item => item.Value).ToArray();
        }

        public void Remove(IHeader header)
        {
            records.TryRemove(header, out _);
        }

        public DataRecord SetRecord(DataRecord oldRecord)
        {
            if (oldRecord == null)
            {
                throw new ArgumentNullException(nameof(oldRecord));
            }

            IHeader header = Owner.Header[oldRecord.Header.Name] ?? Owner.Header.RegisterHeader(oldRecord.Header);
            DataRecord value = GetCreateRecord(header);
            value.Total = oldRecord.Total;
            value.Value = oldRecord.Value;
            return value;
        }

        public IEnumerable<DataRecord> GetFullView()
        {
            for (int i = 0; i < Owner.Header.Total; i++)
            {
                IHeader header = Owner.Header.GetByIndex(i);
                if (records.TryGetValue(header, out DataRecord record))
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
                    Index = Owner.Header.GetIndex(item.Key)
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
                line.Add(Owner.Header.Total - 1, classRecord.Header.ReadValue(classRecord));
            }

            return line.GenerateLine();
        }

        private DataRecord GetCreateRecord(IHeader header)
        {
            if (records.TryGetValue(header, out DataRecord data))
            {
                return data;
            }

            if (header == Owner.Header.Class)
            {
                return Class;
            }

            data = new DataRecord(header);
            records[header] = data;
            return data;
        }

        private IHeader GetHeader(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                throw new ArgumentException("message", nameof(word));
            }

            word = HeadersWordsHandling.GetRegularWord(word);
            IHeader header = Owner.Header[word];
            if (header == null &&
                Owner.Header.CreateHeader)
            {
                header = Owner.Header.RegisterNumeric(word);
            }

            return header;
        }
    }
}
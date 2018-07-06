using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CsvHelper;
using NLog;
using Wikiled.Arff.Persistence.Headers;
using Wikiled.Common.Extensions;

namespace Wikiled.Arff.Persistence
{
    public class ArffDataSet : IArffDataSet
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<int, IArffDataRow> documents = new ConcurrentDictionary<int, IArffDataRow>();

        private readonly string name;

        private int internalDocumentsOffset = 100000;

        private bool useTotal;

        private bool isSparse;

        private ArffDataSet(IHeadersWordsHandling header, string name)
        {
            isSparse = true;
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Header.Removed += HeaderWordsRemoved;
            Header.Added += HeaderWordsOnAdded;
            this.name = string.IsNullOrEmpty(name) ? "DATA" : name;
        }

        public IEnumerable<IArffDataRow> Documents
        {
            get
            {
                return from item in documents.OrderBy(item => item.Key) select item.Value;
            }
        }

        public IHeadersWordsHandling Header { get; }

        public int TotalDocuments => documents.Count;

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

        public bool IsSparse
        {
            get => isSparse;
            set
            {
                foreach (NumericHeader header in Header.Where(item => item is NumericHeader))
                {
                    header.IsSparse = value;
                }

                isSparse = value;
            }
        }

        public static IArffDataSet Create<T>(string name)
        {
            var dataSet = new ArffDataSet(new HeadersWordsHandling(), name);
            dataSet.Header.CreateHeader = true;
            dataSet.Header.RegisterEnumClass<T>();
            return dataSet;
        }

        public static IArffDataSet CreateDataRecord<T>(string[] types)
        {
            var documentHolder = Create<T>("Data");
            foreach (var word in types)
            {
                documentHolder.Header.RegisterNumeric(word);
            }

            return documentHolder;
        }

        public static IArffDataSet CreateFixed(IHeadersWordsHandling header, string name)
        {
            var dataSet = new ArffDataSet(header, name);
            dataSet.Header.CreateHeader = false;
            return dataSet;
        }

        public static IArffDataSet CreateSimple(string name)
        {
            var dataSet = new ArffDataSet(new HeadersWordsHandling(), name);
            dataSet.Header.CreateHeader = true;
            return dataSet;
        }

        public static IArffDataSet Load<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(fileName));
            }

            using (var reader = new StreamReader(fileName))
            {
                return Load<T>(reader);
            }
        }

        public static IArffDataSet Load<T>(StreamReader streamReader)
        {
            if (streamReader == null)
            {
                throw new ArgumentNullException(nameof(streamReader));
            }

            return LoadInternal(streamReader, Create<T>);
        }

        public static IArffDataSet LoadSimple(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(fileName));
            }

            using (var reader = new StreamReader(fileName))
            {
                return LoadSimple(reader);
            }
        }

        public static IArffDataSet LoadSimple(StreamReader streamReader)
        {
            if (streamReader == null)
            {
                throw new ArgumentNullException(nameof(streamReader));
            }

            return LoadInternal(streamReader, CreateSimple);
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

        public IArffDataRow AddDocument()
        {
            Interlocked.Increment(ref internalDocumentsOffset);
            return GetOrCreateDocument(internalDocumentsOffset);
        }

        public void Clear()
        {
            documents.Clear();
        }

        public IArffDataRow GetOrCreateDocument(int documentId)
        {
            if (!documents.TryGetValue(documentId, out IArffDataRow doc))
            {
                doc = new ArffDataRow(documentId, this);
                documents[documentId] = doc;
            }

            return doc;
        }

        public void RemoveDocument(int documentId)
        {
            var doc = GetOrCreateDocument(documentId);
            if (doc != null)
            {
                foreach (var headers in doc.Headers)
                {
                    headers.Remove(doc.Id);
                }

                documents.TryRemove(documentId, out doc);
            }
            else
            {
                log.Warn("Document not found: {0}", documentId);
            }
        }

        public void SaveCsv(string fileName)
        {
            using (var streamWriter = new StreamWriter(fileName, false))
            using (var csvDataOut = new CsvWriter(streamWriter))
            {                
                var headers = Header.ToArray();
                foreach (var header in headers)
                {
                    csvDataOut.WriteField(header.Name);
                }

                csvDataOut.NextRecord();
                foreach (var doc in Documents)
                {
                    foreach (var header in headers)
                    {
                        string value;
                        if (doc.Class.Header == header)
                        {
                            value = header.ReadValue(doc.Class);
                        }
                        else
                        {
                            value = doc.HeadersTable.TryGetValue(header, out var record) ? header.ReadValue(record) : null;
                        }
                        
                        csvDataOut.WriteField(value);
                    }

                    csvDataOut.NextRecord();
                }
            }
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
            foreach (var doc in Documents)
            {
                var text = doc.ToString();
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                stream.WriteLine(text);
            }
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

            var docHolder = factory(name);
            docHolder.Header.CreateHeader = false;
            foreach (var headerItem in headers)
            {
                docHolder.Header.Parse(headerItem);
            }

            while ((line = streamReader.ReadLine()) != null)
            {
                var currentLine = line;
                ProcessLine(docHolder, currentLine);
            }

            return docHolder;
        }

        private static void ProcessLine(IArffDataSet docHolder, string line)
        {
            line = line.Replace("{", string.Empty);
            line = line.Replace("}", string.Empty);
            var doc = docHolder.AddDocument();
            foreach (var item in line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var itemBlocks = item.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (itemBlocks.Length < 2)
                {
                    log.Error("Can't process line: " + line);
                    throw new ArgumentOutOfRangeException("Can't process line: " + line);
                }

                var index = int.Parse(itemBlocks[0]);
                var header = docHolder.Header.GetByIndex(index);
                var wordItem = doc.AddRecord(header);
                wordItem.Value = header.Parse(itemBlocks.Skip(1).AccumulateItems(" "));
            }
        }

        private void HeaderWordsOnAdded(object sender, HeaderEventArgs headerEventArgs)
        {
            if (headerEventArgs.Header is NumericHeader header)
            {
                header.UseCount = UseTotal;
                header.IsSparse = IsSparse;
            }
        }

        private void HeaderWordsRemoved(object sender, HeaderEventArgs e)
        {
            foreach (var doc in Documents)
            {
                doc.Remove(e.Header);
            }
        }
    }
}

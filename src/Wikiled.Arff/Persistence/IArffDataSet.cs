using System.Collections.Generic;
using System.IO;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Persistence
{
    public interface IArffDataSet
    {
        string Name { get; }

        IHeadersWordsHandling Header { get; }

        IEnumerable<IArffDataRow> Documents { get; }

        int TotalDocuments { get; }

        bool UseTotal { get; set; }

        bool IsSparse { get; set; }

        void Save(string fileName);

        void Save(StreamWriter stream);

        IArffDataRow AddDocument();

        IArffDataRow GetOrCreateDocument(int documentId);

        void Clear();

        void RemoveDocument(int documentId);

        void SaveCsv(string fileName);
    }
}
using System.Collections.Generic;
using System.IO;
using Wikiled.Arff.Logic.Headers;

namespace Wikiled.Arff.Logic
{
    public interface IArffDataSet
    {
        string Name { get; }

        IHeadersWordsHandling Header { get; }

        IEnumerable<IArffDataRow> Documents { get; }

        int TotalDocuments { get; }

        bool UseTotal { get; set; }

        bool IsSparse { get; set; }

        bool HasId { get; set; }

        void Save(string fileName);

        void Save(StreamWriter stream);

        IArffDataRow AddDocument();

        IArffDataRow GetOrCreateDocument(string documentId);

        void Clear();

        void RemoveDocument(string documentId);

        void SaveCsv(string fileName);
    }
}
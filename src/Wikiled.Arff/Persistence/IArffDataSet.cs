using System.Collections.Generic;
using System.IO;
using Wikiled.Arff.Normalization;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Persistence
{
    public interface IArffDataSet
    {
        IHeadersWordsHandling Header { get; }

        NormalizationType Normalization { get; }

        IEnumerable<IArffDataRow> Documents { get; }

        int TotalDocuments { get; }

        bool UseTotal { get; set; }

        void Save(string fileName);

        void Save(StreamWriter stream);

        IArffDataRow AddDocument();

        IArffDataRow GetDocument(int documentId);

        void Normalize(NormalizationType type);

        void Clear();

        void RemoveDocument(int documentId);

        void SaveCsv(string fileName);
    }
}
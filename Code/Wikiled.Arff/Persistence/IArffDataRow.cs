using System.Collections.Generic;
using Wikiled.Arff.Data;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Persistence
{
    public interface IArffDataRow
    {
        int Count { get; }

        DataRecord Class { get; }

        IHeader[] Headers { get; }

        int Id { get; }

        DataRecord this[string word] { get; }

        DataRecord Resolve(IHeader header);

        DataRecord AddRecord(string word);

        bool Contains(IHeader header);

        DataRecord[] GetRecords();

        void ProcessLine(DataLine line);

        void Remove(IHeader header);

        DataRecord SetRecord(DataRecord oldRecord);

        IEnumerable<DataRecord> GetFullView();
    }
}
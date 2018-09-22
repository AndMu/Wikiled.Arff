﻿using System.Collections.Generic;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Persistence
{
    public interface IArffDataRow
    {
        string Key { get; set; }

        IArffDataSet Owner { get; }

        int Count { get; }

        DataRecord Class { get; }

        IHeader[] Headers { get; }

        IDictionary<IHeader, DataRecord> HeadersTable { get; }

        int Id { get; }

        DataRecord this[string word] { get; }

        DataRecord AddRecord(IHeader header);

        DataRecord AddRecord(string word);

        bool Contains(IHeader header);

        DataRecord[] GetRecords();

        void Remove(IHeader header);

        DataRecord SetRecord(DataRecord oldRecord);

        IEnumerable<DataRecord> GetFullView();
    }
}
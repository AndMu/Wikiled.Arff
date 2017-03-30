using System;
using System.Collections.Generic;
using System.IO;
using Wikiled.Arff.Normalization;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Persistence
{
    public interface IArffDataSet
    {
        IHeadersWordsHandling Header { get; }

        Func<IEnumerable<IArffDataRow>, IEnumerable<IArffDataRow>> Sort { get; set; }

        NormalizationType Normalization { get; }

        IArffDataRow[] Reviews { get; }

        int TotalReviews { get; }

        bool UseTotal { get; set; }

        void Save(string fileName);

        void Save(StreamWriter stream);

        IArffDataRow AddReview();

        IArffDataRow GetReview(int reviewId);

        void Normalize(NormalizationType type);

        void Clear();

        void RemoveReview(int reviewId);}
}
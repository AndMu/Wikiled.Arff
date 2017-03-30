using System;

namespace Wikiled.Arff.Persistence.Headers
{
    public interface IHeader : ICloneable
    {
        string Name { get; }

        object Source { get; set; }

        void Remove(int reviewId);

        bool Contains(int reviewId);

        string ReadValue(DataRecord record);

        int InReviews { get; }

        void Add(int reviewId);

        object Parse(string text);

        void CheckSupport(object value);
    }
}
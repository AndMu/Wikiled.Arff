using System;

namespace Wikiled.Arff.Persistence.Headers
{
    public interface IHeader : ICloneable
    {
        int InDocuments { get; }

        string Name { get; }

        object Source { get; set; }

        void Add(int docId);

        void CheckSupport(object value);

        bool Contains(int docId);

        object Parse(string text);

        string ReadValue(DataRecord record);

        void Remove(int docId);
    }
}

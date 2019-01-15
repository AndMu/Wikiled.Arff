using System;

namespace Wikiled.Arff.Logic.Headers
{
    public interface IHeader : ICloneable
    {
        int InDocuments { get; }

        string Name { get; }

        object Source { get; set; }

        void Add(string docId);

        void CheckSupport(object value);

        bool Contains(string docId);

        object Parse(string text);

        string ReadValue(DataRecord record);

        void Remove(string docId);
    }
}

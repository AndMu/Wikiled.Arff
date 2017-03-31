using System;

namespace Wikiled.Arff.Persistence.Headers
{
    public interface IHeader : ICloneable
    {
        string Name { get; }

        object Source { get; set; }

        void Remove(int docId);

        bool Contains(int docId);

        string ReadValue(DataRecord record);

        int Indocs { get; }

        void Add(int docId);

        object Parse(string text);

        void CheckSupport(object value);
    }
}
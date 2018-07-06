using System;
using System.Collections.Concurrent;
using System.IO;

namespace Wikiled.Arff.Persistence.Headers
{
    public abstract class BaseHeader : IHeader
    {
        private readonly ConcurrentDictionary<int, int> docs = new ConcurrentDictionary<int, int>();

        private string text;

        protected BaseHeader(int index, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            Name = string.Intern(name);
            Index = index;
        }

        public int Index { get; }

        public int InDocuments => docs.Count;

        public string Name { get; }

        public object Source { get; set; }

        public abstract bool CheckSupport(Type value);

        public abstract object Clone();

        public abstract object Parse(string text);

        public override string ToString()
        {
            if (string.IsNullOrEmpty(text))
            {
                if (!string.IsNullOrEmpty(Name) &&
                    (Name.Contains(" ") || Name.Contains("'") || Name.Contains(",")))
                {
                    text = $"@ATTRIBUTE \"{Name}\" {GetAdditionalText()}";
                }
                else
                {
                    text = $"@ATTRIBUTE {Name} {GetAdditionalText()}";
                }
            }

            return text;
        }

        public void Add(int docId)
        {
            docs.TryAdd(docId, docId);
        }

        public void CheckSupport(object value)
        {
            if (!IsSupported(value))
            {
                throw new InvalidDataException($"{value} is not supported by {Name}");
            }
        }

        public bool Contains(int docId)
        {
            return docs.ContainsKey(docId);
        }

        public string ReadValue(DataRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (record.Header != this)
            {
                throw new ArgumentOutOfRangeException(nameof(record), "Invalid record");
            }

            return ReadValueInternal(record);
        }

        public void Remove(int docId)
        {
            docs.TryRemove(docId, out _);
        }

        protected abstract string GetAdditionalText();

        protected abstract bool IsSupported(object value);

        protected abstract string ReadValueInternal(DataRecord record);
    }
}

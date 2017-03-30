using System;

namespace Wikiled.Arff.Persistence.Headers
{
    public class StringHeader : BaseHeader
    {
        private const string Tag = "STRING";

        public StringHeader(int index, string name)
            : base(index, name)
        {
        }

        public override object Clone()
        {
            return new StringHeader(Index, Name);
        }

        public static bool CanCreate(string[] items)
        {
            return string.Compare(items[2], Tag, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override object Parse(string text)
        {
            return text;
        }

        public override bool CheckSupport(Type value)
        {
            return value == typeof(string);
        }

        protected override string GetAdditionalText()
        {
            return Tag;
        }
        
        protected override string ReadValueInternal(DataRecord record)
        {
            CheckSupport(record.Value);
            return record.Value?.ToString() ?? "NULL";
        }

        protected override bool IsSupported(object value)
        {
            return value is string;
        }
    }
}

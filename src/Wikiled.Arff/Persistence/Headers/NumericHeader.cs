using System;
using Wikiled.Common.Reflection;

namespace Wikiled.Arff.Persistence.Headers
{
    public class NumericHeader : BaseHeader, IClassHeader
    {
        private const string Tag = "NUMERIC";

        public NumericHeader(int index, string name)
            : base(index, name)
        {
        }

        public bool UseCount { get; set; }

        public bool IsSparse { get; set; }

        public static bool CanCreate(string[] items)
        {
            return string.Compare(items[2], Tag, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override bool CheckSupport(Type value)
        {
            return value.IsNumericType();
        }

        public override object Clone()
        {
            return new NumericHeader(Index, Name);
        }

        public override object Parse(string text)
        {
            if (!double.TryParse(text, out var result))
            {
                return null;
            }

            return result;
        }

        public object GetValueByClassId(int classId)
        {
            return classId;
        }

        public int ReadClassIdValue(DataRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return (int)record.Value;
        }

        protected override string GetAdditionalText()
        {
            return Tag;
        }

        protected override bool IsSupported(object value)
        {
            return value == null || CheckSupport(value.GetType());
        }

        protected override string ReadValueInternal(DataRecord record)
        {
            var defaultValue = IsSparse ? string.Empty : "0";

            if (UseCount && record.Value == null)
            {
                return record.Total == 0 ? defaultValue : record.Total.ToString();
            }

            CheckSupport(record.Value);
            if (record.Value == null)
            {
                return string.Empty;
            }

            if (record.Value.Equals(0.0) || record.Value.Equals(0) || record.Value.Equals(0.0f) || record.Value.Equals(0.0d))
            {
                return defaultValue;
            }

            return record.Value.ToString();
        }
    }
}

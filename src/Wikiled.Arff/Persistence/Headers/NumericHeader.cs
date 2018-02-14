using System;
using Wikiled.Common.Arguments;
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
            double result;
            if (!double.TryParse(text, out result))
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
            Guard.NotNull(() => record, record);
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
            if (UseCount &&
                record.Value == null)
            {
                return record.Total == 0 ? string.Empty : record.Total.ToString();
            }

            CheckSupport(record.Value);
            if (record.Value == null ||
                record.Value.Equals(0.0) ||
                record.Value.Equals(0) ||
                record.Value.Equals(0.0f) ||
                record.Value.Equals(0.0d))
            {
                return string.Empty;
            }

            return record.Value.ToString();
        }
    }
}

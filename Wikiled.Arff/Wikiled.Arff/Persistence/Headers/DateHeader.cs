using System;
using System.Globalization;

namespace Wikiled.Arff.Persistence.Headers
{
    public class DateHeader : BaseHeader
    {
        private const string Tag = "DATE";

        public DateHeader(int index, string name, string format = "yyyy-MM-dd")
            : base(index, name)
        {
            Format = format;
        }

        public string Format { get; }

        public static bool CanCreate(string[] items)
        {
            return string.Compare(items[2], Tag, StringComparison.OrdinalIgnoreCase) == 0 && items.Length == 4;
        }

        public override object Parse(string text)
        {
            DateTime date;
            if (!DateTime.TryParseExact(text, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return null;
            }

            return date;
        }

        public override object Clone()
        {
            return new DateHeader(Index, Name, Format);
        }
      
        public override bool CheckSupport(Type value)
        {
            return value == typeof(DateTime);
        }

        protected override string ReadValueInternal(DataRecord record)
        {
            CheckSupport(record.Value);
            return ((DateTime)record.Value).ToString(Format);
        }

        protected override string GetAdditionalText()
        {
            return Tag + " " + Format;
        }

        protected override bool IsSupported(object value)
        {
            return value is DateTime;
        }
    }
}

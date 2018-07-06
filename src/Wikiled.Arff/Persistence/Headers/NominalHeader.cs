using System;
using System.Linq;
using System.Text;

namespace Wikiled.Arff.Persistence.Headers
{
    public class NominalHeader : BaseHeader, IClassHeader
    {
        public NominalHeader(int index, string name, string[] nominalValues)
            : base(index, name)
        {
            if (nominalValues.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(nominalValues));
            }

            Nominals = nominalValues;
        }

        public string[] Nominals { get; set; }

        public override object Clone()
        {
            return new NominalHeader(Index, Name, Nominals);
        }

        public virtual int ReadClassIdValue(DataRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return Array.IndexOf(Nominals, (string)record.Value);
        }

        public virtual object GetValueByClassId(int classId)
        {
            return Nominals[classId];
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
            var builder = new StringBuilder();
            builder.Append("{");

            for (int i = 0; i < Nominals.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(Nominals[i]);
            }

            builder.Append("}");
            return builder.ToString();
        }
        
        protected override string ReadValueInternal(DataRecord record)
        {
            CheckSupport(record.Value);
            return (string)record.Value;
        }

        protected override bool IsSupported(object value)
        {
            return value == null || Nominals.Contains(value);
        }
    }
}
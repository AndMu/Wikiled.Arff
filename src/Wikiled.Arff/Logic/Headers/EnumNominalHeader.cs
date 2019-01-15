using System;
using System.Linq;

namespace Wikiled.Arff.Logic.Headers
{
    public class EnumNominalHeader : NominalHeader
    {
        private readonly Type type;

        private readonly string defaultValue;

        public EnumNominalHeader(int index, string name, Type type)
            : base(index, name, Enum.GetNames(type).OrderBy(item => item).ToArray())
        {
            this.type = type;
            defaultValue = Activator.CreateInstance(type).ToString();
        }

        public override object Clone()
        {
            return new EnumNominalHeader(Index, Name, type);
        }

        public override int ReadClassIdValue(DataRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return (int)record.Value;
        }

        public override object GetValueByClassId(int classId)
        {
            return Enum.ToObject(type, classId);
        }

        public override bool CheckSupport(Type value)
        {
            return value == type;
        }

        public override object Parse(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? Nominals[0] : Enum.Parse(type, text);
        }

        protected override string ReadValueInternal(DataRecord record)
        {
            CheckSupport(record.Value);
            return record.Value?.ToString() ?? defaultValue;
        }

        protected override bool IsSupported(object value)
        {
            return value == null || CheckSupport(value.GetType());
        }
    }
}

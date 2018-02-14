using Wikiled.Arff.Persistence.Headers;
using Wikiled.Common.Arguments;

namespace Wikiled.Arff.Persistence
{
    public class DataRecord
    {
        private object recordValue;

        public DataRecord(IHeader header)
        {
            Guard.NotNull(() => header, header);
            Header = header;
        }

        public IHeader Header { get; }

        public int Total { get; set; }

        public object Value
        {
            get => recordValue;
            set
            {
                Header.CheckSupport(value);
                recordValue = value;
            }
        }

        public void Increment()
        {
            Total++;
        }
    }
}
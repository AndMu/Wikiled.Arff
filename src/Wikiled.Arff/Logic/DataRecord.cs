using Wikiled.Arff.Logic.Headers;

namespace Wikiled.Arff.Logic
{
    public class DataRecord
    {
        private object recordValue;

        public DataRecord(IHeader header)
        {
            Header = header ?? throw new System.ArgumentNullException(nameof(header));
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
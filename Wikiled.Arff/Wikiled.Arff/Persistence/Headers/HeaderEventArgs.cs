using System;

namespace Wikiled.Arff.Persistence.Headers
{
    public class HeaderEventArgs : EventArgs
    {
        public HeaderEventArgs(IHeader header)
        {
            Header = header;
        }

        public IHeader Header { get; }
    }
}

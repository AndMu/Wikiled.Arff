using System;

namespace Wikiled.Arff.Logic.Headers
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

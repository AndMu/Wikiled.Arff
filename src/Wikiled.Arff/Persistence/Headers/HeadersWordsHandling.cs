using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wikiled.Common.Extensions;

namespace Wikiled.Arff.Persistence.Headers
{
    public class HeadersWordsHandling : IHeadersWordsHandling
    {
        public event EventHandler<HeaderEventArgs> Added;

        public event EventHandler<HeaderEventArgs> Removed;

        private readonly List<IHeader> headers = new List<IHeader>();

        private readonly Dictionary<string, IHeader> headerTable =
            new Dictionary<string, IHeader>(StringComparer.OrdinalIgnoreCase);

        private readonly ReaderWriterLockSlim syncSlim = new ReaderWriterLockSlim();

        public HeadersWordsHandling()
        {
            Register = true;
        }

        public IClassHeader Class { get; private set; }

        public bool CreateHeader { get; set; }

        public bool Register { get; set; }

        public int Total => headerTable.Count;

        public IHeader this[string name]
        {
            get
            {
                try
                {
                    syncSlim.EnterReadLock();
                    return headerTable.GetSafe(name);
                }
                finally
                {
                    syncSlim.ExitReadLock();
                }
            }
        }

        public static string GetRegularWord(string word)
        {
            if (IsReserved(word))
            {
                return word + "_word";
            }

            return word;
        }

        public static bool IsReserved(string word)
        {
            return string.Compare(word, "class", StringComparison.OrdinalIgnoreCase) == 0;
        }

        public object Clone()
        {
            return CopyHeader();
        }

        public HeadersWordsHandling CopyHeader(bool sorted = false)
        {
            var header = new HeadersWordsHandling();
            var selected = headerTable.Select(item => item);
            if (sorted)
            {
                selected = headerTable.OrderBy(item => item.Key);
            }

            foreach (var item in selected)
            {
                var cloned = (IHeader)item.Value.Clone();
                header.AddHeader(cloned);
                if (item.Value == Class)
                {
                    header.RegisterClass((IClassHeader)cloned);
                }
            }

            return header;
        }

        public IHeader GetByIndex(int index)
        {
            try
            {
                syncSlim.EnterReadLock();
                return headers[index];
            }
            finally
            {
                syncSlim.ExitReadLock();
            }
        }

        public IEnumerator<IHeader> GetEnumerator()
        {
            try
            {
                syncSlim.EnterReadLock();
                return headers.GetEnumerator();
            }
            finally
            {
                syncSlim.ExitReadLock();
            }
        }

        public int GetIndex(IHeader header)
        {
            try
            {
                syncSlim.EnterReadLock();
                return headers.IndexOf(header);
            }
            finally
            {
                syncSlim.ExitReadLock();
            }
        }

        public IHeader Parse(string line)
        {
            int index = line.IndexOf("@ATTRIBUTE", 0, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            string name;
            var quote = line.IndexOf("'");
            var nextQuote = line.LastIndexOf("'");
            string[] items;
            if (quote > -1 &&
                quote != nextQuote)
            {
                List<string> itemsCollect = new List<string>();
                itemsCollect.Add("@ATTRIBUTE");
                name = line.Substring(quote + 1, nextQuote - quote - 1);
                itemsCollect.Add(name);
                var remaining = line.Substring(nextQuote + 1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                itemsCollect.AddRange(remaining);
                items = itemsCollect.ToArray();
            }
            else
            {
                items = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length < 3)
                {
                    throw new ArgumentNullException(nameof(line), "Not enought blocks");
                }

                name = items[1].Trim();
            }

            if (headerTable.TryGetValue(name, out IHeader header))
            {
                if (header != Class)
                {
                    throw new ArgumentOutOfRangeException(nameof(line), "Dublicate line");
                }

                return header;
            }

            index = line.IndexOf("{", 0);
            if (index > 0)
            {
                int lastIndex = line.IndexOf("}", index);
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("line", "} not found");
                }

                line = line.Substring(index + 1, lastIndex - index - 1);
                var childItems = line.Split(',').Select(item => item.Trim()).ToArray();
                return string.Compare(name, "class", StringComparison.OrdinalIgnoreCase) == 0
                           ? RegisterNominalClass(childItems)
                           : RegisterNominal(name, childItems);
            }

            if (string.Compare(name, "class", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return RegisterNumericClass();
            }

            if (NumericHeader.CanCreate(items))
            {
                return RegisterNumeric(name);
            }

            if (DateHeader.CanCreate(items))
            {
                return RegisterDate(name, items[3]);
            }

            if (StringHeader.CanCreate(items))
            {
                return RegisterString(name);
            }

            throw new ArgumentOutOfRangeException("line", "Can't parse line: " + line);
        }

        public DateHeader RegisterDate(string name, string format = "yyyy-MM-dd")
        {
            try
            {
                syncSlim.EnterWriteLock();
                return RegisterHeader(name, true, x => new DateHeader(Total, x, format));
            }
            finally
            {
                syncSlim.ExitWriteLock();
            }
        }

        public EnumNominalHeader RegisterEnum<T>(string name)
        {
            try
            {
                syncSlim.EnterWriteLock();
                return RegisterHeader(name, true, x => new EnumNominalHeader(Total, x, typeof(T)));
            }
            finally
            {
                syncSlim.ExitWriteLock();
            }
        }

        public EnumNominalHeader RegisterEnumClass<T>()
        {
            try
            {
                syncSlim.EnterWriteLock();
                EnumNominalHeader header = RegisterHeader(
                    "class",
                    false,
                    x => new EnumNominalHeader(Total, x, typeof(T)));
                RegisterClass(header);
                return header;
            }
            finally
            {
                syncSlim.ExitWriteLock();
            }
        }

        public IHeader RegisterHeader(IHeader header)
        {
            if (header is DateHeader numeric)
            {
                return RegisterDate(numeric.Name, numeric.Format);
            }

            if (header is StringHeader stringHeader)
            {
                return RegisterString(stringHeader.Name);
            }

            if (header is NumericHeader)
            {
                return RegisterNumeric(header.Name);
            }

            return RegisterString(header.Name);
        }

        public NominalHeader RegisterNominal(string name, params string[] nominals)
        {
            try
            {
                syncSlim.EnterWriteLock();
                return RegisterHeader(name, true, x => new NominalHeader(Total, x, nominals));
            }
            finally
            {
                syncSlim.ExitWriteLock();
            }
        }

        public NominalHeader RegisterNominalClass(params string[] nominals)
        {
            try
            {
                syncSlim.EnterWriteLock();
                NominalHeader header = RegisterHeader("class", false, x => new NominalHeader(Total, x, nominals));
                RegisterClass(header);
                return header;
            }
            finally
            {
                syncSlim.ExitWriteLock();
            }
        }

        public NumericHeader RegisterNumeric(string name)
        {
            try
            {
                syncSlim.EnterWriteLock();
                return RegisterHeader(name, true, x => new NumericHeader(Total, x));
            }
            finally
            {
                syncSlim.ExitWriteLock();
            }
        }

        public NumericHeader RegisterNumericClass()
        {
            try
            {
                syncSlim.EnterWriteLock();
                NumericHeader header = RegisterHeader("class", false, x => new NumericHeader(Total, x));
                RegisterClass(header);
                return header;
            }
            finally
            {
                syncSlim.ExitWriteLock();
            }
        }

        public StringHeader RegisterString(string name)
        {
            try
            {
                syncSlim.EnterWriteLock();
                StringHeader header = RegisterHeader(name, true, x => new StringHeader(Total, x));
                return header;
            }
            finally
            {
                syncSlim.ExitWriteLock();
            }
        }

        public void Remove(IHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            try
            {
                syncSlim.EnterWriteLock();
                headerTable.Remove(header.Name);
                headers.Remove(header);
                if (Class == header)
                {
                    Class = null;
                }
            }
            finally
            {
                syncSlim.ExitWriteLock();
            }

            Removed?.Invoke(this, new HeaderEventArgs(header));
        }

        private void AddHeader(IHeader header)
        {
            headerTable[header.Name] = header;
            int totalRegistered = Class != null ? 1 : 0;
            totalRegistered = header is DateHeader ? headers.Count : totalRegistered;
            headers.Insert(headers.Count - totalRegistered, header);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void RegisterClass(IClassHeader header)
        {
            if (Class == header)
            {
                return;
            }

            if (Class != null)
            {
                throw new InvalidOperationException("Class is already registered");
            }

            Class = header;
            headers.Remove(Class);
            headers.Add(Class); // move to last position
        }

        private T RegisterHeader<T>(string name, bool check, Func<string, T> create)
            where T : IHeader
        {
            if (check)
            {
                name = GetRegularWord(name);
            }

            if (!Register)
            {
                return create(name);
            }

            if (headerTable.TryGetValue(name, out IHeader header))
            {
                return (T)header;
            }

            header = create(name);
            AddHeader(header);
            Added?.Invoke(this, new HeaderEventArgs(header));
            return (T)header;
        }
    }
}

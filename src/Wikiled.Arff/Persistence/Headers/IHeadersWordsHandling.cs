using System;
using System.Collections.Generic;

namespace Wikiled.Arff.Persistence.Headers
{
    public interface IHeadersWordsHandling : IEnumerable<IHeader>, ICloneable
    {
        event EventHandler<HeaderEventArgs> Added;

        event EventHandler<HeaderEventArgs> Removed;

        IClassHeader Class { get; }

        bool CreateHeader { get; set; }

        bool Register { get; set; }

        int Total { get; }

        IHeader this[string name] { get; }

        IHeader GetByIndex(int index);

        int GetIndex(IHeader header);

        IHeader Parse(string line);

        HeadersWordsHandling CopyHeader(bool sorted = false);

        IHeader RegisterHeader(IHeader header);
        
        StringHeader RegisterString(string name);

        DateHeader RegisterDate(string name, string format = "yyyy-MM-dd");

        NominalHeader RegisterNominal(string name, string[] nominals);

        NominalHeader RegisterNominalClass(params string[] nominals);

        EnumNominalHeader RegisterEnum<T>(string name);

        EnumNominalHeader RegisterEnumClass<T>();

        NumericHeader RegisterNumeric(string name);
        
        NumericHeader RegisterNumericClass();

        void Remove(IHeader header);
    }
}
using System.Collections.Generic;

namespace Wikiled.Arff.Normalization
{
    public interface INormalize
    {
        IEnumerable<double> GetNormalized { get; }
        double Coeficient { get; }
        NormalizationType Type { get; }
    }
}
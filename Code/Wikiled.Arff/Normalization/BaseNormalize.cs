using System;
using System.Collections.Generic;
using System.Linq;

namespace Wikiled.Arff.Normalization
{
    public abstract class BaseNormalize : INormalize
    {
        protected IEnumerable<double> source;
        private readonly Lazy<double> coeficient;
        private readonly Lazy<IEnumerable<double>> normalized;

        protected BaseNormalize(IEnumerable<double> source)
        {
            this.source = source;
            coeficient = new Lazy<double>(CalculateCoef);
            normalized = new Lazy<IEnumerable<double>>(() => source.Select(item =>  Math.Round(item / Coeficient, 10))); 
        }

        protected abstract double CalculateCoef();

        public IEnumerable<double> GetNormalized => normalized.Value;

        public double Coeficient
        {
            get
            {
                var coef = coeficient.Value;
                return Math.Abs(coef - 0) < 0.0000001 ? 1 : coef;
            }
        }

        public abstract NormalizationType Type { get; }
    }
}

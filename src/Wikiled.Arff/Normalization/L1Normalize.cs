using System;
using System.Collections.Generic;
using System.Linq;

namespace Wikiled.Arff.Normalization
{
    public class L1Normalize : BaseNormalize
    {
        public L1Normalize(IEnumerable<double> source) 
            : base(source)
        {
        }

        protected override double CalculateCoef()
        {
            return source.Sum(item => Math.Abs(item));
        }

        public override NormalizationType Type => NormalizationType.L1;
    }
}

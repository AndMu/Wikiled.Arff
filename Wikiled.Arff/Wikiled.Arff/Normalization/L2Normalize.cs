using System;
using System.Collections.Generic;
using System.Linq;

namespace Wikiled.Arff.Normalization
{
    public class L2Normalize : BaseNormalize
    {
        public L2Normalize(IEnumerable<double> source)
            : base(source)
        {
        }

        protected override double CalculateCoef()
        {
            return Math.Sqrt(source.Sum(item => item * item));
        }

        public override NormalizationType Type => NormalizationType.L2;
    }
}

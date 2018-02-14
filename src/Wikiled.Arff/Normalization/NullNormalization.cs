﻿using System.Collections.Generic;

namespace Wikiled.Arff.Normalization
{
    public class NullNormalization : BaseNormalize
    {
        public NullNormalization(IEnumerable<double> source)
            : base(source)
        {
        }

        public override NormalizationType Type => NormalizationType.None;

        protected override double CalculateCoef()
        {
            return 1;
        }
    }
}

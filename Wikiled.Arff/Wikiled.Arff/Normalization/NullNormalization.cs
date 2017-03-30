using System.Collections.Generic;

namespace Wikiled.Arff.Normalization
{
    public class NullNormalization : BaseNormalize
    {
        public NullNormalization(IEnumerable<double> source)
            : base(source)
        {
        }

        protected override double CalculateCoef()
        {
            return 1;
        }

        public override NormalizationType Type
        {
            get { return NormalizationType.None; }
        }
    }
}

using System.Collections.Generic;

namespace Wikiled.Arff.Normalization
{
    public class ElasticNormalization : BaseNormalize
    {
        private L1Normalize l1;
        private L2Normalize l2;

        public ElasticNormalization(IEnumerable<double> source)
            : base(source)
        {
            l1 = new L1Normalize(source);
            l2 = new L2Normalize(source);
        }

        protected override double CalculateCoef()
        {
            return 0.15 * l1.Coeficient + 0.85 * l2.Coeficient;
        }

        public override NormalizationType Type
        {
            get { return NormalizationType.Elastic; }
        }
    }
}

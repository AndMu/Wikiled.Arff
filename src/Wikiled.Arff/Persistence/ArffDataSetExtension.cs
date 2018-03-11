using System;
using System.Collections.Generic;
using System.Linq;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Persistence
{
    public static class ArffDataSetExtension
    {
        public static IEnumerable<(int Y, double[] X)> GetData(this IArffDataSet dataSet)
        {
            foreach (var dataRow in dataSet.Documents)
            {
                int y = dataSet.Header.Class.ReadClassIdValue(dataRow.Class);
                yield return (y, dataRow.GetX());
            }
        }

        public static double[] GetX(this IArffDataRow row)
        {
            double[] x = new double[row.Headers.Length];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 0;
            }

            foreach (var wordsData in row.GetRecords().Where(item => !(item.Header is DateHeader)))
            {
                int index = row.Owner.Header.GetIndex(wordsData.Header);
                double value = 1;
                if (wordsData.Value != null)
                {
                    value = Convert.ToDouble(wordsData.Value);
                }

                x[index] = value;
            }

            return x;
        }
    }
}

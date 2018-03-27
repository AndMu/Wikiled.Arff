using System;
using System.Collections.Generic;
using System.Linq;
using Wikiled.Arff.Persistence.Headers;

namespace Wikiled.Arff.Persistence
{
    public static class ArffDataSetExtension
    {
        public static IEnumerable<(int? Y, double[] X)> GetData(this IArffDataSet dataSet)
        {
            var table = GetFeatureTable(dataSet);
            foreach (var dataRow in dataSet.Documents)
            {
                var y = dataSet.Header.Class?.ReadClassIdValue(dataRow.Class);
                yield return (y, dataRow.GetX(table));
            }
        }

        public static Dictionary<IHeader, int> GetFeatureTable(this IArffDataSet dataSet)
        {
            var headers = dataSet.Header.Where(item => dataSet.Header.Class != item && !(item is DateHeader)).OrderBy(item => dataSet.Header.GetIndex(item));
            var table = new Dictionary<IHeader, int>();
            int index = 0;
            foreach (var header in headers)
            {
                table[header] = index;
                index++;
            }

            return table;
        }

        private static double[] GetX(this IArffDataRow row, Dictionary<IHeader, int> headers)
        {
            double[] x = new double[headers.Count];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 0;
            }

            foreach (var wordsData in row.GetRecords())
            {
                if (!headers.TryGetValue(wordsData.Header, out var index))
                {
                    continue;
                }

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

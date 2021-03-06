﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Wikiled.Arff.Logic
{
    public class SparseInformationLine : IInformationLine
    {
        private readonly Dictionary<int, string> data = new Dictionary<int, string>();

        private StringBuilder builder;

        public int Index { get; private set; }

        public void Add(int currentIndex, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (currentIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentIndex));
            }

            if (data.ContainsKey(currentIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(currentIndex), "Index is already added - " + currentIndex);
            }

            data[currentIndex] = value;
            MoveIndex(currentIndex);
        }

        public void Add(string value)
        {
            Index++;
            Add(Index - 1, value);
        }

        public string GenerateLine()
        {
            if (builder != null)
            {
                return builder.ToString();
            }

            builder = new StringBuilder();
            foreach (var item in data)
            {
                AddItem(item.Key, item.Value);
            }

            builder.Insert(0, "{");
            builder.Append("}");
            return builder.ToString();
        }

        public void MoveIndex(int index)
        {
            Index = index > Index ? index : Index;
        }

        private void AddItem(int currentIndex, string value)
        {
            if (builder.Length > 0)
            {
                builder.Append(',');
            }

            builder.Append(currentIndex + " " + value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MarginTrading.Activities.Core.Domain
{
    public class Asset
    {
        public Asset(string id, string name, int accuracy)
        {
            Id = id;
            Name = name;
            Accuracy = accuracy;
        }

        public string Id { get; }
        public string Name { get; }
        public int Accuracy { get; }
    }
}
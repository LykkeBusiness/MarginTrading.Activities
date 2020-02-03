using MarginTrading.Activities.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace MarginTrading.Activities.Core.Caches
{
    public interface IAssetsCache
    {
        string GetName(string id);

        int GetAccuracy(string id);

        Asset GetAsset(string id);
    }
}
// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Lykke.MarginTrading.Activities.Contracts.Models;

namespace Lykke.MarginTrading.Activities.Contracts.Helpers
{
    public static class DescriptionAttributesHelper
    {
        public static HashSet<int> GetQtyIndexes(ActivityCategoryContract category, ActivityTypeContract eventType)
        {
            var result = new HashSet<int>();
            if (category == ActivityCategoryContract.Position)
            {
                result.Add(1);
            }
            if (eventType == ActivityTypeContract.OrderModificationVolume)
            {
                result.Add(6);
                result.Add(7);
            }
            if (category == ActivityCategoryContract.Order)
            {
                result.Add(2);
            }
            if (category == ActivityCategoryContract.Adjustment)
            {
                result.Add(2);
            }
            return result;
        }
    }
}
// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Lykke.MarginTrading.Activities.Contracts.Models;

namespace Lykke.MarginTrading.Activities.Contracts.Helpers
{
    public static class DescriptionAttributesHelper
    {
        public static HashSet<int> GetQtyIndexes(ActivityContract activity)
        {
            return GetQtyIndexes(activity.Category, activity.Event);
        }
        
        public static HashSet<int> GetQtyIndexes(ActivityCategoryContract category, ActivityTypeContract eventType)
        {
            return GetQtyIndexes(category.ToString(), eventType.ToString());
        }
        
        public static HashSet<int> GetQtyIndexes(string category, string eventType)
        {
            var result = new HashSet<int>();
            if (category == ActivityCategoryContract.Position.ToString())
            {
                result.Add(1);
            }

            if (eventType == ActivityTypeContract.OrderModificationVolume.ToString())
            {
                result.Add(6);
                result.Add(7);
            }

            if (category == ActivityCategoryContract.Order.ToString())
            {
                result.Add(2);
            }

            if (category == ActivityCategoryContract.Adjustment.ToString())
            {
                result.Add(2);
            }

            return result;
        }
    }
}
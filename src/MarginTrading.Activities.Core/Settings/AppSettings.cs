// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.Activities.Core.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {
        public ActivitiesSettings ActivitiesProducer { get; set; }
        
        public ServiceClientSettings MarginTradingAssetServiceClient { get; set; }
    }
}

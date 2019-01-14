using JetBrains.Annotations;

namespace MarginTrading.Activities.Core.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {
        public ActivitiesSettings ActivitiesProducer { get; set; }
        
        public ServiceClientSettings MarginTradingSettingsServiceClient { get; set; }
    }
}

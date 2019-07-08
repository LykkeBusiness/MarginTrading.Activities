// Copyright (c) 2019 Lykke Corp.

using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase.Models;
using Lykke.MarginTrading.BrokerBase.Settings;

namespace MarginTrading.Activities.Broker
{
    [UsedImplicitly]
    public class Settings : BrokerSettingsBase
    {
        public Db Db { get; set; }
        public RabbitMqQueues RabbitMqQueues { get; set; }
    }
    
    [UsedImplicitly]
    public class Db
    {
        public StorageMode StorageMode { get; set; }
        public string ConnString { get; set; }
    }
    
    [UsedImplicitly]
    public class RabbitMqQueues
    {
        public RabbitMqQueueSettings Activities { get; set; }
    }
}

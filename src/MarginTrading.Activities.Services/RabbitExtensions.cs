// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.RabbitMqBroker;

using MarginTrading.Activities.Core.Extensions;
using MarginTrading.Activities.Core.Settings;

namespace MarginTrading.Activities.Services
{
    public static class RabbitExtensions
    {
        public static RabbitMqSubscriptionSettings ToInstanceSubscriptionSettings(
            this RabbitMqSettings config,
            string instanceId,
            bool isDurable)
        {
            return new RabbitMqSubscriptionSettings
            {
                ConnectionString = config.ConnectionString,
                QueueName = QueueHelper.BuildQueueName(config.ExchangeName, env: instanceId),
                ExchangeName = config.ExchangeName,
                IsDurable = isDurable,
            };
        }
    }
}
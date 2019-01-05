using System.Collections.Generic;
using Autofac;
using Common;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using MarginTrading.Activities.Core.Settings;

namespace MarginTrading.Activities.Services.Modules
{
    public class ServicesModule : Module
    {
        public ServicesModule(ActivitiesSettings settings)
        {
            
        }

        protected override void Load(ContainerBuilder builder)
        {
            
        }
        
//        private void RegisterActivitiesPublisher(ContainerBuilder builder)
//        {
//            var publishers = new List<string>
//            {
//                _settings.RabbitMqQueues.OrderHistory.ExchangeName,
//                _settings.RabbitMqQueues.OrderbookPrices.ExchangeName,
//                _settings.RabbitMqQueues.AccountChanged.ExchangeName,
//                _settings.RabbitMqQueues.AccountMarginEvents.ExchangeName,
//                _settings.RabbitMqQueues.AccountStats.ExchangeName,
//                _settings.RabbitMqQueues.Trades.ExchangeName,
//                _settings.RabbitMqQueues.PositionHistory.ExchangeName,
//                _settings.RabbitMqQueues.ExternalOrder.ExchangeName,
//            };
//
//            var bytesSerializer = new BytesStringSerializer();
//
//            foreach (var exchangeName in publishers)
//            {
//                var pub = new RabbitMqPublisher<string>(new RabbitMqSubscriptionSettings
//                    {
//                        ConnectionString = _settings.MtRabbitMqConnString,
//                        ExchangeName = exchangeName
//                    })
//                    .SetSerializer(bytesSerializer)
//                    .SetPublishStrategy(new DefaultFanoutPublishStrategy(new RabbitMqSubscriptionSettings {IsDurable = true}))
//                    .DisableInMemoryQueuePersistence()
//                    .SetLogger(_log)
//                    .SetConsole(consoleWriter)
//                    .Start();
//
//                builder.RegisterInstance(pub)
//                    .Named<IMessageProducer<string>>(exchangeName)
//                    .As<IStopable>()
//                    .SingleInstance();
//            }
//        }
    }
}
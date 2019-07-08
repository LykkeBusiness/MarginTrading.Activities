// Copyright (c) 2019 Lykke Corp.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using MarginTrading.Activities.Core;
using MarginTrading.Activities.Core.Extensions;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services
{
    public class RabbitMqSubscriberService : IRabbitMqSubscriberService, IDisposable
    {
        private readonly ILog _logger;
        private readonly string _env;

        private readonly ConcurrentDictionary<(RabbitMqSubscriptionSettings, int), IStopable> _subscribers =
            new ConcurrentDictionary<(RabbitMqSubscriptionSettings, int), IStopable>(new SubscriptionSettingsWithNumberEqualityComparer());

        public RabbitMqSubscriberService(ILog logger, ActivitiesSettings settings)
        {
            _logger = logger;
            _env = settings.Cqrs.EnvironmentName;
        }

        public void Dispose()
        {
            foreach (var stoppable in _subscribers.Values)
                stoppable.Stop();
        }

        public IMessageDeserializer<TMessage> GetJsonDeserializer<TMessage>()
        {
            return new DeserializerWithErrorLogging<TMessage>(_logger);
        }

        public IMessageDeserializer<TMessage> GetMsgPackDeserializer<TMessage>()
        {
            return new MessagePackMessageDeserializer<TMessage>();
        }

        public void Subscribe<TMessage>(RabbitMqSettings settings, bool isDurable,
            Func<TMessage, Task> handler, IMessageDeserializer<TMessage> deserializer)
        {
            var consumerCount = settings.ConsumerCount <= 0 ? 1 : settings.ConsumerCount;
            
            foreach (var consumerNumber in Enumerable.Range(1, consumerCount))
            {
                var subscriptionSettings = new RabbitMqSubscriptionSettings
                {
                    ConnectionString = settings.ConnectionString,
                    QueueName = QueueHelper.BuildQueueName(settings.ExchangeName, _env),
                    ExchangeName = settings.ExchangeName,
                    IsDurable = isDurable,
                };
                
                var rabbitMqSubscriber = new RabbitMqSubscriber<TMessage>(subscriptionSettings,
                        new DefaultErrorHandlingStrategy(_logger, subscriptionSettings))
                    .SetMessageDeserializer(deserializer)
                    .Subscribe(handler)
                    .SetLogger(_logger);

                if (!_subscribers.TryAdd((subscriptionSettings, consumerNumber), rabbitMqSubscriber))
                {
                    throw new InvalidOperationException(
                        $"{consumerNumber} subscribes for queue {subscriptionSettings.QueueName} were already initialized");
                }

                rabbitMqSubscriber.Start();
            }
        }
        
        /// <remarks>
        ///     ReSharper auto-generated
        /// </remarks>
        private sealed class SubscriptionSettingsWithNumberEqualityComparer : IEqualityComparer<(RabbitMqSubscriptionSettings, int)>
        {
            public bool Equals((RabbitMqSubscriptionSettings, int) x, (RabbitMqSubscriptionSettings, int) y)
            {
                if (ReferenceEquals(x.Item1, y.Item1) && x.Item2 == y.Item2) return true;
                if (ReferenceEquals(x.Item1, null)) return false;
                if (ReferenceEquals(y.Item1, null)) return false;
                if (x.Item1.GetType() != y.Item1.GetType()) return false;
                return string.Equals(x.Item1.ConnectionString, y.Item1.ConnectionString)
                       && string.Equals(x.Item1.ExchangeName, y.Item1.ExchangeName)
                       && x.Item2 == y.Item2;
            }

            public int GetHashCode((RabbitMqSubscriptionSettings, int) obj)
            {
                unchecked
                {
                    return ((obj.Item1.ConnectionString != null ? obj.Item1.ConnectionString.GetHashCode() : 0) * 397) ^
                           (obj.Item1.ExchangeName != null ? obj.Item1.ExchangeName.GetHashCode() : 0);
                }
            }
        }
    }
}
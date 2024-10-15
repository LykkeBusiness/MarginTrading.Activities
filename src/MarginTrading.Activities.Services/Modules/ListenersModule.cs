// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;

using Axle.Contracts;

using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Snow.Common.Correlation.RabbitMq;

using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.MessageHandlers;
using MarginTrading.Backend.Contracts.Events;

namespace MarginTrading.Activities.Services.Modules
{
    public class ListenersModule : Module
    {
        private readonly AppSettings _settings;

        public ListenersModule(AppSettings settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddRabbitMqConnectionProvider();

            builder.AddRabbitMqListener<OrderHistoryEvent, OrdersHistoryHandler>(
                _settings.ActivitiesProducer.Consumers.Orders.ToInstanceSubscriptionSettings(
                    _settings.ActivitiesProducer.Cqrs.EnvironmentName,
                    true),
                ConfigureCorrelationIdReader)
                .AddOptions(RabbitMqListenerOptions<OrderHistoryEvent>.Json.NoLoss);

            builder.AddRabbitMqListener<PositionHistoryEvent, PositionsHistoryHandler>(
                _settings.ActivitiesProducer.Consumers.Positions.ToInstanceSubscriptionSettings(
                    _settings.ActivitiesProducer.Cqrs.EnvironmentName,
                    true),
                ConfigureCorrelationIdReader)
                .AddOptions(RabbitMqListenerOptions<PositionHistoryEvent>.Json.NoLoss);

            builder.AddRabbitMqListener<MarginEventMessage, MarginEventHandler>(
                _settings.ActivitiesProducer.Consumers.MarginControl.ToInstanceSubscriptionSettings(
                    _settings.ActivitiesProducer.Cqrs.EnvironmentName,
                    true),
                ConfigureCorrelationIdReader)
                .AddOptions(RabbitMqListenerOptions<MarginEventMessage>.Json.NoLoss);

            builder.AddRabbitMqListener<SessionActivity, SessionActivityHandler>(
                _settings.ActivitiesProducer.Consumers.SessionActivity.ToInstanceSubscriptionSettings(
                    _settings.ActivitiesProducer.Cqrs.EnvironmentName,
                    true),
                ConfigureCorrelationIdReader)
                .AddOptions(RabbitMqListenerOptions<SessionActivity>.MessagePack.NoLoss);
        }
        
        private static void ConfigureCorrelationIdReader<T>(RabbitMqSubscriber<T> subscriber, IComponentContext provider)
        {
            var correlationManager = provider.Resolve<RabbitMqCorrelationManager>();
            subscriber.SetReadHeadersAction(correlationManager.FetchCorrelationIfExists);
        }
    }
}
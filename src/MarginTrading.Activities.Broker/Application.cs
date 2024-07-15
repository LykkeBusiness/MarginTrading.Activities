// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common;
using Lykke.MarginTrading.Activities.Contracts.Models;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Messaging;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.Snow.Common.Correlation;
using Lykke.Snow.Common.Correlation.RabbitMq;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Extensions;
using MarginTrading.Activities.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace MarginTrading.Activities.Broker
{
    public class Application : BrokerApplicationBase<ActivityEvent>
    {
        private readonly IActivitiesRepository _activitiesRepository;
        private readonly ILogger _logger;
        private readonly Settings _settings;
        private readonly CorrelationContextAccessor _correlationContextAccessor;

        public Application(
            IActivitiesRepository activitiesRepository,
            ILogger<Application> logger,
            Settings settings, 
            CurrentApplicationInfo applicationInfo,
            ILoggerFactory loggerFactory,
            RabbitMqCorrelationManager correlationManager,
            CorrelationContextAccessor correlationContextAccessor,
            IMessagingComponentFactory<ActivityEvent> messagingComponentFactory) 
        : base(correlationManager, loggerFactory, applicationInfo, messagingComponentFactory)
        {
            _activitiesRepository = activitiesRepository;
            _logger = logger;
            _settings = settings;
            _correlationContextAccessor = correlationContextAccessor;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.Activities.ExchangeName;
        public override string RoutingKey => nameof(ActivityEvent);
        
        protected override async Task HandleMessage(ActivityEvent e)
        {
            var correlationId = _correlationContextAccessor.CorrelationContext?.CorrelationId;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                _logger.LogDebug("Correlation id is empty for activity {ActivityId}", e.Id);
            }
            
            var contract = e.Activity;
            if (string.IsNullOrEmpty(contract.Id))
            {
                throw new ArgumentException($"Id is empty for {e.ToJson()}", nameof(contract.Id));
            }
            
            var activity = new Activity(contract.Id, contract.AccountId, contract.Instrument, contract.EventSourceId,
                contract.Timestamp, contract.Event.ToType<ActivityType>(), contract.DescriptionAttributes, contract.RelatedIds, correlationId, contract.IsOnBehalf);

            try
            {
                await _activitiesRepository.InsertIfNotExist(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing activity event: {Json}", activity.ToJson());
                throw;
            }
        }
    }
}
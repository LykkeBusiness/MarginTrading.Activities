// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.MarginTrading.Activities.Contracts.Models;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Models;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SlackNotifications;
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
        private readonly ILog _log;
        private readonly Settings _settings;
        private readonly RabbitMqCorrelationManager _correlationManager;
        private readonly CorrelationContextAccessor _correlationContextAccessor;

        public Application(
            IActivitiesRepository activitiesRepository,
            ILog logger,
            Settings settings, 
            CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender,
            ILoggerFactory loggerFactory,
            RabbitMqCorrelationManager correlationManager,
            CorrelationContextAccessor correlationContextAccessor) 
        : base(loggerFactory, logger, slackNotificationsSender, applicationInfo, MessageFormat.MessagePack)
        {
            _activitiesRepository = activitiesRepository;
            _log = logger;
            _settings = settings;
            _correlationManager = correlationManager;
            _correlationContextAccessor = correlationContextAccessor;
        }
        
        protected override Action<IDictionary<string, object>> ReadHeadersAction =>
            _correlationManager.FetchCorrelationIfExists;

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.Activities.ExchangeName;
        public override string RoutingKey => nameof(ActivityEvent);
        
        protected override async Task HandleMessage(ActivityEvent e)
        {
            var correlationId = _correlationContextAccessor.CorrelationContext?.CorrelationId;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                await _log.WriteMonitorAsync(
                    nameof(HandleMessage), 
                    nameof(ActivityEvent),
                    $"Correlation id is empty for activity {e.Id}");
            }
            
            var contract = e.Activity;
            if (string.IsNullOrEmpty(contract.Id))
            {
                throw new ArgumentException($"Id is empty for {e.ToJson()}", nameof(contract.Id));
            }
            
            var activity = new Activity(contract.Id, contract.AccountId, contract.Instrument, contract.EventSourceId,
                contract.Timestamp, contract.Event.ToType<ActivityType>(), contract.DescriptionAttributes, contract.RelatedIds, correlationId);

            try
            {
                await _activitiesRepository.InsertIfNotExist(activity);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(Broker), nameof(HandleMessage),
                    activity.ToJson(), ex);
                throw;
            }
        }
    }
}
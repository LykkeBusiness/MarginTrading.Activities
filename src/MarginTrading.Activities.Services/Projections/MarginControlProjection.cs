// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.Backend.Contracts.Positions;
using MarginTrading.Backend.Contracts.TradeMonitoring;
using OrderStatusContract = MarginTrading.Backend.Contracts.Orders.OrderStatusContract;

namespace MarginTrading.Activities.Services.Projections
{
    public class MarginControlProjection : IStartable
    {
        private readonly IRabbitMqSubscriberService _rabbitMqSubscriberService;
        private readonly ActivitiesSettings _settings;
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly ILog _log;
        
        public MarginControlProjection(IRabbitMqSubscriberService rabbitMqSubscriberService,
            ActivitiesSettings settings,
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            ILog log)
        {
            _rabbitMqSubscriberService = rabbitMqSubscriberService;
            _settings = settings;
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
            _log = log;
        }
        
        public void Start()
        {
            _rabbitMqSubscriberService.Subscribe(_settings.Consumers.MarginControl,
                true,
                HandleMarginControlEvent,
                _rabbitMqSubscriberService.GetJsonDeserializer<MarginEventMessage>());
        }

        private Task HandleMarginControlEvent(MarginEventMessage historyEvent)
        {
            var activityType = ActivityType.None;
            var descriptionAttributes = new string[0];
            var relatedIds = new string[0];
            
            switch (historyEvent.EventType)
            {
                case MarginEventTypeContract.MarginCall1:

                    activityType = ActivityType.MarginCall1;
                    
                    break;
                
                case MarginEventTypeContract.MarginCall2:

                    activityType = ActivityType.MarginCall2;
                    
                    break;
                
                case MarginEventTypeContract.Stopout:

                    activityType = ActivityType.Liquidation;
                    
                    break;
                
                default:
                    return Task.CompletedTask;
            }

            var activity = new Activity(
                _identityGenerator.GenerateId(),
                historyEvent.AccountId,
                string.Empty,
                historyEvent.EventId,
                historyEvent.EventTime,
                activityType,
                descriptionAttributes,
                relatedIds
            );

            _cqrsSender.PublishActivity(activity);
            
            return Task.CompletedTask;
        }
    }
}
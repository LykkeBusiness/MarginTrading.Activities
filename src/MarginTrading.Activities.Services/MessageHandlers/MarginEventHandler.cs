// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Lykke.RabbitMqBroker.Subscriber;

using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Events;

namespace MarginTrading.Activities.Services.MessageHandlers
{
    [UsedImplicitly]
    internal sealed class MarginEventHandler : IMessageHandler<MarginEventMessage>
    {
        private readonly IIdentityGenerator _identityGenerator;
        private readonly IActivitiesSender _cqrsSender;

        public MarginEventHandler(IIdentityGenerator identityGenerator, IActivitiesSender cqrsSender)
        {
            _identityGenerator = identityGenerator;
            _cqrsSender = cqrsSender;
        }

        public Task Handle(MarginEventMessage message)
        {
            ActivityType activityType;
            var descriptionAttributes = Array.Empty<string>();
            var relatedIds = Array.Empty<string>();
            
            switch (message.EventType)
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
                message.AccountId,
                string.Empty,
                message.EventId,
                message.EventTime,
                activityType,
                descriptionAttributes,
                relatedIds
            );

            _cqrsSender.PublishActivity(activity);
            
            return Task.CompletedTask;
        }
    }
}
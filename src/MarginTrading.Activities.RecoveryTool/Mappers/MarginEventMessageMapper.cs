// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.RecoveryTool.Model;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Events;
using Newtonsoft.Json;

namespace MarginTrading.Activities.RecoveryTool.Mappers
{
    public class MarginEventMessageMapper : IActivityMapper
    {
        private readonly IIdentityGenerator _identityGenerator;

        public MarginEventMessageMapper(IIdentityGenerator identityGenerator)
        {
            _identityGenerator = identityGenerator;
        }

        public Task<List<IActivity>> Map(DomainEvent domainEvent)
        {
            if (string.IsNullOrEmpty(domainEvent?.Json))
                return Task.FromResult(new List<IActivity>());

            var historyEvent = JsonConvert.DeserializeObject<MarginEventMessage>(domainEvent.Json);
            if (historyEvent == null)
                return Task.FromResult(new List<IActivity>());

            ActivityType activityType;
            var descriptionAttributes = Array.Empty<string>();
            var relatedIds = Array.Empty<string>();

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
                    return Task.FromResult(new List<IActivity>());
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

            return Task.FromResult(new List<IActivity> {activity});
        }
    }
}
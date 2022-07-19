// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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

        public async Task<List<IActivity>> Map(DomainEvent domainEvent)
        {
            var historyEvent = JsonConvert.DeserializeObject<MarginEventMessage>(domainEvent.Json);

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
                    return new List<IActivity>();
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

            return new List<IActivity>() {activity};
        }
    }
}
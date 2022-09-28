// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.RecoveryTool.Model;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Workflow.Liquidation;
using MarginTrading.Backend.Contracts.Workflow.Liquidation.Events;
using Newtonsoft.Json;

namespace MarginTrading.Activities.RecoveryTool.Mappers
{
    public class LiquidationResumedEventMapper : IActivityMapper
    {
        private readonly IIdentityGenerator _identityGenerator;

        public LiquidationResumedEventMapper(IIdentityGenerator identityGenerator)
        {
            _identityGenerator = identityGenerator;
        }

        public Task<List<IActivity>> Map(DomainEvent domainEvent)
        {
            if (string.IsNullOrEmpty(domainEvent?.Json))
                return Task.FromResult(new List<IActivity>());
            
            var @event = JsonConvert.DeserializeObject<LiquidationResumedEvent>(domainEvent.Json);

            if (@event == null ||  @event.LiquidationType != LiquidationTypeContract.Forced)
            {
                return Task.FromResult(new List<IActivity>());
            }

            var activityId = _identityGenerator.GenerateId();
            var activity = new Activity(id: activityId,
                accountId: @event.AccountId,
                instrument: @event.AssetPairId,
                eventSourceId: @event.OperationId,
                @event.CreationTime,
                @event: ActivityType.CloseAllRestarted,
                descriptionAttributes: Array.Empty<string>(),
                relatedIds: Array.Empty<string>());

            return Task.FromResult(new List<IActivity>{activity});
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.RecoveryTool.Model;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Workflow.Liquidation;
using MarginTrading.Backend.Contracts.Workflow.Liquidation.Events;
using Newtonsoft.Json;

namespace MarginTrading.Activities.RecoveryTool.Mappers
{
    public class LiquidationFinishedEventMapper : IActivityMapper
    {
        private readonly IIdentityGenerator _identityGenerator;

        public LiquidationFinishedEventMapper(IIdentityGenerator identityGenerator)
        {
            _identityGenerator = identityGenerator;
        }

        [ItemNotNull]
        public Task<List<IActivity>> Map(DomainEvent domainEvent)
        {
            if (string.IsNullOrWhiteSpace(domainEvent?.Json))
                return Task.FromResult(new List<IActivity>());
            
            var @event = JsonConvert.DeserializeObject<LiquidationFinishedEvent>(domainEvent.Json);

            if (@event == null || @event.LiquidationType != LiquidationTypeContract.Forced)
            {
                return Task.FromResult(new List<IActivity>());
            }

            var activityId = _identityGenerator.GenerateId();
            var activity = new Activity(id: activityId,
                accountId: @event.AccountId,
                instrument: @event.AssetPairId,
                eventSourceId: @event.OperationId,
                @event.CreationTime,
                @event: ActivityType.CloseAllFinished,
                descriptionAttributes: Array.Empty<string>(),
                relatedIds: Array.Empty<string>());

            return Task.FromResult(new List<IActivity> {activity});
        }
    }
}
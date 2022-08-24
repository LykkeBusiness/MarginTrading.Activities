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
    public class LiquidationFailedEventMapper : IActivityMapper
    {
        private readonly IIdentityGenerator _identityGenerator;
        private readonly IAccountsService _accountsService;

        public LiquidationFailedEventMapper(IIdentityGenerator identityGenerator, IAccountsService accountsService)
        {
            _identityGenerator = identityGenerator;
            _accountsService = accountsService;
        }

        public async Task<List<IActivity>> Map(DomainEvent domainEvent)
        {
            var @event = JsonConvert.DeserializeObject<LiquidationFailedEvent>(domainEvent.Json);

            if (@event.LiquidationType != LiquidationTypeContract.Forced)
            {
                return new List<IActivity>();
            }

            var activityId = _identityGenerator.GenerateId();
            var activity = new Activity(id: activityId,
                accountId: @event.AccountId,
                instrument: @event.AssetPairId,
                eventSourceId: @event.OperationId,
                @event.CreationTime,
                @event: ActivityType.CloseAllFailed,
                descriptionAttributes: Array.Empty<string>(),
                relatedIds: Array.Empty<string>());

            return new List<IActivity>() {activity};
        }
    }
}
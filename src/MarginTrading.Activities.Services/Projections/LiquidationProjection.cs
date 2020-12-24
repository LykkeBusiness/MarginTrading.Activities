using System;
using System.Threading.Tasks;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Workflow.Liquidation;
using MarginTrading.Backend.Contracts.Workflow.Liquidation.Events;

namespace MarginTrading.Activities.Services.Projections
{
    public class LiquidationProjection
    {
        private readonly IActivitiesSender _publisher;
        private readonly IIdentityGenerator _identityGenerator;

        public LiquidationProjection(IActivitiesSender publisher, IIdentityGenerator identityGenerator)
        {
            _publisher = publisher;
            _identityGenerator = identityGenerator;
        }
        
        public Task Handle(LiquidationStartedEvent @event)
        {
            if (@event.LiquidationType != LiquidationTypeContract.Forced)
            {
                return Task.CompletedTask;
            }

            var activityId = _identityGenerator.GenerateId();
            var activity = new Activity(id: activityId,
                accountId: @event.AccountId,
                instrument: @event.AssetPairId,
                eventSourceId: @event.OperationId,
                @event.CreationTime,
                @event: ActivityType.CloseAllStarted,
                descriptionAttributes: Array.Empty<string>(),
                relatedIds: Array.Empty<string>());

            _publisher.PublishActivity(activity);

            return Task.CompletedTask;
        }

        public Task Handle(LiquidationResumedEvent @event)
        {
            if (@event.LiquidationType != LiquidationTypeContract.Forced)
            {
                return Task.CompletedTask;
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

            _publisher.PublishActivity(activity);

            return Task.CompletedTask;
        }

        public Task Handle(LiquidationFailedEvent @event)
        {
            if (@event.LiquidationType != LiquidationTypeContract.Forced)
            {
                return Task.CompletedTask;
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

            _publisher.PublishActivity(activity);

            return Task.CompletedTask;
        }        
        
        public Task Handle(LiquidationFinishedEvent @event)
        {
            if (@event.LiquidationType != LiquidationTypeContract.Forced)
            {
                return Task.CompletedTask;
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

            _publisher.PublishActivity(activity);

            return Task.CompletedTask;
        }   
    }
}

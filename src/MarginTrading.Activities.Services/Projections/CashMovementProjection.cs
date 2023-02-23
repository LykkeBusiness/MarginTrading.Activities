using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services.Projections
{
    public class CashMovementProjection
    {
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private ILog _log;

        public CashMovementProjection(
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            ILog log)
        {
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
            _log = log;
        }

        [UsedImplicitly]
        public Task Handle(DepositSucceededEvent e)
        {
            var activity = new Activity(
                    id: _identityGenerator.GenerateId(),
                    accountId: e.AccountId,
                    instrument: string.Empty,
                    eventSourceId: e.AccountId, 
                    timestamp: e.EventTimestamp,
                    @event: ActivityType.AccountDepositSucceeded,
                    descriptionAttributes: GetDescriptionAtributes(e), 
                    relatedIds: Array.Empty<string>());
            
            _cqrsSender.PublishActivity(activity);

            return Task.CompletedTask;
        } 
        
        [UsedImplicitly]
        public Task Handle(DepositFailedEvent e)
        {
            
            //TODO Remove
            _log.WriteInfoAsync(nameof(CashMovementProjection), "DepositFailedEvent", e.ToJson());
            
            // TODO get operation details here
            // and set accountId and eventSourceID
            var activity = new Activity(
                    id: _identityGenerator.GenerateId(),
                    accountId: string.Empty, 
                    instrument: string.Empty,
                    eventSourceId: string.Empty,
                    timestamp: e.EventTimestamp,
                    @event: ActivityType.AccountDepositFailed,
                    descriptionAttributes: GetDescriptionAtributes(e),
                    relatedIds: Array.Empty<string>());
            
            _cqrsSender.PublishActivity(activity);

            return Task.CompletedTask;
        }

        [UsedImplicitly]
        public Task Handle(WithdrawalSucceededEvent e)
        {
            var activity = new Activity(
                    id: _identityGenerator.GenerateId(),
                    accountId: e.AccountId,
                    instrument: string.Empty,
                    eventSourceId: e.AccountId, 
                    timestamp: e.EventTimestamp,
                    @event: ActivityType.AccountWithdrawalSucceeded,
                    descriptionAttributes: GetDescriptionAtributes(e),
                    relatedIds: Array.Empty<string>());
            
            _cqrsSender.PublishActivity(activity);

            return Task.CompletedTask;
        }

        [UsedImplicitly]
        public Task Handle(WithdrawalFailedEvent e)
        {
            var activity = new Activity(
                    id: _identityGenerator.GenerateId(),
                    accountId: e.AccountId,
                    instrument: string.Empty,
                    eventSourceId: e.AccountId, 
                    timestamp: e.EventTimestamp,
                    @event: ActivityType.AccountWithdrawalFailed,
                    descriptionAttributes: GetDescriptionAtributes(e),
                    relatedIds: Array.Empty<string>());
            
            _cqrsSender.PublishActivity(activity);

            return Task.CompletedTask;
        }
        
        private string[] GetDescriptionAtributes(BaseEvent @event)
        {
            switch(@event)
            {
                case DepositSucceededEvent e:
                    return new string[] { e.Amount.ToString() };

                case DepositFailedEvent e: //TODO: Get the amount somehow
                    return new string[] {  };

                case WithdrawalSucceededEvent e:
                    return new string[] { e.Amount.ToString() };

                case WithdrawalFailedEvent e:
                    return new string[] { e.Amount.ToString() };
                default:
                    return Array.Empty<string>();
            }
        }
        
    }
}
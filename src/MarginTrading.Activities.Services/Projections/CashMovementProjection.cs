using System;
using System.Threading.Tasks;
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
        private readonly IAccountsService _accountService;

        public CashMovementProjection(
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            IAccountsService accountService)
        {
            _cqrsSender = cqrsSender;
            _accountService = accountService;
        }

        [UsedImplicitly]
        public async Task Handle(DepositSucceededEvent e)
        {
            var activity = new Activity(
                    id: _identityGenerator.GenerateId(),
                    accountId: e.AccountId,
                    instrument: string.Empty,
                    eventSourceId: e.AccountId, 
                    timestamp: e.EventTimestamp,
                    @event: ActivityType.AccountDepositSucceeded,
                    descriptionAttributes: await GetDescriptionAtributes(e, e.AccountId), 
                    relatedIds: Array.Empty<string>());
            
            _cqrsSender.PublishActivity(activity);
        } 
        
        [UsedImplicitly]
        public async Task Handle(DepositFailedEvent e)
        {
            var activity = new Activity(
                    id: _identityGenerator.GenerateId(),
                    accountId: e.AccountId, 
                    instrument: string.Empty,
                    eventSourceId: e.AccountId,
                    timestamp: e.EventTimestamp,
                    @event: ActivityType.AccountDepositFailed,
                    descriptionAttributes: await GetDescriptionAtributes(e, e.AccountId),
                    relatedIds: Array.Empty<string>());
            
            _cqrsSender.PublishActivity(activity);
        }

        [UsedImplicitly]
        public async Task Handle(WithdrawalSucceededEvent e)
        {
            var activity = new Activity(
                    id: _identityGenerator.GenerateId(),
                    accountId: e.AccountId,
                    instrument: string.Empty,
                    eventSourceId: e.AccountId, 
                    timestamp: e.EventTimestamp,
                    @event: ActivityType.AccountWithdrawalSucceeded,
                    descriptionAttributes: await GetDescriptionAtributes(e, e.AccountId),
                    relatedIds: Array.Empty<string>());
            
            _cqrsSender.PublishActivity(activity);
        }

        [UsedImplicitly]
        public async Task Handle(WithdrawalFailedEvent e)
        {
            var activity = new Activity(
                    id: _identityGenerator.GenerateId(),
                    accountId: e.AccountId,
                    instrument: string.Empty,
                    eventSourceId: e.AccountId, 
                    timestamp: e.EventTimestamp,
                    @event: ActivityType.AccountWithdrawalFailed,
                    descriptionAttributes: await GetDescriptionAtributes(e, e.AccountId),
                    relatedIds: Array.Empty<string>());
            
            _cqrsSender.PublishActivity(activity);
        }
        
        private async Task<string[]> GetDescriptionAtributes(BaseEvent @event, string accountId)
        {
            switch(@event)
            {
                case DepositSucceededEvent e:
                    return new string[] { e.Amount.ToString(), e.Currency, await _accountService.GetEitherAccountNameOrAccountId(accountId) };

                case DepositFailedEvent e: 
                    return new string[] { e.Amount.ToString(), e.Currency, await _accountService.GetEitherAccountNameOrAccountId(accountId) };

                case WithdrawalSucceededEvent e:
                    return new string[] { e.Amount.ToString(), e.Currency, await _accountService.GetEitherAccountNameOrAccountId(accountId) };

                case WithdrawalFailedEvent e:
                    return new string[] { e.Amount.ToString(), e.Currency, await _accountService.GetEitherAccountNameOrAccountId(accountId) };

                default:
                    return Array.Empty<string>();
            }
        }
        
    }
}
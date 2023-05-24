// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Contracts.Models.AdditionalInfo;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services.Projections
{
    public class AccountsProjection
    {
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly IAccountsService _accountService;

        public AccountsProjection(
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            IAccountsService accountService)
        {
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
            _accountService = accountService;
        }
        [UsedImplicitly]
        public async Task Handle(AccountChangedEvent e)
        {
            var activityTypes = GetActivityTypes(e);

            foreach (var activityType in activityTypes)
            {
                var activity = new Activity(
                    _identityGenerator.GenerateId(),
                    e.Account.Id,
                    string.Empty,
                    e.Account.Id,
                    e.ChangeTimestamp,
                    activityType,
                    await GetDescriptionAttributes(activityType, e),
                    new string[0]);

                _cqrsSender.PublishActivity(activity);
            }
        }

        private async Task<string[]> GetDescriptionAttributes(ActivityType activityType, AccountChangedEvent e)
        {
            switch (activityType)
            {
                case ActivityType.AccountComplexityWarningEnabled:
                    return new[] { await _accountService.GetEitherAccountNameOrAccountId(e.Account.Id) };

                case ActivityType.AccountComplexityWarningDisabled:
                    return new[] { await _accountService.GetEitherAccountNameOrAccountId(e.Account.Id), e.ActivitiesMetadata?.DeserializeJson<AccountChangeMetadata>()?.OrderId };

                case ActivityType.AccountTradingDisabled:
                    return new[] { await _accountService.GetEitherAccountNameOrAccountId(e.Account.Id) };

                case ActivityType.AccountTradingEnabled:
                    return new[] { await _accountService.GetEitherAccountNameOrAccountId(e.Account.Id) };

                case ActivityType.AccountWithdrawalDisabled:
                    return new[] { await _accountService.GetEitherAccountNameOrAccountId(e.Account.Id) };

                case ActivityType.AccountWithdrawalEnabled:
                    return new[] { await _accountService.GetEitherAccountNameOrAccountId(e.Account.Id) };
                
                default: return new string[0];
            }
        }

        private List<ActivityType> GetActivityTypes(AccountChangedEvent e)
        {
            var activities = new List<ActivityType>();
            
            switch (e.EventType)
            {
                case AccountChangedEventTypeContract.Created:
                    activities.Add(ActivityType.AccountCreation);
                    break;
                
                case AccountChangedEventTypeContract.Updated:

                    var currentAccountState = e.Account;
                    var previousAccountState = e.ActivitiesMetadata.DeserializeJson<AccountChangeMetadata>()
                        .PreviousAccountSnapshot;

                    if (currentAccountState == null || previousAccountState == null)
                        break;
                    
                    var shouldShowProductComplexityWarning = (e.Account.AdditionalInfo?.DeserializeJson<AccountAdditionalInfo>().ShouldShowProductComplexityWarning).GetValueOrDefault();
                    var previousShouldShowProductComplexityWarning = (previousAccountState.AdditionalInfo?.DeserializeJson<AccountAdditionalInfo>().ShouldShowProductComplexityWarning).GetValueOrDefault();

                    if(!previousAccountState.IsDisabled && currentAccountState.IsDisabled)
                        activities.Add(ActivityType.AccountTradingDisabled);
                    
                    if(previousAccountState.IsDisabled && !currentAccountState.IsDisabled)
                        activities.Add(ActivityType.AccountTradingEnabled);
                    
                    if(!previousAccountState.IsWithdrawalDisabled && currentAccountState.IsWithdrawalDisabled)
                        activities.Add(ActivityType.AccountWithdrawalDisabled);
                    
                    if(previousAccountState.IsWithdrawalDisabled && !currentAccountState.IsWithdrawalDisabled)
                        activities.Add(ActivityType.AccountWithdrawalEnabled);

                    if (!previousShouldShowProductComplexityWarning && shouldShowProductComplexityWarning)
                    {
                        activities.Add(ActivityType.AccountComplexityWarningEnabled);
                    }
                    
                    if (previousShouldShowProductComplexityWarning && !shouldShowProductComplexityWarning)
                    {
                        activities.Add(ActivityType.AccountComplexityWarningDisabled);
                    }
                    
                    break;
                
            }

            return activities;
        }
    }
}
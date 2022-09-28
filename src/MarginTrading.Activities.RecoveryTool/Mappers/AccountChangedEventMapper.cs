// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Contracts.Models.AdditionalInfo;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.RecoveryTool.Model;
using MarginTrading.Activities.Services.Abstractions;
using Newtonsoft.Json;

namespace MarginTrading.Activities.RecoveryTool.Mappers
{
    public class AccountChangedEventMapper : IActivityMapper
    {
        private readonly IIdentityGenerator _identityGenerator;

        public AccountChangedEventMapper(IIdentityGenerator identityGenerator)
        {
            _identityGenerator = identityGenerator;
        }
        
        public Task<List<IActivity>> Map(DomainEvent domainEvent)
        {
            var e = JsonConvert.DeserializeObject<AccountChangedEvent>(domainEvent.Json);
            var activityTypes = GetActivityTypes(e);

            var result = new List<IActivity>();

            foreach (var activityType in activityTypes)
            {
                var activity = new Activity(
                    _identityGenerator.GenerateId(),
                    e.Account.Id,
                    string.Empty,
                    e.Account.Id,
                    e.ChangeTimestamp,
                    activityType,
                    GetDescriptionAttributes(activityType, e),
                    Array.Empty<string>());

                result.Add(activity);
            }

            return Task.FromResult(result);
        }
        
        private string[] GetDescriptionAttributes(ActivityType activityType, AccountChangedEvent e)
        {
            switch (activityType)
            {
                case ActivityType.AccountComplexityWarningEnabled:
                    return new[] { e.Account.Id };
                case ActivityType.AccountComplexityWarningDisabled:
                    return new[] { e.Account.Id, e.ActivitiesMetadata?.DeserializeJson<AccountChangeMetadata>()?.OrderId };
                
                default: return Array.Empty<string>();
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
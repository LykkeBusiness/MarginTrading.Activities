// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services.Projections
{
    public class AccountsProjection
    {
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;

        public AccountsProjection(
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator)
        {
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
        }
        [UsedImplicitly]
        public Task Handle(AccountChangedEvent e)
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
                    new string[0],
                    new string[0]);

                _cqrsSender.PublishActivity(activity);
            }
            
            return Task.CompletedTask;
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
                    
                    if(!previousAccountState.IsDisabled && currentAccountState.IsDisabled)
                        activities.Add(ActivityType.AccountTradingDisabled);
                    
                    if(previousAccountState.IsDisabled && !currentAccountState.IsDisabled)
                        activities.Add(ActivityType.AccountTradingEnabled);
                    
                    if(!previousAccountState.IsWithdrawalDisabled && currentAccountState.IsWithdrawalDisabled)
                        activities.Add(ActivityType.AccountWithdrawalDisabled);
                    
                    if(previousAccountState.IsWithdrawalDisabled && !currentAccountState.IsWithdrawalDisabled)
                        activities.Add(ActivityType.AccountWithdrawalEnabled);
                    
                    break;
                
            }

            return activities;
        }
    }
}
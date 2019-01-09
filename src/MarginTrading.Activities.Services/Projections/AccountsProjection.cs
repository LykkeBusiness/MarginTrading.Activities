using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Settings;
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
            var activityType = GetActivityType(e);

            if (activityType == null)
                return Task.CompletedTask;

            var activity = new Activity(
                _identityGenerator.GenerateId(),
                e.Account.Id,
                string.Empty,
                e.Account.Id,
                e.ChangeTimestamp,
                activityType.Value,
                new string[0],
                new string[0]);

            _cqrsSender.PublishActivity(activity);
            
            return Task.CompletedTask;
        }

        private ActivityType? GetActivityType(AccountChangedEvent e)
        {
            switch (e.EventType)
            {
                case AccountChangedEventTypeContract.Created:
                    return ActivityType.AccountCreation;
                
                case AccountChangedEventTypeContract.Updated:
                    //todo: identify update type from activity info (to be added to event)
                default:
                    return null;
            } 
        }
    }
}
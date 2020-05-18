// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;
using Common;
using Common.Log;
using Lykke.Cqrs;
using Lykke.MarginTrading.Activities.Contracts.Models;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.Core.Extensions;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services
{
    public class ActivitiesSender : IActivitiesSender
    {
        private readonly IDateService _dateService;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly string _activities;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ILog _log;

        public ActivitiesSender(
            IDateService dateService,
            IIdentityGenerator identityGenerator, 
            IComponentContext componentContext,
            string activities,
            ILog log)
        {
            _log = log;
            _activities = activities ?? throw new ArgumentNullException(nameof(activities));
            _dateService = dateService;
            _identityGenerator = identityGenerator;
            _cqrsEngine = componentContext.Resolve<ICqrsEngine>();
        }
        
        public void PublishActivity(IActivity activity)
        {
            try
            {
                var id = _identityGenerator.GenerateId();
                var now = _dateService.Now();
                var activityCategory = activity.Category.ToType<ActivityCategoryContract>();
                var activityType = activity.Event.ToType<ActivityTypeContract>();
                
                var @event = new ActivityEvent
                {
                    Id = id,
                    Timestamp = now,
                    Activity = new ActivityContract(
                        activity.Id,
                        activity.AccountId,
                        activity.Instrument,
                        activity.EventSourceId,
                        activity.Timestamp,
                        activityCategory,
                        activityType,
                        activity.DescriptionAttributes,
                        activity.RelatedIds)
                };
                
                _cqrsEngine.PublishEvent(@event, _activities);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(ActivitiesSender), activity.ToJson(), ex);
            }
        }
    }
}
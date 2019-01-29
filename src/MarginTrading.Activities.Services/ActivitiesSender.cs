using System;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.MarginTrading.Activities.Contracts.Models;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.Core.Extensions;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services
{
    public class ActivitiesSender : IActivitiesSender
    {
        [NotNull] public ICqrsEngine CqrsEngine { get; set; }//property injection
        [NotNull] private readonly CqrsContextNamesSettings _cqrsContextNamesSettings;
        private readonly ILog _log;
        private readonly IDateService _dateService;
        private readonly IIdentityGenerator _identityGenerator;

        public ActivitiesSender([NotNull] CqrsContextNamesSettings cqrsContextNamesSettings, 
            ILog log,
            IDateService dateService,
            IIdentityGenerator identityGenerator)
        {
            _cqrsContextNamesSettings = cqrsContextNamesSettings ??
                                        throw new ArgumentNullException(nameof(cqrsContextNamesSettings));
            _log = log;
            _dateService = dateService;
            _identityGenerator = identityGenerator;
        }
        
        public void PublishActivity(IActivity activity)
        {
            try
            {
                var e = new ActivityEvent
                {
                    Id = _identityGenerator.GenerateId(),
                    Timestamp = _dateService.Now(),
                    Activity = new ActivityContract(
                        activity.Id,
                        activity.AccountId,
                        activity.Instrument,
                        activity.EventSourceId,
                        activity.Timestamp,
                        activity.Category.ToType<ActivityCategoryContract>(),
                        activity.Event.ToType<ActivityTypeContract>(),
                        activity.DescriptionAttributes,
                        activity.RelatedIds)
                };
                
                CqrsEngine.PublishEvent(e, _cqrsContextNamesSettings.Activities);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(ActivitiesSender), activity.ToJson(), ex);
            }
        }
    }
}
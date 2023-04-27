// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Common;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Domain.Abstractions;

namespace MarginTrading.Activities.SqlRepositories
{
    public class ActivityEntity: IActivity
    {
        public string Id { get; set; }
        
        public string AccountId { get; set; }
        
        public string Instrument { get; set; }
        
        public string EventSourceId { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public string Category { get; set; }
        
        ActivityCategory IActivity.Category => string.IsNullOrEmpty(Category)
            ? ActivityCategory.None
            : Category.ParseEnum<ActivityCategory>();
        
        public string Event { get; set; }
        
        ActivityType IActivity.Event => string.IsNullOrEmpty(Event)
            ? ActivityType.None
            : Event.ParseEnum<ActivityType>();
        
        public string DescriptionAttributes { get; set; }
        
        string[] IActivity.DescriptionAttributes => string.IsNullOrEmpty(DescriptionAttributes)
            ? new string[0]
            : DescriptionAttributes.DeserializeJson<string[]>();
        
        public string RelatedIds { get; set; }
        
        string[] IActivity.RelatedIds => string.IsNullOrEmpty(RelatedIds)
            ? new string[0]
            : RelatedIds.DeserializeJson<string[]>();
        
        public string CorrelationId { get; set; }

        public string AdditionalInfo { get; set; }

        public static ActivityEntity Create(IActivity activity)
        {
            return new ActivityEntity
            {
                Id = activity.Id,
                AccountId = activity.AccountId,
                Instrument = activity.Instrument,
                EventSourceId = activity.EventSourceId,
                Timestamp = activity.Timestamp,
                Category = activity.Category.ToString(),
                Event = activity.Event.ToString(),
                DescriptionAttributes = activity.DescriptionAttributes.ToJson(),
                RelatedIds = activity.RelatedIds.ToJson(),
                CorrelationId = activity.CorrelationId,
                AdditionalInfo = activity.AdditionalInfo
            };
        }
    }
}
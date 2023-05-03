// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.MarginTrading.Activities.Contracts.Models
{
    [MessagePackObject]
    public class ActivityContract
    {
        public ActivityContract(string id, string accountId, string instrument, string eventSourceId,
            DateTime timestamp, ActivityCategoryContract category, ActivityTypeContract @event,
            string[] descriptionAttributes, string[] relatedIds, bool isOnBehalf)
        {
            Id = id;
            AccountId = accountId;
            Instrument = instrument;
            Timestamp = timestamp;
            Event = @event;
            DescriptionAttributes = descriptionAttributes;
            RelatedIds = relatedIds;
            EventSourceId = eventSourceId;
            Category = category;
            IsOnBehalf = isOnBehalf;
        }

        [Key(0)]
        public string Id { get; }
        
        [Key(1)]
        public string AccountId { get; }
        
        [Key(2)]
        public string Instrument { get; }
        
        [Key(3)]
        public string EventSourceId { get; }
        
        [Key(4)]
        public DateTime Timestamp { get; }
        
        [Key(5)]
        public ActivityCategoryContract Category { get; }
        
        [Key(6)]
        public ActivityTypeContract Event { get; }
        
        [Key(7)]
        public string[] DescriptionAttributes { get; }
        
        [Key(8)]
        public string[] RelatedIds { get; }

        [Key(9)]
        public bool IsOnBehalf { get; }
    }
}
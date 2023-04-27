// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using MarginTrading.Activities.Core.Domain.Abstractions;

namespace MarginTrading.Activities.Core.Domain
{
    public class Activity : IActivity
    {
        public Activity(string id, string accountId, string instrument, string eventSourceId,
            DateTime timestamp, ActivityType @event, string[] descriptionAttributes, string[] relatedIds,
            string correlationId = null, string additionalInfo = null)
        {
            Id = id;
            AccountId = accountId;
            Instrument = instrument;
            Timestamp = timestamp;
            Event = @event;
            DescriptionAttributes = descriptionAttributes;
            RelatedIds = relatedIds;
            EventSourceId = eventSourceId;
            CorrelationId = correlationId;
            AdditionalInfo = additionalInfo;
        }

        public string Id { get; }
        public string AccountId { get; }
        public string Instrument { get; }
        public string EventSourceId { get; }
        public DateTime Timestamp { get; }
        
        public ActivityCategory Category => (ActivityCategory) ((int)Event / 1000);
        public ActivityType Event { get; }
        public string[] DescriptionAttributes { get; }
        public string[] RelatedIds { get; }
        public string CorrelationId { get; }
        public string AdditionalInfo { get; }
    }
}
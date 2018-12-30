using System;
using System.Collections.Generic;
using MarginTrading.Activities.Core.Domain.Abstractions;

namespace MarginTrading.Activities.Core.Domain
{
    public class Activity : IActivity
    {
        public Activity(string id, string accountId, string instrument, DateTime timestamp, ActivityType @event, string[] descriptionAttributes, List<string> ids)
        {
            Id = id;
            AccountId = accountId;
            Instrument = instrument;
            Timestamp = timestamp;
            Event = @event;
            DescriptionAttributes = descriptionAttributes;
            Ids = ids;
        }

        public string Id { get; }
        public string AccountId { get; }
        public string Instrument { get; }
        public DateTime Timestamp { get; }
        
        public ActivityCategory Category => (ActivityCategory) ((int)Event / 1000);
        public ActivityType Event { get; }
        public string[] DescriptionAttributes { get; }
        public List<string> Ids { get; }
    }
}
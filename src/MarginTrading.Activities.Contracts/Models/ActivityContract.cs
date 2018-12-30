using System;
using System.Collections.Generic;

namespace Lykke.MarginTrading.Activities.Contracts.Models
{
    public class ActivityContract
    {
        public ActivityContract(string id, string accountId, string instrument, DateTime timestamp,
            ActivityCategoryContract category,
            ActivityTypeContract @event, string[] descriptionAttributes, List<string> ids)
        {
            Id = id;
            AccountId = accountId;
            Instrument = instrument;
            Timestamp = timestamp;
            Event = @event;
            DescriptionAttributes = descriptionAttributes;
            Ids = ids;
            Category = category;
        }

        public string Id { get; }
        public string AccountId { get; }
        public string Instrument { get; }
        public DateTime Timestamp { get; }
        public ActivityCategoryContract Category { get; }
        public ActivityTypeContract Event { get; }
        public string[] DescriptionAttributes { get; }
        public List<string> Ids { get; }
    }
}
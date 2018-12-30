using System;
using System.Collections.Generic;
using Common;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Domain.Abstractions;
using Newtonsoft.Json;

namespace MarginTrading.Activities.SqlRepositories
{
    public class ActivityEntity: IActivity
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Instrument { get; set; }
        public DateTime Timestamp { get; set; }
        public ActivityCategory Category { get; set; }
        public ActivityType Event { get; set; }
        public string[] DescriptionAttributes { get; set; }
        public List<string> Ids { get; set; }
    }
}
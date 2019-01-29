using System;
using MessagePack;

namespace Lykke.MarginTrading.Activities.Contracts.Models
{
    [MessagePackObject]
    public class ActivityEvent
    {
        [Key(0)]
        public string Id { get; set; }
        
        [Key(1)]
        public DateTime Timestamp { get; set; }
        
        [Key(2)]
        public ActivityContract Activity { get; set; }
    }
}
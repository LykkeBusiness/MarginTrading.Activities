// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
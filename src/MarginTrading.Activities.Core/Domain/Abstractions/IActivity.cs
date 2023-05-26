// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace MarginTrading.Activities.Core.Domain.Abstractions
{
    public interface IActivity
    {
        string Id { get; }

        string AccountId { get; }

        string Instrument { get; }
        
        string EventSourceId { get; }

        DateTime Timestamp { get; }

        ActivityCategory Category { get; }

        ActivityType Event { get; }

        string[] DescriptionAttributes { get; }

        string[] RelatedIds { get; }

        string CorrelationId { get; }

        bool? IsOnBehalf { get; }
    }
}
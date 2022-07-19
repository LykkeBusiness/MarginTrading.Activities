// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MarginTrading.Activities.RecoveryTool.Model;

namespace MarginTrading.Activities.RecoveryTool.LogParsers
{
    public class ActivityProducerLogParser
    {
        public List<DomainEvent> Parse(string log)
        {
            var result = new List<DomainEvent>();
            
            var accounts = Create("AccountsProjection", "AccountChangedEvent");
            result.AddRange(Parse(log, accounts, EventType.AccountChangedEvent));

            var liquidation = CreateLiquidationRegex("LiquidationProjection");
            result.AddRange(ParseLiquidation(log, liquidation));

            var orderRejected = Create("OrderPlacementRejectedProjection", "OrderPlacementRejectedEvent");
            result.AddRange(Parse(log, orderRejected, EventType.OrderPlacementRejectedEvent));

            return result;
        }

        private IEnumerable<DomainEvent> Parse(string log, Regex regex, EventType type)
        {
            return regex.Matches(log)
                .Select(x => ExtractJson(x.Value))
                .Select(x => new DomainEvent(x, type));
        }
        
        private IEnumerable<DomainEvent> ParseLiquidation(string log, Regex regex)
        {
            return regex.Matches(log)
                .Select(x =>
                {
                    var json = ExtractJson(x.Value);
                    var type = GetLiquidationType(x.Value);
                    return new DomainEvent(json, type);
                });
        }

        private EventType GetLiquidationType(string value)
        {
            if (value.Contains("LiquidationStartedEvent")) return EventType.LiquidationStartedEvent;
            if (value.Contains("LiquidationResumedEvent")) return EventType.LiquidationResumedEvent;
            if (value.Contains("LiquidationFailedEvent")) return EventType.LiquidationFailedEvent;
            if (value.Contains("LiquidationFinishedEvent")) return EventType.LiquidationFinishedEvent;
            return EventType.None;
        }

        private string ExtractJson(string val)
        {
            var start = val.IndexOf('{');
            var end = val.LastIndexOf('}');
            var json = val.Substring(start, end - start + 1);

            return json;
        }

        private Regex Create(string start, string end)
        {
            var str = $"({start})(.*?)({end})";
            return new Regex(str, RegexOptions.Singleline);
        }
        
        private Regex CreateLiquidationRegex(string start)
        {
            var str = $"({start})(.*?)(LiquidationStartedEvent|LiquidationResumedEvent|LiquidationFailedEvent|LiquidationFinishedEvent)";
            return new Regex(str, RegexOptions.Singleline);
        }
    }
}
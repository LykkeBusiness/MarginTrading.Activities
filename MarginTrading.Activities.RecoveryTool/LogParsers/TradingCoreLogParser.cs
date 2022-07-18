// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MarginTrading.Activities.RecoveryTool.Model;

namespace MarginTrading.Activities.RecoveryTool.LogParsers
{
    public class TradingCoreLogParser
    {
        public List<DomainEvent> Parse(string log)
        {
            var result = new List<DomainEvent>();
            
            var margin = Create("RabbitMqNotifyService:lykke.mt.account.marginevents:", "Published RabbitMqEvent");
            result.AddRange(Parse(log, margin, EventType.MarginEventMessage));
            
            var order = Create("RabbitMqNotifyService:lykke.mt.orderhistory:", "Published RabbitMqEvent");
            result.AddRange(Parse(log, order, EventType.OrderHistoryEvent));
            
            var position = Create("RabbitMqNotifyService:lykke.mt.position.history:", "Published RabbitMqEvent");
            result.AddRange(Parse(log, position, EventType.PositionHistoryEvent));

            return result;
        }
        
        private IEnumerable<DomainEvent> Parse(string log, Regex regex, EventType type)
        {
            return regex.Matches(log)
                .Select(x => ExtractJson(x.Value))
                .Select(x => new DomainEvent(x, type));
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
    }
}
// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.Activities.Core.Settings
{
    [UsedImplicitly]
    public class ConsumersSettings
    {
        public RabbitMqSettings Orders { get; set; }
        
        public RabbitMqSettings Positions { get; set; }
        
        public RabbitMqSettings MarginControl { get; set; }
        
        public RabbitMqSettings SessionActivity { get; set; }
    }
}
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
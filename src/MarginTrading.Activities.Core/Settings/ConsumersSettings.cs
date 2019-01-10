namespace MarginTrading.Activities.Core.Settings
{
    public class ConsumersSettings
    {
        public RabbitMqSettings Orders { get; set; }
        
        public RabbitMqSettings Positions { get; set; }
        
        public RabbitMqSettings MarginControl { get; set; }
    }
}
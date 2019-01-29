using Lykke.SettingsReader.Attributes;

namespace MarginTrading.Activities.Core.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        
        public string ExchangeName { get; set; }

        [Optional] 
        public int ConsumerCount { get; set; } = 1;
    }
}
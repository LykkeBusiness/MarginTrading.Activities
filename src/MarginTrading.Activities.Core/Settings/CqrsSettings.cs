using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.Activities.Core.Settings
{
    public class CqrsSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        [Optional, CanBeNull]
        public string EnvironmentName { get; set; }

        [Optional]
        public CqrsContextNamesSettings ContextNames { get; set; } = new CqrsContextNamesSettings();
    }
}
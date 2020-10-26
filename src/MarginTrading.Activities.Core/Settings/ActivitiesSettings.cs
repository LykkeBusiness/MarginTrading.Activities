// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.Activities.Core.Settings
{
    [UsedImplicitly]
    public class ActivitiesSettings
    {
        public DbSettings Db { get; set; }

        [Optional, CanBeNull]
        public ChaosSettings ChaosKitty { get; set; }
        
        [Optional]
        public bool UseSerilog { get; set; }
        
        public CqrsSettings Cqrs { get; set; }
        
        public ConsumersSettings Consumers { get; set; }

        [Optional]
        public DefaultLegalEntitySettings LegalEntitySettings { get; set; } = new DefaultLegalEntitySettings();
    }
}

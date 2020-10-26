// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.SettingsReader.Attributes;

namespace MarginTrading.Activities.Core.Settings
{
    public class DefaultLegalEntitySettings
    {
        [Optional]
        public string DefaultLegalEntity { get; set; } = "Default";
    }
}
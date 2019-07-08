// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.Activities.Core.Settings
{
    [UsedImplicitly]
    public class DbSettings
    {
        public StorageMode StorageMode { get; set; }
        public string LogsConnString { get; set; }
    }
}

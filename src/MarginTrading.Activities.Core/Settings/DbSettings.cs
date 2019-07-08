// Copyright (c) 2019 Lykke Corp.

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

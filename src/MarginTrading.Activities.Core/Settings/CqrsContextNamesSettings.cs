// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.SettingsReader.Attributes;

namespace MarginTrading.Activities.Core.Settings
{
    public class CqrsContextNamesSettings
    {
        [Optional] public string AccountsManagement { get; set; } = nameof(AccountsManagement);

        [Optional] public string SettingsService { get; set; } = nameof(SettingsService);

        [Optional] public string Activities { get; set; } = nameof(Activities);

        [Optional] public string TradingEngine { get; set; } = nameof(TradingEngine);
    }
}
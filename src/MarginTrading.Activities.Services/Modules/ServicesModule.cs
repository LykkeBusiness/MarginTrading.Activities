// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;

using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Retries;
using Lykke.MarginTrading.Activities.Contracts.Models;

using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.AssetService.Contracts;

namespace MarginTrading.Activities.Services.Modules
{
    public class ServicesModule : Module
    {
        private readonly AppSettings _settings;

        public ServicesModule(AppSettings settings)
        {
            _settings = settings;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GuidIdentityGenerator>()
                .As<IIdentityGenerator>()
                .SingleInstance();

            builder.RegisterType<DateService>()
                .As<IDateService>()
                .SingleInstance();

            builder.RegisterType<AssetPairsCacheService>()
                .As<IAssetPairsCacheService>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<AccountService>()
                .As<IAccountsService>()
                .SingleInstance();

            //External
            
            var settingsClientGeneratorBuilder = HttpClientGenerator
                .BuildForUrl(_settings.MarginTradingSettingsServiceClient.ServiceUrl)
                .WithServiceName<LykkeErrorResponse>($"MT Settings [{_settings.MarginTradingSettingsServiceClient.ServiceUrl}]")
                .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromMilliseconds(30), 3));

            if (!string.IsNullOrWhiteSpace(_settings.MarginTradingSettingsServiceClient.ApiKey))
            {
                settingsClientGeneratorBuilder = settingsClientGeneratorBuilder
                    .WithApiKey(_settings.MarginTradingSettingsServiceClient.ApiKey);
            }

            builder.RegisterInstance(settingsClientGeneratorBuilder.Create().Generate<IAssetPairsApi>())
                .As<IAssetPairsApi>()
                .SingleInstance();

            var accountManagementGeneratorBuilder = HttpClientGenerator
                .BuildForUrl(_settings.MarginTradingAccountManagementServiceClient.ServiceUrl)
                .WithServiceName<LykkeErrorResponse>($"MT Settings [{_settings.MarginTradingAccountManagementServiceClient.ServiceUrl}]")
                .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromMilliseconds(30), 3));

            if (!string.IsNullOrWhiteSpace(_settings.MarginTradingAccountManagementServiceClient.ApiKey))
            {
                accountManagementGeneratorBuilder = accountManagementGeneratorBuilder
                    .WithApiKey(_settings.MarginTradingAccountManagementServiceClient.ApiKey);
            }

            builder.RegisterInstance(accountManagementGeneratorBuilder.Create().Generate<IAccountsApi>())
                .As<IAccountsApi>()
                .SingleInstance();
        }
    }
}
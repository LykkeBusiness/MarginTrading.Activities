// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Retries;
using Lykke.MarginTrading.Activities.Contracts.Models;
using Lykke.Snow.Common.Startup;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Activities.Services.Projections;
using MarginTrading.SettingsService.Contracts;

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

            builder.RegisterType<RabbitMqSubscriberService>()
                .As<IRabbitMqSubscriberService>()
                .SingleInstance();

            builder.RegisterType<AssetPairsCacheService>()
                .As<IAssetPairsCacheService>()
                .As<IStartable>()
                .SingleInstance();

            //Projections

            builder.RegisterType<OrdersProjection>()
                .As<IStartable>()
                .SingleInstance();
                
            builder.RegisterType<PositionsProjection>()
                .As<IStartable>()
                .SingleInstance();
            
            builder.RegisterType<MarginControlProjection>()
                .As<IStartable>()
                .SingleInstance();
            
            builder.RegisterType<SessionActivityProjection>()
                .As<IStartable>()
                .SingleInstance();
            
            //External
            
            var settingsClientGeneratorBuilder = HttpClientGenerator
                .BuildForUrl(_settings.MarginTradingSettingsServiceClient.ServiceUrl)
                .WithServiceName<LykkeErrorResponse>(
                    $"MT Settings [{_settings.MarginTradingSettingsServiceClient.ServiceUrl}]")
                .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromMilliseconds(30), 3));

            if (!string.IsNullOrWhiteSpace(_settings.MarginTradingSettingsServiceClient.ApiKey))
            {
                settingsClientGeneratorBuilder = settingsClientGeneratorBuilder
                    .WithApiKey(_settings.MarginTradingSettingsServiceClient.ApiKey);
            }

            builder.RegisterInstance(settingsClientGeneratorBuilder.Create().Generate<IAssetPairsApi>())
                .As<IAssetPairsApi>()
                .SingleInstance();

            builder.RegisterInstance(settingsClientGeneratorBuilder.Create().Generate<IAssetsApi>())
                .As<IAssetsApi>()
                .SingleInstance();
        }
    }
}
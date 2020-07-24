// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Retries;
using Lykke.MarginTrading.Activities.Contracts.Models;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Activities.Services.Projections;
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

            builder.RegisterType<RabbitMqSubscriberService>()
                .As<IRabbitMqSubscriberService>()
                .SingleInstance();

            builder.RegisterType<AssetPairsCacheService>()
                .As<IAssetPairsCacheService>()
                .As<IStartable>()
                .SingleInstance();
            
            //Projections
            
            builder.RegisterType<OrdersProjection>()
                .As<ISubscriber>()
                .SingleInstance();
                
            builder.RegisterType<PositionsProjection>()
                .As<ISubscriber>()
                .SingleInstance();
            
            builder.RegisterType<MarginControlProjection>()
                .As<ISubscriber>()
                .SingleInstance();
            
            builder.RegisterType<SessionActivityProjection>()
                .As<ISubscriber>()
                .SingleInstance();
            
            //External
            
            var settingsClientGeneratorBuilder = HttpClientGenerator
                .BuildForUrl(_settings.MarginTradingAssetServiceClient.ServiceUrl)
                .WithServiceName<LykkeErrorResponse>($"MT Settings [{_settings.MarginTradingAssetServiceClient.ServiceUrl}]")
                .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromMilliseconds(30), 3));

            if (!string.IsNullOrWhiteSpace(_settings.MarginTradingAssetServiceClient.ApiKey))
            {
                settingsClientGeneratorBuilder = settingsClientGeneratorBuilder
                    .WithApiKey(_settings.MarginTradingAssetServiceClient.ApiKey);
            }

            builder.RegisterInstance(settingsClientGeneratorBuilder.Create().Generate<IAssetPairsApi>())
                .As<IAssetPairsApi>()
                .SingleInstance();
        }
    }
}
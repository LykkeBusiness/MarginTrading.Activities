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
            
            //External
            
            var settingsClientGenerator = HttpClientGenerator
                .BuildForUrl(_settings.MarginTradingSettingsServiceClient.ServiceUrl)
                .WithServiceName<LykkeErrorResponse>(
                    $"MT Settings [{_settings.MarginTradingSettingsServiceClient.ServiceUrl}]")
                .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromMilliseconds(30), 3))
                .Create();

            builder.RegisterInstance(settingsClientGenerator.Generate<IAssetPairsApi>())
                .As<IAssetPairsApi>()
                .SingleInstance();
        }
    }
}
// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Autofac;
using JetBrains.Annotations;

using Lykke.MarginTrading.Activities.Contracts.Models;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Models;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SettingsReader;
using Lykke.SettingsReader.SettingsTemplate;
using MarginTrading.Activities.Core.Repositories;
using MarginTrading.Activities.SqlRepositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarginTrading.Activities.Broker
{
    [UsedImplicitly]
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        public Startup(IHostEnvironment env, IConfiguration configuration) : base(env, configuration)
        {
        }

        protected override string ApplicationName => "ActivitiesBroker";

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddSettingsTemplateGenerator();
        }

        protected override void ConfigureEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapSettingsTemplate();
        }

        protected override void RegisterCustomServices(ContainerBuilder builder,
            IReloadingManager<Settings> settings)
        {
            builder.AddMessagePackBrokerMessagingFactory<ActivityEvent>();
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();

            if (settings.CurrentValue.Db.StorageMode == StorageMode.Azure)
            {
                throw new NotImplementedException("Azure storage is not implemented yet");
            }

            if (settings.CurrentValue.Db.StorageMode == StorageMode.SqlServer)
            {
                builder.Register(c => new ActivitiesRepository(settings.CurrentValue.Db.ConnString,
                        c.Resolve<ILogger<ActivitiesRepository>>()))
                    .As<IActivitiesRepository>()
                    .SingleInstance();
            }
        }
    }
}
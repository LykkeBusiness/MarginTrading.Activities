// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Models;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SettingsReader;
using MarginTrading.Activities.Core.Repositories;
using MarginTrading.Activities.SqlRepositories;
using Microsoft.Extensions.Hosting;

namespace MarginTrading.Activities.Broker
{
    [UsedImplicitly]
    public class Startup : BrokerStartupBase<DefaultBrokerApplicationSettings<Settings>, Settings>
    {
        public Startup(IHostEnvironment env) : base(env)
        {
        }

        protected override string ApplicationName => "ActivitiesBroker";

        protected override void RegisterCustomServices(ContainerBuilder builder, 
            IReloadingManager<Settings> settings, ILog log)
        {
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();
            
            if (settings.CurrentValue.Db.StorageMode == StorageMode.Azure)
            {
                //todo implement azure repos before using
            }
            else if (settings.CurrentValue.Db.StorageMode == StorageMode.SqlServer)
            {
                builder.RegisterInstance(new ActivitiesRepository(
                        settings.CurrentValue.Db.ConnString, log))
                    .As<IActivitiesRepository>();
            }
        }
    }
}

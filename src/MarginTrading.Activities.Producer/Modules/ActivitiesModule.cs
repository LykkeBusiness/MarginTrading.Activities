using System;
using Autofac;
using Common.Log;
using Lykke.Common;
using Lykke.Common.Chaos;
using Lykke.SettingsReader;
using MarginTrading.Activities.Core.Repositories;
using MarginTrading.Activities.Core.Services;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services;
using MarginTrading.Activities.SqlRepositories;
using Microsoft.Extensions.Internal;

namespace MarginTrading.Activities.Producer.Modules
{
    internal class ActivitiesModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public ActivitiesModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings.Nested(s => s.Activities)).SingleInstance();
            builder.RegisterInstance(_settings.CurrentValue.Activities).SingleInstance();
            builder.RegisterInstance(_settings.CurrentValue.Activities.Db).SingleInstance();
            
            builder.RegisterInstance(_log).As<ILog>().SingleInstance();
            builder.RegisterType<SystemClock>().As<ISystemClock>().SingleInstance();
            
            builder.RegisterType<ThreadSwitcherToNewTask>()
                .As<IThreadSwitcher>()
                .SingleInstance();
            
            builder.RegisterChaosKitty(_settings.CurrentValue.Activities.ChaosKitty);

            RegisterServices(builder);
            RegisterRepositories(builder);
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            if (_settings.CurrentValue.Activities.Db.StorageMode == StorageMode.Azure)
            {
                throw new NotImplementedException("Azure repos are not implemented");
                //todo implement azure repos before using
            }
            else if (_settings.CurrentValue.Activities.Db.StorageMode == StorageMode.SqlServer)
            {
                builder.RegisterInstance(new ActivitiesRepository(
                        _settings.CurrentValue.Activities.Db.DataConnString, _log))
                    .As<IActivitiesRepository>();
            }
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            
            builder.RegisterType<ConvertService>()
                .As<IConvertService>()
                .SingleInstance();
        }
    }
}
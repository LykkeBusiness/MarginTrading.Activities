using Autofac;
using Common.Log;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services.Modules
{
    public class ServicesModule : Module
    {
        private readonly ActivitiesSettings _settings;
        private readonly ILog _log;

        public ServicesModule(ActivitiesSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GuidIdentityGenerator>().As<IIdentityGenerator>();
            builder.RegisterType<DateService>().As<IDateService>();
        }
    }
}
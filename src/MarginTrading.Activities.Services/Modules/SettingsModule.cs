// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;
using MarginTrading.Activities.Core.Settings;

namespace MarginTrading.Activities.Services.Modules
{
    public class SettingsModule : Module
    {
        private readonly AppSettings _settings;

        public SettingsModule(AppSettings settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings.ActivitiesProducer.LegalEntitySettings);
        }
    }
}
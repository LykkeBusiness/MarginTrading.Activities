using System.Collections.Generic;
using System.Linq;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Configuration.BoundedContext;
using Lykke.MarginTrading.Activities.Contracts.Models;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Activities.Services.Projections;
using MarginTrading.SettingsService.Contracts.AssetPair;

namespace MarginTrading.Activities.Services.Modules
{
    public class CqrsModule : Module
    {
        private const string EventsRoute = "events";
        private readonly CqrsSettings _settings;
        private readonly ILog _log;

        public CqrsModule(CqrsSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings.ContextNames).AsSelf().SingleInstance();
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>()
                .SingleInstance();
            builder.RegisterType<ActivitiesSender>().As<IActivitiesSender>()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.ConnectionString
            };
            var messagingEngine = new MessagingEngine(_log, new TransportResolver(
                new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }), new RabbitMqTransportFactory());

            // Sagas & command handlers
            builder.RegisterAssemblyTypes(GetType().Assembly).Where(t =>
                new[] {"Saga", "CommandsHandler", "Projection"}.Any(ending => t.Name.EndsWith(ending))).AsSelf();

            builder.Register(ctx => CreateEngine(ctx, messagingEngine)).As<ICqrsEngine>().SingleInstance();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx, IMessagingEngine messagingEngine)
        {
            var rabbitMqConventionEndpointResolver =
                new RabbitMqConventionEndpointResolver("RabbitMq", SerializationFormat.MessagePack,
                    environment: _settings.EnvironmentName);

            var registrations = new List<IRegistration>
            {
                Register.DefaultEndpointResolver(rabbitMqConventionEndpointResolver),
                RegisterContext(),
            };

            return new CqrsEngine(_log, ctx.Resolve<IDependencyResolver>(), messagingEngine,
                new DefaultEndpointProvider(), true, registrations.ToArray());
        }

        private IRegistration RegisterContext()
        {
            var contextRegistration = Register.BoundedContext(_settings.ContextNames.Activities);

            RegisterAccountsProjection(contextRegistration);
            RegisterAssetPairsProjection(contextRegistration);
            
            contextRegistration.PublishingEvents(typeof(ActivityEvent)).With(EventsRoute);

            return contextRegistration;
        }
                    
        private void RegisterAccountsProjection(
            IBoundedContextRegistration contextRegistration)
        {
            contextRegistration.ListeningEvents(
                    typeof(AccountChangedEvent))
                .From(_settings.ContextNames.AccountsManagement).On(EventsRoute)
                .WithProjection(
                    typeof(AccountsProjection), _settings.ContextNames.AccountsManagement);
        }
        
        private void RegisterAssetPairsProjection(
            IBoundedContextRegistration contextRegistration)
        {
            contextRegistration.ListeningEvents(
                    typeof(AssetPairChangedEvent))
                .From(_settings.ContextNames.SettingsService)
                .On(EventsRoute)
                .WithProjection(
                    typeof(AssetPairProjection), _settings.ContextNames.SettingsService);
        }
    }
}
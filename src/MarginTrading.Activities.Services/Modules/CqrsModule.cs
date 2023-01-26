// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Configuration.BoundedContext;
using Lykke.Cqrs.Middleware.Logging;
using Lykke.MarginTrading.Activities.Contracts.Models;
using Lykke.Messaging.Serialization;
using Lykke.Snow.Common.Correlation.Cqrs;
using Lykke.Snow.Cqrs;
using Lykke.Snow.PriceAlerts.Contract.Models.Events;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Projections;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.AssetService.Contracts.Products;
using MarginTrading.Backend.Contracts.Workflow.Liquidation.Events;
using Microsoft.Extensions.Logging;

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
            builder.Register(ctx =>
                {
                    var context = ctx.Resolve<IComponentContext>();
                    return new AutofacDependencyResolver(context);
                })
                .As<IDependencyResolver>()
                .SingleInstance();

            // Sagas & command handlers
            builder.RegisterAssemblyTypes(GetType().Assembly).Where(t =>
                new[] {"Saga", "CommandsHandler", "Projection"}.Any(ending => t.Name.EndsWith(ending))).AsSelf();

            builder.Register(CreateEngine)
                .As<ICqrsEngine>()
                .SingleInstance();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx)
        {
            var rabbitMqConventionEndpointResolver =
                new RabbitMqConventionEndpointResolver("RabbitMq", SerializationFormat.MessagePack,
                    environment: _settings.EnvironmentName);
            
            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = new Uri(_settings.ConnectionString, UriKind.Absolute)
            };

            var loggerFactory = ctx.Resolve<ILoggerFactory>();

            var registrations = new List<IRegistration>
            {
                Register.DefaultEndpointResolver(rabbitMqConventionEndpointResolver),
                RegisterContext(),
                Register.CommandInterceptors(new DefaultCommandLoggingInterceptor(loggerFactory)),
                Register.EventInterceptors(new DefaultEventLoggingInterceptor(loggerFactory))
            };

            var engine = new RabbitMqCqrsEngine(
                loggerFactory,
                ctx.Resolve<IDependencyResolver>(),
                new DefaultEndpointProvider(),
                rabbitMqSettings.Endpoint.ToString(),
                rabbitMqSettings.UserName,
                rabbitMqSettings.Password,
                true,
                registrations.ToArray());
            
            var correlationManager = ctx.Resolve<CqrsCorrelationManager>();
            engine.SetReadHeadersAction(correlationManager.FetchCorrelationIfExists);
            engine.SetWriteHeadersFunc(correlationManager.BuildCorrelationHeadersIfExists);

            return engine;
        }

        private IRegistration RegisterContext()
        {
            var contextRegistration = Register.BoundedContext(_settings.ContextNames.Activities);

            RegisterAccountsProjection(contextRegistration);
            RegisterAssetPairsProjection(contextRegistration);
            RegisterTradingEngineProjections(contextRegistration);
            RegisterPriceAlertsProjection(contextRegistration);
            
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
        
        private void RegisterPriceAlertsProjection(
            IBoundedContextRegistration contextRegistration)
        {
            contextRegistration.ListeningEvents(
                    typeof(PriceAlertChangedEvent))
                .From(_settings.ContextNames.PriceAlertsService).On(EventsRoute)
                .WithProjection(
                    typeof(PriceAlertsProjection), _settings.ContextNames.PriceAlertsService);
        }
        
        private void RegisterAssetPairsProjection(
            IBoundedContextRegistration contextRegistration)
        {
            contextRegistration.ListeningEvents(
                    typeof(ProductChangedEvent))
                .From(_settings.ContextNames.SettingsService)
                .On(nameof(ProductChangedEvent))
                .WithProjection(
                    typeof(ProductChangedProjection), _settings.ContextNames.SettingsService);
        }
        
        private void RegisterTradingEngineProjections(
            IBoundedContextRegistration contextRegistration)
        {
            contextRegistration.ListeningEvents(
                    typeof(OrderPlacementRejectedEvent))
                .From(_settings.ContextNames.TradingEngine)
                .On(EventsRoute)
                .WithProjection(
                    typeof(OrderPlacementRejectedProjection), _settings.ContextNames.TradingEngine);
            
            contextRegistration.ListeningEvents(
                    typeof(LiquidationStartedEvent),
                    typeof(LiquidationResumedEvent),
                    typeof(LiquidationFailedEvent),
                    typeof(LiquidationFinishedEvent))
                .From(_settings.ContextNames.TradingEngine)
                .On(EventsRoute)
                .WithProjection(
                    typeof(LiquidationProjection), _settings.ContextNames.TradingEngine);
        }
    }
}
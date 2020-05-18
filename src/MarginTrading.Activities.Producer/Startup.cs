// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Cqrs;
using Lykke.Logs;
using Lykke.Logs.MsSql;
using Lykke.Logs.MsSql.Repositories;
using Lykke.Logs.Serilog;
using Lykke.MarginTrading.Activities.Contracts.Api;
using Lykke.SettingsReader;
using Lykke.Snow.Common.Startup.Hosting;
using Lykke.Snow.Common.Startup.Log;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Producer.Infrastructure;
using MarginTrading.Activities.Producer.Modules;
using MarginTrading.Activities.Services;
using MarginTrading.Activities.Services.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MarginTrading.Activities.Producer
{
    [UsedImplicitly]
    public class Startup
    {
        private IReloadingManager<AppSettings> _mtSettingsManager;
        public static string ServiceName { get; } = PlatformServices.Default.Application.ApplicationName;
        private IHostingEnvironment Environment { get; }
        private ILifetimeScope ApplicationContainer { get; set; }
        private IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddSerilogJson(env)
                .AddEnvironmentVariables()
                .Build();
            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services
                    .AddControllers()
                    .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    });

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", ServiceName + " API");
                    options.OperationFilter<CustomOperationIdOperationFilter>();
                });

                _mtSettingsManager = Configuration.LoadSettings<AppSettings>();
                LogLocator.Log = CreateLog(Configuration, services, _mtSettingsManager);
                services.AddSingleton<ILoggerFactory>(x => new WebHostLoggerFactory(LogLocator.Log));
            }
            catch (Exception ex)
            {
                LogLocator.Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        [UsedImplicitly]
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new ActivitiesModule(_mtSettingsManager, LogLocator.Log));
            builder.RegisterModule(new CqrsModule(_mtSettingsManager.CurrentValue.ActivitiesProducer.Cqrs, LogLocator.Log));
            builder.RegisterModule(new ServicesModule(_mtSettingsManager.CurrentValue));
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                ApplicationContainer = app.ApplicationServices.GetAutofacRoot();
                
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseHsts();
                }

#if DEBUG
                app.UseLykkeMiddleware(ServiceName, ex => ex.ToString());
#else
                app.UseLykkeMiddleware(ServiceName,
                    ex => new ErrorResponse {ErrorMessage = "Technical problem", Details = ex.Message});
#endif
                
                
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
                app.UseSwagger();
                app.UseSwaggerUI(a => a.SwaggerEndpoint("/swagger/v1/swagger.json", "Main Swagger"));

                appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
                appLifetime.ApplicationStopping.Register(() => StopApplication().Wait());
                appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
            }
            catch (Exception ex)
            {
                LogLocator.Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).Wait();
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                var cqrsEngine = ApplicationContainer.Resolve<ICqrsEngine>();
                cqrsEngine.StartSubscribers();
                cqrsEngine.StartProcesses();

                Program.AppHost.WriteLogs(Environment, LogLocator.Log);
                
                await LogLocator.Log.WriteMonitorAsync("", "", "Started");
            }
            catch (Exception ex)
            {
                await LogLocator.Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Service still can receive and process requests here, so take care about it if you add logic here.
            }
            catch (Exception ex)
            {
                await LogLocator.Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
                
                throw;
            }
        }

        private async Task CleanUp()
        {
            try
            {
                await LogLocator.Log.WriteMonitorAsync("", "", "Terminating");
                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                await LogLocator.Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                (LogLocator.Log as IDisposable)?.Dispose();

                throw;
            }
        }

        private static ILog CreateLog(IConfiguration configuration, IServiceCollection services, 
            IReloadingManager<AppSettings> settings)
        {
            var logName = $"{nameof(Activities)}Log";
            
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            if (settings.CurrentValue.ActivitiesProducer.UseSerilog)
            {
                aggregateLogger.AddLog(new SerilogLogger(typeof(Startup).Assembly, configuration));
            }
            else if (settings.CurrentValue.ActivitiesProducer.Db.StorageMode == StorageMode.SqlServer)
            {
                aggregateLogger.AddLog(new LogToSql(new SqlLogRepository(logName,
                    settings.CurrentValue.ActivitiesProducer.Db.LogsConnString)));
            }
            else if (settings.CurrentValue.ActivitiesProducer.Db.StorageMode == StorageMode.Azure)
            {
                aggregateLogger.AddLog(services.UseLogToAzureStorage(settings.Nested(s => s.ActivitiesProducer.Db.LogsConnString),
                    null, logName, consoleLogger));
            }

            return aggregateLogger;
        }
    }
}
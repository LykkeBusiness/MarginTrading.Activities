using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Retries;
using Lykke.MarginTrading.Activities.Contracts.Models;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.Activities.Core.Repositories;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.RecoveryTool.LogParsers;
using MarginTrading.Activities.RecoveryTool.Mappers;
using MarginTrading.Activities.Services;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Activities.SqlRepositories;
using MarginTrading.AssetService.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarginTrading.Activities.RecoveryTool
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var cache = host.Services.GetService<IAssetPairsCacheService>() as AssetPairsCacheService;
            cache?.Start();

            var app = host.Services.GetService<App>();
            await app.ImportFromActivityProducerAsync();
            await app.ImportFromTradingCoreAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("appsettings.json");
                })
                .ConfigureLogging(x => x.AddConsole())
                .ConfigureServices((context, services) =>
                {
                    // http clients
                    var settings = context.Configuration.Get<AppSettings>();

                    var accountManagementGeneratorBuilder = HttpClientGenerator
                        .BuildForUrl(settings.MarginTradingAccountManagementServiceClient.ServiceUrl)
                        .WithServiceName<LykkeErrorResponse>(
                            $"MT Settings [{settings.MarginTradingAccountManagementServiceClient.ServiceUrl}]")
                        .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromMilliseconds(30), 3));

                    if (!string.IsNullOrWhiteSpace(settings.MarginTradingAccountManagementServiceClient.ApiKey))
                    {
                        accountManagementGeneratorBuilder = accountManagementGeneratorBuilder
                            .WithApiKey(settings.MarginTradingAccountManagementServiceClient.ApiKey);
                    }

                    var settingsClientGeneratorBuilder = HttpClientGenerator
                        .BuildForUrl(settings.MarginTradingSettingsServiceClient.ServiceUrl)
                        .WithServiceName<LykkeErrorResponse>(
                            $"MT Settings [{settings.MarginTradingSettingsServiceClient.ServiceUrl}]")
                        .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromMilliseconds(30), 3));

                    if (!string.IsNullOrWhiteSpace(settings.MarginTradingSettingsServiceClient.ApiKey))
                    {
                        settingsClientGeneratorBuilder = settingsClientGeneratorBuilder
                            .WithApiKey(settings.MarginTradingSettingsServiceClient.ApiKey);
                    }

                    // db connection
                    var cs = context.Configuration.GetConnectionString("db");

                    // container
                    services.AddSingleton(context.Configuration);
                    services.AddSingleton<App>();
                    services.AddSingleton<ActivityProducerLogParser>();
                    services.AddSingleton<TradingCoreLogParser>();
                    services.AddSingleton<ActivityMapper>();
                    services.AddSingleton<IIdentityGenerator, GuidIdentityGenerator>();
                    services.AddSingleton<IActivitiesRepository, ActivitiesRepository>(x =>
                        new ActivitiesRepository(cs, EmptyLog.Instance));
                    services.AddSingleton(accountManagementGeneratorBuilder.Create()
                        .Generate<IAccountsApi>());
                    services.AddSingleton(settingsClientGeneratorBuilder.Create().Generate<IAssetPairsApi>());
                    services.AddSingleton<IAccountsService, AccountService>();
                    services.AddSingleton<IAssetPairsCacheService, AssetPairsCacheService>();

                    // mappers
                    services.AddSingleton<AccountChangedEventMapper>();

                    services.AddSingleton<LiquidationStartedEventMapper>();
                    services.AddSingleton<LiquidationResumedEventMapper>();
                    services.AddSingleton<LiquidationFailedEventMapper>();
                    services.AddSingleton<LiquidationFinishedEventMapper>();

                    services.AddSingleton<MarginEventMessageMapper>();

                    services.AddSingleton<OrderPlacementRejectedEventMapper>();

                    services.AddSingleton<OrderHistoryEventMapper>();

                    services.AddSingleton<PositionHistoryEventMapper>();
                });

            return hostBuilder;
        }
    }
}
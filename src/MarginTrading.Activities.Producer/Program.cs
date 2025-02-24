﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;

using Lykke.SettingsReader.ConfigurationProvider;

using MarginTrading.Activities.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace MarginTrading.Activities.Producer
{
    [UsedImplicitly]
    internal class Program
    {
        internal static IHost AppHost { get; private set; }

        public static async Task Main()
        {
            Console.WriteLine($"{PlatformServices.Default.Application.ApplicationName} version {PlatformServices.Default.Application.ApplicationVersion}");

            var restartAttemptsLeft = int.TryParse(Environment.GetEnvironmentVariable("RESTART_ATTEMPTS_NUMBER"),
                out var restartAttemptsFromEnv)
                ? restartAttemptsFromEnv
                : int.MaxValue;
            var restartAttemptsInterval = int.TryParse(Environment.GetEnvironmentVariable("RESTART_ATTEMPTS_INTERVAL_MS"),
                out var restartAttemptsIntervalFromEnv)
                ? restartAttemptsIntervalFromEnv
                : 10000;

            while (restartAttemptsLeft > 0)
            {
                try
                {
                    var configurationBuilder = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddUserSecrets<Startup>()
                        .AddEnvironmentVariables();

                    if (Environment.GetEnvironmentVariable("SettingsUrl")?.StartsWith("http") ?? false)
                    {
                        configurationBuilder.AddHttpSourceConfiguration();
                    }

                    var configuration = configurationBuilder.Build();

                    AppHost = Host.CreateDefaultBuilder()
                        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.ConfigureKestrel(serverOptions =>
                                {
                                    // Set properties and call methods on options
                                })
                                .UseConfiguration(configuration)
                                .UseStartup<Startup>();
                        })
                        .Build();

                    await AppHost.RunAsync();
                }
                catch (Exception e)
                {
                    if (LogLocator.Log == null)
                    {
                        Console.WriteLine($"Fatal Error: {e.Message}{Environment.NewLine}{e.StackTrace}{Environment.NewLine}Restarting...");
                    }
                    else
                    {
                        await LogLocator.Log.WriteFatalErrorAsync($"MT {nameof(Activities)}", "Restart host",
                            $"Attempts left: {restartAttemptsLeft}", e);
                    }

                    restartAttemptsLeft--;
                    Thread.Sleep(restartAttemptsInterval);
                }
            }

            Console.WriteLine("Terminated");
        }
    }
}
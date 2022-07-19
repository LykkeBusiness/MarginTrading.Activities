// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.Core.Repositories;
using MarginTrading.Activities.RecoveryTool.LogParsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MarginTrading.Activities.RecoveryTool
{
    public class App
    {
        private readonly ActivityProducerLogParser _activityProducerLogParser;
        private readonly TradingCoreLogParser _tradingCoreLogParser;
        private readonly ActivityMapper _activityMapper;
        private readonly IActivitiesRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<App> _logger;

        private readonly bool _dryRun;

        public App(
            ActivityProducerLogParser activityProducerLogParser,
            TradingCoreLogParser tradingCoreLogParser,
            ActivityMapper activityMapper,
            IActivitiesRepository repository,
            IConfiguration configuration,
            ILogger<App> logger)
        {
            _activityProducerLogParser = activityProducerLogParser;
            _tradingCoreLogParser = tradingCoreLogParser;
            _activityMapper = activityMapper;
            _repository = repository;
            _configuration = configuration;
            _logger = logger;

            _dryRun = _configuration.GetValue<bool>("DryRun");
        }

        public async Task ImportFromActivityProducerAsync()
        {
            _logger.LogInformation("Starting to import data from Activity Producer");

            var activityProducerPath = _configuration.GetValue<string>("ActivityProducerLogDirectory");
            var files = GetFiles(activityProducerPath, "activity producer");

            foreach (var file in files)
            {
                _logger.LogInformation("Starting to parse {File}", file);
                var events = _activityProducerLogParser.Parse(await File.ReadAllTextAsync(file));

                var activities = new List<IActivity>();
                foreach (var domainEvent in events)
                {
                    var res = await _activityMapper.Map(domainEvent);
                    activities.AddRange(res);
                }

                LogStats(activities);
                await InsertEvents(activities);

                _logger.LogInformation("File {File} uploaded", file);
            }

            _logger.LogInformation("Data from Activity Producer imported");
        }

        private void LogStats(List<IActivity> activities)
        {
            var groups = activities.GroupBy(x => x.Category);
            foreach (var group in groups)
            {
                _logger.LogInformation("Found {Count} activities of category {Category}",
                    group.Count(),
                    group.Key);
            }
        }

        public async Task ImportFromTradingCoreAsync()
        {
            _logger.LogInformation("Starting to import data from Trading Core");

            var tradingCorePath = _configuration.GetValue<string>("TradingCoreLogDirectory");
            var files = GetFiles(tradingCorePath, "trading core");

            foreach (var file in files)
            {
                _logger.LogInformation("Starting to parse {File}", file);
                var events = _tradingCoreLogParser.Parse(await File.ReadAllTextAsync(file));

                var activities = new List<IActivity>();
                foreach (var domainEvent in events)
                {
                    var res = await _activityMapper.Map(domainEvent);
                    activities.AddRange(res);
                }

                foreach (var activity in activities)
                {
                    await _repository.InsertIfNotExist(activity);
                }

                _logger.LogInformation("File {File} uploaded", file);
            }

            _logger.LogInformation("Data from Trading Core imported");
        }

        private async Task InsertEvents(List<IActivity> activities)
        {
            if (_dryRun)
            {
                _logger.LogWarning("Dry run is enabled. Activities will not be saved to the database");
            }

            foreach (var activity in activities)
            {
                _logger.LogInformation("Inserting event with id: {Id}, category: {Category}",
                    activity.Id, activity.Category);
                if (!_dryRun)
                {
                    await _repository.InsertIfNotExist(activity);
                }
            }
        }

        private List<string> GetFiles(string path, string service)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogError("Directory {Path} for service {Service} not found",
                    path, service);
                throw new Exception("Check directory configuration: directory not found");
            }

            var files = Directory.EnumerateFiles(path).ToList();
            if (files.Count == 0)
            {
                _logger.LogError("Logfiles not found for service {Service}", service);
                throw new Exception("Check directory configuration: logfiles not found");
            }

            return files;
        }
    }
}
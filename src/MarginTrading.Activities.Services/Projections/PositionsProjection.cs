// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using MarginTrading.Activities.Core.Caches;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Activities;
using MarginTrading.Backend.Contracts.Events;

namespace MarginTrading.Activities.Services.Projections
{
    public class PositionsProjection : IStartable
    {
        private readonly IRabbitMqSubscriberService _rabbitMqSubscriberService;
        private readonly ActivitiesSettings _settings;
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly ILog _log;
        private readonly IAssetPairsCacheService _assetPairsCacheService;
        private readonly IAssetsCache _assetsCache;

        public PositionsProjection(IRabbitMqSubscriberService rabbitMqSubscriberService,
            ActivitiesSettings settings,
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            ILog log,
            IAssetPairsCacheService assetPairsCacheService,
            IAssetsCache assetsCache)
        {
            _rabbitMqSubscriberService = rabbitMqSubscriberService;
            _settings = settings;
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
            _log = log;
            _assetPairsCacheService = assetPairsCacheService;
            _assetsCache = assetsCache;
        }

        public void Start()
        {
            _rabbitMqSubscriberService.Subscribe(_settings.Consumers.Positions,
                true,
                HandlePositionHistoryEvent,
                _rabbitMqSubscriberService.GetJsonDeserializer<PositionHistoryEvent>());
        }

        private Task HandlePositionHistoryEvent(PositionHistoryEvent historyEvent)
        {
            var position = historyEvent.PositionSnapshot;

            if (position == null)
            {
                _log.WriteWarning(
                    nameof(HandlePositionHistoryEvent),
                    historyEvent?.ToJson(),
                    "Position snapshot is null. Unable to create activity."
                );

                return Task.CompletedTask;
            }

            var deal = historyEvent.Deal;

            if (deal == null && historyEvent.EventType != PositionHistoryTypeContract.Open)
            {
                _log.WriteWarning(
                    nameof(HandlePositionHistoryEvent),
                    historyEvent?.ToJson(),
                    $"Deal snapshot is null for {historyEvent.EventType}. Unable to create activity."
                );

                return Task.CompletedTask;
            }

            var asset = _assetsCache.GetAsset(position.AssetPairId);

            var eventSourceId = position.Id;
            var activityType = ActivityType.None;
            var descriptionAttributes = new string[0];
            var relatedIds = new string[0];

            switch (historyEvent.EventType)
            {
                case PositionHistoryTypeContract.Open:

                    var assetPair = _assetPairsCacheService.TryGetAssetPair(position.AssetPairId);
                    var metadata = historyEvent.ActivitiesMetadata.DeserializeJson<PositionOpenMetadata>();
                    activityType = metadata.ExistingPositionIncreased
                        ? ActivityType.PositionIncrease
                        : ActivityType.PositionOpening;
                    relatedIds = new[] { position.OpenTradeId, position.OpenTradeId, position.Id };
                    descriptionAttributes = new[]
                    {
                        position.Direction.ToString(),
                        position.Volume.ToUiString(asset?.Accuracy),
                        asset?.Name ?? position.AssetPairId,
                        position.OpenPrice.ToUiString(asset?.Accuracy),
                        assetPair?.QuoteAssetId
                    };

                    break;

                case PositionHistoryTypeContract.Close:

                    eventSourceId = deal.DealId;
                    activityType = ActivityType.PositionClosing;
                    relatedIds = new[] { deal.CloseTradeId, deal.CloseTradeId, position.Id, deal.DealId };
                    descriptionAttributes = new[]
                    {
                        position.Direction.ToString(),
                        deal.Volume.ToUiString(asset?.Accuracy),
                        asset?.Name ?? position.AssetPairId,
                        deal.Fpl.ToUiString(asset?.Accuracy)
                    };

                    break;

                case PositionHistoryTypeContract.PartiallyClose:

                    eventSourceId = deal.DealId;
                    activityType = ActivityType.PositionPartialClosing;
                    relatedIds = new[] { deal.CloseTradeId, deal.CloseTradeId, position.Id, deal.DealId };
                    descriptionAttributes = new[]
                    {
                        position.Direction.ToString(),
                        deal.Volume.ToUiString(asset?.Accuracy),
                        asset?.Name ?? position.AssetPairId,
                        deal.Fpl.ToUiString(asset?.Accuracy)
                    };

                    break;

                default:
                    return Task.CompletedTask;
            }

            var activity = new Activity(
                _identityGenerator.GenerateId(),
                position.AccountId,
                position.AssetPairId,
                eventSourceId,
                historyEvent.Timestamp,
                activityType,
                descriptionAttributes,
                relatedIds
            );

            _cqrsSender.PublishActivity(activity);

            return Task.CompletedTask;
        }
    }
}
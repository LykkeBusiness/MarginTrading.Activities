// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

using Common;
using Common.Log;

using JetBrains.Annotations;

using Lykke.RabbitMqBroker.Subscriber;

using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Activities;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;

namespace MarginTrading.Activities.Services.MessageHandlers
{
    [UsedImplicitly]
    public sealed class PositionsHistoryHandler : IMessageHandler<PositionHistoryEvent>
    {
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly ILog _log;
        private readonly IAssetPairsCacheService _assetPairsCacheService;

        public PositionsHistoryHandler(
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            IAssetPairsCacheService assetPairsCacheService,
            ILog log)
        {
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
            _assetPairsCacheService = assetPairsCacheService;
            _log = log;
        }

        public Task Handle(PositionHistoryEvent message)
        {
            var position = message.PositionSnapshot;

            if (position == null)
            {
                _log.WriteWarning(
                    nameof(Handle),
                    message.ToJson(),
                    "Position snapshot is null. Unable to create activity."
                );

                return Task.CompletedTask;
            }

            var deal = message.Deal;

            if (deal == null && message.EventType != PositionHistoryTypeContract.Open)
            {
                _log.WriteWarning(
                    nameof(Handle),
                    message.ToJson(),
                    $"Deal snapshot is null for {message.EventType}. Unable to create activity."
                );

                return Task.CompletedTask;
            }

            var assetPair = _assetPairsCacheService.TryGetAssetPair(position.AssetPairId);

            var eventSourceId = position.Id;
            ActivityType activityType;
            string[] descriptionAttributes;
            string[] relatedIds;

            switch (message.EventType)
            {
                case PositionHistoryTypeContract.Open:

                    var metadata = message.ActivitiesMetadata.DeserializeJson<PositionOpenMetadata>();
                    activityType = metadata.ExistingPositionIncreased
                        ? ActivityType.PositionIncrease
                        : ActivityType.PositionOpening;
                    relatedIds = new[] { position.OpenTradeId, position.OpenTradeId, position.Id };
                    descriptionAttributes = new[]
                    {
                        position.Direction.ToString(), position.Volume.ToUiString(assetPair?.Accuracy),
                        assetPair?.Name ?? position.AssetPairId, position.OpenPrice.ToUiString(assetPair?.Accuracy),
                        assetPair?.QuoteAssetId
                    };

                    break;

                case PositionHistoryTypeContract.Close:

                    eventSourceId = deal?.DealId;
                    activityType = ActivityType.PositionClosing;
                    relatedIds = new[] { deal?.CloseTradeId, deal?.CloseTradeId, position.Id, deal?.DealId };
                    descriptionAttributes = new[]
                    {
                        position.Direction.ToString(), deal?.Volume.ToUiString(assetPair?.Accuracy),
                        assetPair?.Name ?? position.AssetPairId, deal?.Fpl.ToUiString(assetPair?.Accuracy),
                        position.AccountAssetId
                    };

                    break;

                case PositionHistoryTypeContract.PartiallyClose:

                    eventSourceId = deal?.DealId;
                    activityType = ActivityType.PositionPartialClosing;
                    relatedIds = new[] { deal?.CloseTradeId, deal?.CloseTradeId, position.Id, deal?.DealId };
                    descriptionAttributes = new[]
                    {
                        position.Direction.ToString(), deal?.Volume.ToUiString(assetPair?.Accuracy),
                        assetPair?.Name ?? position.AssetPairId, deal?.Fpl.ToUiString(assetPair?.Accuracy),
                        position.AccountAssetId
                    };

                    break;

                default:
                    return Task.CompletedTask;
            }

            var isOnBehalf = CheckIfOnBehalf(message);

            var activity = new Activity(
                _identityGenerator.GenerateId(),
                position.AccountId,
                position.AssetPairId,
                eventSourceId,
                message.Timestamp,
                activityType,
                descriptionAttributes,
                relatedIds,
                isOnBehalf: isOnBehalf
            );

            _cqrsSender.PublishActivity(activity);

            return Task.CompletedTask;
        }

        public static bool CheckIfOnBehalf(PositionHistoryEvent historyEvent)
        {
            if (historyEvent.EventType == PositionHistoryTypeContract.Open)
            {
                return historyEvent.PositionSnapshot.OpenOriginator == OriginatorTypeContract.OnBehalf;
            }

            return historyEvent.PositionSnapshot.CloseOriginator == OriginatorTypeContract.OnBehalf;
        }
    }
}
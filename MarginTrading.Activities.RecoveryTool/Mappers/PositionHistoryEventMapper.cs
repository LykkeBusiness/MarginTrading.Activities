// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.RecoveryTool.Model;
using MarginTrading.Activities.Services;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Activities;
using MarginTrading.Backend.Contracts.Events;
using Newtonsoft.Json;

namespace MarginTrading.Activities.RecoveryTool.Mappers
{
    public class PositionHistoryEventMapper : IActivityMapper
    {
        private readonly IIdentityGenerator _identityGenerator;
        private readonly IAssetPairsCacheService _assetPairsCacheService;

        public PositionHistoryEventMapper(IIdentityGenerator identityGenerator,
            IAssetPairsCacheService assetPairsCacheService)
        {
            _identityGenerator = identityGenerator;
            _assetPairsCacheService = assetPairsCacheService;
        }


        public async Task<List<IActivity>> Map(DomainEvent domainEvent)
        {
            var historyEvent = JsonConvert.DeserializeObject<PositionHistoryEvent>(domainEvent.Json);

            var position = historyEvent.PositionSnapshot;

            if (position == null)
            {
                return new List<IActivity>();
            }

            var deal = historyEvent.Deal;

            if (deal == null && historyEvent.EventType != PositionHistoryTypeContract.Open)
            {
                return new List<IActivity>();
            }

            var assetPair = _assetPairsCacheService.TryGetAssetPair(position.AssetPairId);

            var eventSourceId = position.Id;
            var activityType = ActivityType.None;
            var descriptionAttributes = new string[0];
            var relatedIds = new string[0];

            switch (historyEvent.EventType)
            {
                case PositionHistoryTypeContract.Open:

                    var metadata = historyEvent.ActivitiesMetadata.DeserializeJson<PositionOpenMetadata>();
                    activityType = metadata.ExistingPositionIncreased
                        ? ActivityType.PositionIncrease
                        : ActivityType.PositionOpening;
                    relatedIds = new[] {position.OpenTradeId, position.OpenTradeId, position.Id};
                    descriptionAttributes = new[]
                    {
                        position.Direction.ToString(),
                        position.Volume.ToUiString(assetPair?.Accuracy),
                        assetPair?.Name ?? position.AssetPairId,
                        position.OpenPrice.ToUiString(assetPair?.Accuracy),
                        assetPair?.QuoteAssetId
                    };

                    break;

                case PositionHistoryTypeContract.Close:

                    eventSourceId = deal.DealId;
                    activityType = ActivityType.PositionClosing;
                    relatedIds = new[] {deal.CloseTradeId, deal.CloseTradeId, position.Id, deal.DealId};
                    descriptionAttributes = new[]
                    {
                        position.Direction.ToString(),
                        deal.Volume.ToUiString(assetPair?.Accuracy),
                        assetPair?.Name ?? position.AssetPairId,
                        deal.Fpl.ToUiString(assetPair?.Accuracy),
                        position.AccountAssetId
                    };

                    break;

                case PositionHistoryTypeContract.PartiallyClose:

                    eventSourceId = deal.DealId;
                    activityType = ActivityType.PositionPartialClosing;
                    relatedIds = new[] {deal.CloseTradeId, deal.CloseTradeId, position.Id, deal.DealId};
                    descriptionAttributes = new[]
                    {
                        position.Direction.ToString(),
                        deal.Volume.ToUiString(assetPair?.Accuracy),
                        assetPair?.Name ?? position.AssetPairId,
                        deal.Fpl.ToUiString(assetPair?.Accuracy),
                        position.AccountAssetId
                    };

                    break;

                default:
                    return new List<IActivity>();
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

            return new List<IActivity>() {activity};
        }
    }
}
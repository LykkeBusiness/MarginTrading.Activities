// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Common;
using Common.Log;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Activities;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;

namespace MarginTrading.Activities.Services.Projections
{
    public class PositionsProjection : ISubscriber
    {
        private readonly IRabbitMqSubscriberService _rabbitMqSubscriberService;
        private readonly ActivitiesSettings _settings;
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly ILog _log;
        private readonly IAssetPairsCacheService _assetPairsCacheService;
        
        public PositionsProjection(IRabbitMqSubscriberService rabbitMqSubscriberService,
            ActivitiesSettings settings,
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            ILog log,
            IAssetPairsCacheService assetPairsCacheService)
        {
            _rabbitMqSubscriberService = rabbitMqSubscriberService;
            _settings = settings;
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
            _log = log;
            _assetPairsCacheService = assetPairsCacheService;
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
                    return Task.CompletedTask;
            }

            var isOnBehalf = CheckIfOnBehalf(historyEvent);
            
            var activity = new Activity(
                _identityGenerator.GenerateId(),
                position.AccountId,
                position.AssetPairId,
                eventSourceId,
                historyEvent.Timestamp,
                activityType,
                descriptionAttributes,
                relatedIds,
                isOnBehalf: isOnBehalf
            );

            _cqrsSender.PublishActivity(activity);
            
            return Task.CompletedTask;
        }

        public bool CheckIfOnBehalf(PositionHistoryEvent historyEvent)
        {
            if(historyEvent.EventType == PositionHistoryTypeContract.Open)
            {
                return historyEvent.PositionSnapshot.OpenOriginator == OriginatorTypeContract.OnBehalf;
            }
            else
            {
                return historyEvent.PositionSnapshot.CloseOriginator == OriginatorTypeContract.OnBehalf;
            }
        }
    }
}
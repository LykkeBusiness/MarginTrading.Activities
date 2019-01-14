using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.Backend.Contracts.TradeMonitoring;
using Newtonsoft.Json;
using OrderStatusContract = MarginTrading.Backend.Contracts.Orders.OrderStatusContract;

namespace MarginTrading.Activities.Services.Projections
{
    public class OrdersProjection : IStartable
    {
        private readonly IRabbitMqSubscriberService _rabbitMqSubscriberService;
        private readonly ActivitiesSettings _settings;
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly ILog _log;
        private readonly IAssetPairsCacheService _assetPairsCacheService;
        
        public OrdersProjection(IRabbitMqSubscriberService rabbitMqSubscriberService,
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
            _rabbitMqSubscriberService.Subscribe(_settings.Consumers.Orders,
                true,
                HandleOrderHistoryEvent,
                _rabbitMqSubscriberService.GetJsonDeserializer<OrderHistoryEvent>());
        }

        private Task HandleOrderHistoryEvent(OrderHistoryEvent historyEvent)
        {
            var order = historyEvent.OrderSnapshot;

            if (order == null)
            {
                _log.WriteWarning(
                    nameof(HandleOrderHistoryEvent),
                    historyEvent?.ToJson(),
                    "Order snapshot is null. Unable to create activity."
                );
                
                return Task.CompletedTask;
            }

            var activityType = ActivityType.None;
            var descriptionAttributes = new List<string>();
            var relatedIds = new string[0];
            
            switch (historyEvent.Type)
            {
                case OrderHistoryTypeContract.Place:

                    //basic orders place event is always combined with another event type
                    if (IsBasicOrder(order) || !string.IsNullOrEmpty(order.PositionId))
                        return Task.CompletedTask;

                    activityType = ActivityType.OrderAcceptance;
                    relatedIds = new[] {order.Id};
                    descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                    descriptionAttributes.Add(GetValidity(order.ValidityTime));
                    
                    break;
                
                case OrderHistoryTypeContract.Activate:

                    if (IsBasicPendingOrder(order) || !string.IsNullOrEmpty(order.PositionId))
                    {
                        activityType = ActivityType.OrderAcceptanceAndActivation;
                        relatedIds = new[] {order.Id};
                        descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                        descriptionAttributes.Add(GetValidity(order.ValidityTime));
                    }
                    else
                    {
                        activityType = ActivityType.OrderActivation;
                        relatedIds = new[] {order.Id};
                        descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                    }
                    
                    break;
                
                case OrderHistoryTypeContract.Change:
                    
                    activityType = ActivityType.OrderModification;
                    relatedIds = new[] {order.Id};
                    descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                    descriptionAttributes.Add("previous value"); //TODO: get previous value from event
                    descriptionAttributes.Add(order.ExpectedOpenPrice?.ToString());
                    
                    break;
                
                case OrderHistoryTypeContract.Reject:

                    relatedIds = new[] {order.Id};
                    descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));

                    switch (order.RejectReason)
                    {
                        case OrderRejectReasonContract.ShortPositionsDisabled :
                            activityType = ActivityType.OrderRejectionBecauseShortDisabled;
                            break;
                        
                        //TODO: this also can happen when volume is not valid because of min deal limit.. need clarification?
                        case OrderRejectReasonContract.InvalidVolume:
                            activityType = ActivityType.OrderRejectionBecauseMaxPositionLimit;
                            break;
                        
                        case OrderRejectReasonContract.NotEnoughBalance:
                            activityType = ActivityType.OrderRejectionBecauseNotSufficientCapital;
                            break;
                        
                        case OrderRejectReasonContract.NoLiquidity:
                            activityType = ActivityType.OrderRejectionBecauseNoLiquidity;
                            break;
                        
                        default:
                            activityType = ActivityType.OrderRejection;
                            break;
                    }
                    
                    break;
                
                case OrderHistoryTypeContract.Cancel:

                    relatedIds = new[] {order.Id};
                    descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                    //TODO: get cancellation reason from event
                    activityType = ActivityType.OrderCancellation;
                    
                    break;
                
                case OrderHistoryTypeContract.Executed:

                    relatedIds = new[] {order.Id, order.Id};
                    descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));

                    activityType = order.Type == OrderTypeContract.Market ? 
                        ActivityType.OrderAcceptanceAndExecution 
                        : ActivityType.OrderExecution;

                    HandleCancellationAndAdjustment(historyEvent, order);
                    
                    break;
                
                default:
                    return Task.CompletedTask;
            }

            PublishActivity(
                historyEvent,
                order,
                activityType,
                descriptionAttributes,
                relatedIds);

            return Task.CompletedTask;
        }

        
        #region Helpers
        
        private static bool IsBasicPendingOrder(OrderContract order)
        {
            return order.Type == OrderTypeContract.Limit || order.Type == OrderTypeContract.Stop;
        }

        private static bool IsBasicOrder(OrderContract order)
        {
            return order.Type == OrderTypeContract.Market ||
                   IsBasicPendingOrder(order);
        }

        private static string GetValidity(DateTime? validity)
        {
            return validity.HasValue ? validity.Value.ToString("g") : "GTC";
        }

        private List<string> GetCommonDescriptionAttributesForOrder(OrderContract order)
        {
            var assetPair = _assetPairsCacheService.TryGetAssetPair(order.AssetPairId);

            var result = new List<string>
            {
                order.Direction.ToString(),
                order.Type.ToString(),
                order.Volume.ToUiString(0),
                assetPair?.Name ?? order.AssetPairId,
            };

            if (order.Type == OrderTypeContract.Market)
            {
                if (order.Status == OrderStatusContract.Executed)
                {
                    result.AddRange(new[]
                    {
                        order.ExecutionPrice.ToUiString(assetPair?.Accuracy),
                        assetPair?.QuoteAssetId
                    });
                }
            }
            else
            {
                result.AddRange(new[]
                {
                    order.ExpectedOpenPrice.ToUiString(assetPair?.Accuracy),
                    assetPair?.QuoteAssetId
                });

                if (order.Status == OrderStatusContract.Executed)
                {
                    result.AddRange(new[]
                    {
                        order.ExecutionPrice.ToUiString(assetPair?.Accuracy),
                        assetPair?.QuoteAssetId
                    });
                }
            }

            return result;
        }
        
        private void HandleCancellationAndAdjustment(OrderHistoryEvent historyEvent, OrderContract order)
        {
            var additionalAttributes = JsonConvert.DeserializeAnonymousType(
                order.AdditionalInfo,
                new
                {
                    TargetTradeId = "",
                    IsCancellationTrade = false,
                    IsAdjustmentTrade = false
                });

            if (additionalAttributes.IsCancellationTrade)
            {
                var activityType = ActivityType.CancellationTrade;
                var descriptionAttributes = GetCommonDescriptionAttributesForOrder(order);
                descriptionAttributes.Add(additionalAttributes.TargetTradeId);
                var relatedIds = new[] {order.Id, order.Id};

                PublishActivity(historyEvent,
                    order,
                    activityType,
                    descriptionAttributes,
                    relatedIds);
            }
            
            if (additionalAttributes.IsAdjustmentTrade)
            {
                var activityType = ActivityType.AdjustmentTrade;
                var descriptionAttributes = GetCommonDescriptionAttributesForOrder(order);
                descriptionAttributes.Add(additionalAttributes.TargetTradeId);
                var relatedIds = new[] {order.Id, order.Id};

                PublishActivity(historyEvent,
                    order,
                    activityType,
                    descriptionAttributes,
                    relatedIds);
            }
        }
        
        private void PublishActivity(OrderHistoryEvent historyEvent, OrderContract order, ActivityType activityType,
            List<string> descriptionAttributes, string[] relatedIds)
        {
            var activity = new Activity(
                _identityGenerator.GenerateId(),
                order.AccountId,
                order.AssetPairId,
                order.Id,
                historyEvent.Timestamp,
                activityType,
                descriptionAttributes.ToArray(),
                relatedIds
            );

            _cqrsSender.PublishActivity(activity);
        }
        
        #endregion
        
    }
}
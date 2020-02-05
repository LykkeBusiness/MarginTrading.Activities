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
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.Backend.Contracts.TradeMonitoring;
using MarginTrading.SettingsService.Contracts.AssetPair;
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
        private readonly IAssetsCache _assetsCache;

        public OrdersProjection(IRabbitMqSubscriberService rabbitMqSubscriberService,
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
                    relatedIds = new[] { order.Id };
                    descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                    descriptionAttributes.Add(GetValidity(order.ValidityTime));

                    break;

                case OrderHistoryTypeContract.Activate:

                    if (IsBasicPendingOrder(order) || !string.IsNullOrEmpty(order.PositionId))
                    {
                        activityType = ActivityType.OrderAcceptanceAndActivation;
                        relatedIds = new[] { order.Id };
                        descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                        descriptionAttributes.Add(GetValidity(order.ValidityTime));
                    }
                    else
                    {
                        activityType = ActivityType.OrderActivation;
                        relatedIds = new[] { order.Id };
                        descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                    }

                    break;

                case OrderHistoryTypeContract.Change:

                    relatedIds = new[] { order.Id };

                    descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));

                    activityType = MapOrderChangeToActivityType(
                        order,
                        historyEvent.ActivitiesMetadata,
                        descriptionAttributes);

                    if (activityType == ActivityType.None)
                        return Task.CompletedTask;

                    break;

                case OrderHistoryTypeContract.Reject:

                    relatedIds = new[] { order.Id };
                    descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                    activityType = MapRejectReasonToActivityType(order);

                    break;

                case OrderHistoryTypeContract.Cancel:

                    relatedIds = new[] { order.Id };
                    descriptionAttributes.AddRange(GetCommonDescriptionAttributesForOrder(order));
                    activityType = MapOrderCancelToActivityType(historyEvent.ActivitiesMetadata);

                    break;

                case OrderHistoryTypeContract.Executed:

                    relatedIds = new[] { order.Id, order.Id };
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
            return GetCommonDescriptionAttributesForOrder(
                _assetsCache.GetAsset, _assetPairsCacheService.TryGetAssetPair,
                order.AssetPairId, order.Direction, order.Type, order.Volume, order.Status, order.ExecutionPrice,
                order.ExpectedOpenPrice);
        }

        public static List<string> GetCommonDescriptionAttributesForOrder(
            Func<string, Asset> getAssetFunc, Func<string, AssetPairContract> getAssetPair,
            string assetPairId, OrderDirectionContract direction, OrderTypeContract type, decimal? volume,
            OrderStatusContract status, decimal? executionPrice, decimal? expectedOpenPrice)
        {
            var asset = getAssetFunc(assetPairId);
            var assetPair = getAssetPair(assetPairId);

            var result = new List<string>
            {
                direction.ToString(),
                type.ToString(),
                volume.ToUiString(0),
                asset?.Name ?? assetPairId,
            };

            if (type == OrderTypeContract.Market)
            {
                if (status == OrderStatusContract.Executed)
                {
                    result.AddRange(new[]
                    {
                        executionPrice.ToUiString(asset?.Accuracy),
                        assetPair?.QuoteAssetId
                    });
                }
            }
            else
            {

                if (status != OrderStatusContract.Rejected)
                {
                    result.AddRange(new[]
                    {
                        expectedOpenPrice.ToUiString(asset?.Accuracy),
                        assetPair?.QuoteAssetId
                    });
                }

                if (status == OrderStatusContract.Executed)
                {
                    result.AddRange(new[]
                    {
                        executionPrice.ToUiString(asset?.Accuracy),
                        assetPair?.QuoteAssetId
                    });
                }
            }

            return result;
        }

        private void HandleCancellationAndAdjustment(OrderHistoryEvent historyEvent, OrderContract order)
        {
            if (string.IsNullOrEmpty(order.AdditionalInfo))
                return;

            var additionalAttributes = new
            {
                TargetTradeId = "",
                IsCancellationTrade = false,
                IsAdjustmentTrade = false
            };
            try
            {
                additionalAttributes = JsonConvert.DeserializeAnonymousType(
                    order.AdditionalInfo,
                    additionalAttributes);
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to parse AdditionalInfo from order: [{order.ToJson()}]", exception);
            }

            if (additionalAttributes.IsCancellationTrade)
            {
                var activityType = ActivityType.CancellationTrade;
                var descriptionAttributes = GetCommonDescriptionAttributesForOrder(order);
                descriptionAttributes.Add(additionalAttributes.TargetTradeId);
                var relatedIds = new[] { order.Id, order.Id };

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
                var relatedIds = new[] { order.Id, order.Id };

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


        #region Mappers

        private static ActivityType MapRejectReasonToActivityType(OrderContract order)
        {
            switch (order.RejectReason)
            {
                case OrderRejectReasonContract.InvalidVolume:
                    return ActivityType.OrderRejection; //any other

                case OrderRejectReasonContract.ShortPositionsDisabled:
                    return ActivityType.OrderRejectionBecauseShortDisabled;

                case OrderRejectReasonContract.MaxPositionLimit:
                    return ActivityType.OrderRejectionBecauseMaxPositionLimit;

                case OrderRejectReasonContract.NotEnoughBalance:
                    return ActivityType.OrderRejectionBecauseNotSufficientCapital;

                case OrderRejectReasonContract.NoLiquidity:
                    return ActivityType.OrderRejectionBecauseNoLiquidity;

                case OrderRejectReasonContract.MinOrderSizeLimit:
                    return ActivityType.OrderRejectionBecauseMinOrderSizeLimit;

                case OrderRejectReasonContract.MaxOrderSizeLimit:
                    return ActivityType.OrderRejectionBecauseMaxOrderSizeLimit;

                default:
                    return ActivityType.OrderRejection;
            }
        }

        private static ActivityType MapOrderChangeToActivityType(OrderContract order,
            string metadata,
            List<string> descriptionAttributes)
        {
            var metadataObject = metadata.DeserializeJson<OrderChangedMetadata>();
            descriptionAttributes.Add(metadataObject.OldValue);

            switch (metadataObject.UpdatedProperty)
            {
                case OrderChangedProperty.Price:
                    descriptionAttributes.Add(order.ExpectedOpenPrice.ToString());
                    return ActivityType.OrderModificationPrice;

                case OrderChangedProperty.Volume:
                    descriptionAttributes.Add(order.Volume.ToString());
                    return ActivityType.OrderModificationVolume;

                case OrderChangedProperty.Validity:
                    descriptionAttributes.Add(GetValidity(order.ValidityTime));
                    return ActivityType.OrderModificationValidity;

                case OrderChangedProperty.RelatedOrderRemoved:
                    return ActivityType.OrderModificationRelatedOrderRemoved;

                case OrderChangedProperty.ForceOpen:
                    descriptionAttributes.Add(order.ForceOpen.ToString());
                    return ActivityType.OrderModificationForceOpen;


                case OrderChangedProperty.None:
                    return ActivityType.None;

                default:
                    return ActivityType.OrderModification;
            }
        }

        private static ActivityType MapOrderCancelToActivityType(string metadata)
        {
            var metadataObject = metadata.DeserializeJson<OrderCancelledMetadata>();

            switch (metadataObject.Reason)
            {
                case OrderCancellationReasonContract.Expired:
                    return ActivityType.OrderExpiry;

                case OrderCancellationReasonContract.CorporateAction:
                    return ActivityType.OrderCancellationBecauseCorporateAction;

                case OrderCancellationReasonContract.AccountInactivated:
                    return ActivityType.OrderCancellationBecauseAccountIsNotValid;

                case OrderCancellationReasonContract.InstrumentInvalidated:
                    return ActivityType.OrderCancellationBecauseInstrumentInNotValid;

                case OrderCancellationReasonContract.BaseOrderCancelled:
                    return ActivityType.OrderCancellationBecauseBaseOrderCancelled;

                case OrderCancellationReasonContract.ParentPositionClosed:
                    return ActivityType.OrderCancellationBecausePositionClosed;

                case OrderCancellationReasonContract.ConnectedOrderExecuted:
                    return ActivityType.OrderCancellationBecauseConnectedOrderExecuted;

                default:
                    return ActivityType.OrderCancellation;
            }
        }

        #endregion

    }
}
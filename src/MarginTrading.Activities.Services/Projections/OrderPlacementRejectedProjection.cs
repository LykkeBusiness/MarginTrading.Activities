// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.TradeMonitoring;
using OrderStatusContract = MarginTrading.Backend.Contracts.Orders.OrderStatusContract;

namespace MarginTrading.Activities.Services.Projections
{
    /// <summary>
    /// Listens to <see cref="OrderPlacementRejectedEvent"/>s and builds a projection.
    /// </summary>
    [UsedImplicitly]
    public class OrderPlacementRejectedProjection
    {
        private readonly IAssetPairsCacheService _assetPairsCacheService;
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly ILog _log;

        public OrderPlacementRejectedProjection(
            IAssetPairsCacheService assetPairsCacheService,
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            ILog log)
        {
            _assetPairsCacheService = assetPairsCacheService;
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
            _log = log;
        }

        [UsedImplicitly]
        public async Task Handle(OrderPlacementRejectedEvent @event)
        {
            if (@event.OrderPlaceRequest == null)
            {
                await _log.WriteErrorAsync(nameof(OrderPlacementRejectedProjection), nameof(Handle),
                    new Exception("OrderPlacementRejectedEvent have null OrderPlaceRequest."));
                return;
            }

            var commonDescriptionAttributes = OrdersProjection.GetCommonDescriptionAttributesForOrder(
                _assetPairsCacheService.TryGetAssetPair, @event.OrderPlaceRequest.InstrumentId,
                @event.OrderPlaceRequest.Direction, @event.OrderPlaceRequest.Type, @event.OrderPlaceRequest.Volume,
                OrderStatusContract.Rejected, null, null);

            _cqrsSender.PublishActivity(new Activity(
                _identityGenerator.GenerateId(),
                @event.OrderPlaceRequest.AccountId,
                @event.OrderPlaceRequest.InstrumentId,
                eventSourceId: @event.CorrelationId,
                @event.EventTimestamp,
                MapType(@event.RejectReason),
                descriptionAttributes: commonDescriptionAttributes.ToArray(),
                relatedIds: Array.Empty<string>()));
        }

        private static ActivityType MapType(OrderRejectReasonContract eventRejectReason)
        {
            switch (eventRejectReason)
            {
                case OrderRejectReasonContract.NoLiquidity:
                    return ActivityType.OrderRejectionBecauseNoLiquidity;
                case OrderRejectReasonContract.NotEnoughBalance:
                    return ActivityType.OrderRejectionBecauseNotSufficientCapital;
                case OrderRejectReasonContract.ShortPositionsDisabled:
                    return ActivityType.OrderRejectionBecauseShortDisabled;
                case OrderRejectReasonContract.MaxPositionLimit:
                    return ActivityType.OrderRejectionBecauseMaxPositionLimit;
                case OrderRejectReasonContract.MinOrderSizeLimit:
                    return ActivityType.OrderRejectionBecauseMinOrderSizeLimit;
                case OrderRejectReasonContract.MaxOrderSizeLimit:
                    return ActivityType.OrderRejectionBecauseMaxOrderSizeLimit;
                default:
                    return ActivityType.OrderRejection;
            }
        }
    }
}
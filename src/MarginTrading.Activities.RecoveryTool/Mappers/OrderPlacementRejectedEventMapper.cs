// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.RecoveryTool.Model;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Activities.Services.MessageHandlers;
using MarginTrading.Activities.Services.Projections;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.TradeMonitoring;
using Newtonsoft.Json;
using OrderStatusContract = MarginTrading.Backend.Contracts.Orders.OrderStatusContract;

namespace MarginTrading.Activities.RecoveryTool.Mappers
{
    public class OrderPlacementRejectedEventMapper : IActivityMapper
    {
        private readonly IIdentityGenerator _identityGenerator;
        private readonly IAssetPairsCacheService _assetPairsCacheService;

        public OrderPlacementRejectedEventMapper(IIdentityGenerator identityGenerator,
            IAssetPairsCacheService assetPairsCacheService)
        {
            _identityGenerator = identityGenerator;
            _assetPairsCacheService = assetPairsCacheService;
        }

        public Task<List<IActivity>> Map(DomainEvent domainEvent)
        {
            if (string.IsNullOrEmpty(domainEvent?.Json))
                return Task.FromResult(new List<IActivity>());

            var @event = JsonConvert.DeserializeObject<OrderPlacementRejectedEvent>(domainEvent.Json);

            if (@event == null || @event.OrderPlaceRequest == null)
                return Task.FromResult(new List<IActivity>());

            var commonDescriptionAttributes = OrdersHistoryHandler.GetCommonDescriptionAttributesForOrder(
                _assetPairsCacheService.TryGetAssetPair, @event.OrderPlaceRequest.InstrumentId,
                @event.OrderPlaceRequest.Direction, @event.OrderPlaceRequest.Type, @event.OrderPlaceRequest.Volume,
                OrderStatusContract.Rejected, null, null);
            
            var activity = new Activity(
                _identityGenerator.GenerateId(),
                @event.OrderPlaceRequest.AccountId,
                @event.OrderPlaceRequest.InstrumentId,
                eventSourceId: @event.CorrelationIdDeprecated ?? _identityGenerator.GenerateId(),
                @event.EventTimestamp,
                MapType(@event.RejectReason),
                descriptionAttributes: commonDescriptionAttributes.ToArray(),
                relatedIds: Array.Empty<string>());

            return Task.FromResult(new List<IActivity> {activity});
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
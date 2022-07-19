// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.RecoveryTool.Mappers;
using MarginTrading.Activities.RecoveryTool.Model;
using Microsoft.Extensions.Logging;

namespace MarginTrading.Activities.RecoveryTool
{
    public class ActivityMapper
    {
        private readonly AccountChangedEventMapper _accountChangedEventMapper;
        private readonly LiquidationStartedEventMapper _liquidationStartedEventMapper;
        private readonly LiquidationResumedEventMapper _liquidationResumedEventMapper;
        private readonly LiquidationFailedEventMapper _liquidationFailedEventMapper;
        private readonly LiquidationFinishedEventMapper _liquidationFinishedEventMapper;
        private readonly MarginEventMessageMapper _marginEventMessageMapper;
        private readonly OrderPlacementRejectedEventMapper _orderPlacementRejectedEventMapper;
        private readonly OrderHistoryEventMapper _orderHistoryEventMapper;
        private readonly PositionHistoryEventMapper _positionHistoryEventMapper;
        private readonly ILogger<ActivityMapper> _logger;

        public ActivityMapper(AccountChangedEventMapper accountChangedEventMapper,
            LiquidationStartedEventMapper liquidationStartedEventMapper,
            LiquidationResumedEventMapper liquidationResumedEventMapper,
            LiquidationFailedEventMapper liquidationFailedEventMapper,
            LiquidationFinishedEventMapper liquidationFinishedEventMapper,
            MarginEventMessageMapper marginEventMessageMapper,
            OrderPlacementRejectedEventMapper orderPlacementRejectedEventMapper,
            OrderHistoryEventMapper orderHistoryEventMapper,
            PositionHistoryEventMapper positionHistoryEventMapper,
            ILogger<ActivityMapper> logger)
        {
            _accountChangedEventMapper = accountChangedEventMapper;
            _liquidationStartedEventMapper = liquidationStartedEventMapper;
            _liquidationResumedEventMapper = liquidationResumedEventMapper;
            _liquidationFailedEventMapper = liquidationFailedEventMapper;
            _liquidationFinishedEventMapper = liquidationFinishedEventMapper;
            _marginEventMessageMapper = marginEventMessageMapper;
            _orderPlacementRejectedEventMapper = orderPlacementRejectedEventMapper;
            _orderHistoryEventMapper = orderHistoryEventMapper;
            _positionHistoryEventMapper = positionHistoryEventMapper;
            _logger = logger;
        }

        public async Task<List<IActivity>> Map(DomainEvent @event)
        {
            switch (@event.Type)
            {
                case EventType.None:
                    throw new ArgumentOutOfRangeException("Unknown event type");
                    break;
                case EventType.AccountChangedEvent:
                    return await _accountChangedEventMapper.Map(@event);
                    break;
                case EventType.LiquidationStartedEvent:
                    return await _liquidationStartedEventMapper.Map(@event);
                    break;
                case EventType.LiquidationResumedEvent:
                    return await _liquidationResumedEventMapper.Map(@event);
                    break;
                case EventType.LiquidationFailedEvent:
                    return await _liquidationFailedEventMapper.Map(@event);
                    break;
                case EventType.LiquidationFinishedEvent:
                    return await _liquidationFinishedEventMapper.Map(@event);
                    break;
                case EventType.MarginEventMessage:
                    return await _marginEventMessageMapper.Map(@event);
                    break;
                case EventType.OrderPlacementRejectedEvent:
                    return await _orderPlacementRejectedEventMapper.Map(@event);
                    break;
                case EventType.OrderHistoryEvent:
                    return await _orderHistoryEventMapper.Map(@event);
                    break;
                case EventType.PositionHistoryEvent:
                    return await _positionHistoryEventMapper.Map(@event);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown event type");
            }
        }
    }
}
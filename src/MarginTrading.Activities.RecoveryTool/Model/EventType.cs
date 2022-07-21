// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.Activities.RecoveryTool.Model
{
    public enum EventType
    {
        None,
        AccountChangedEvent,
        
        LiquidationStartedEvent,
        LiquidationResumedEvent,
        LiquidationFailedEvent,
        LiquidationFinishedEvent,
        
        MarginEventMessage,
        
        OrderPlacementRejectedEvent,
        
        OrderHistoryEvent,
        
        PositionHistoryEvent,
        // not possible to restore from logs
        // SessionActivity,
    }
}
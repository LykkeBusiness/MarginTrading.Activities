namespace MarginTrading.Activities.Core.Domain
{
    public enum ActivityType
    {
        None = 0,
        
        //Order
        OrderAcceptance = 1010,
        OrderActivation = 1020,
        OrderAcceptanceAndActivation = 1030,
        OrderModification = 1040,
        OrderModificationPrice = 1041,
        OrderModificationVolume = 1042,
        OrderModificationRelatedOrderRemoved = 1043,
        OrderModificationValidity = 1044,
        OrderRejection = 1050,
        OrderRejectionBecauseShortDisabled = 1051,
        OrderRejectionBecauseMaxPositionLimit = 1052,
        OrderRejectionBecauseNotSufficientCapital = 1053,
        OrderRejectionBecauseNoLiquidity = 1054,
        OrderExpiry = 1060,
        OrderCancellation = 1070,
        OrderCancellationBecauseBaseOrderCancelled = 1071,
        OrderCancellationBecausePositionClosed = 1072,
        OrderCancellationBecauseConnectedOrderExecuted = 1073,
        OrderCancellationBecauseCorporateAction = 1074,
        OrderCancellationBecauseInstrumentInNotValid = 1075,
        OrderCancellationBecauseAccountIsNotValid = 1076,
        OrderAcceptanceAndExecution = 1080,
        OrderExecution = 1090,
     
        //Position
        PositionOpening = 2010,
        PositionIncrease = 2020,
        PositionValuation = 2030,
        PositionPartialClosing = 2040,
        PositionClosing = 2050,
        
        //Adjustment
        CancellationTrade = 3010,
        AdjustmentTrade = 3020,
        
        //MarginControl
        MarginСall1 = 4010,
        MarginСall2 = 4020,
        Liquidation = 4030,
        
        //Account
        AccountCreation = 5010, 
        AccountActivation = 5020, //???
        AccountTradingDisabled = 5030,
        AccountTradingEnabled = 5040,
        AccountWithdrawalDisabled = 5050,
        AccountWithdrawalEnabled = 5060,
        AccountClosing = 5070,
        AccountReopening = 5080,
        
        //Session
        SessionLogIn = 6010,
        SessionSupportLogIn = 6011,
        SessionTimeOutTermination = 6020,
        SessionDifferentDeviceTermination = 6021,
        SessionManualTermination = 6022,
        SessionSignOut = 6030,
        SessionSupportSignOut = 6031,
        SessionSwitchedToOnBehalfTrading = 6040,
        SessionConnectedByOnBehalfInvestor = 6050,
        SessionConnectedByOnBehalfSupport = 6060,
        SessionDisconnectedByOnBehalfInvestor = 6070,
        SessionDisconnectedByOnBehalfSupport = 6080,
        SessionSwitchedToOwnAssetAccount = 6090,

        //Settings
        SettingsAny = 7000,
        SettingsChangedAdvancedMenuOrderEntryConfirmation = 7011,
        SettingsChangedAdvancedMenuGraphicalTrading = 7012,
        SettingsChangedAdvancedMenuDarkMode = 7013,
        SettingsChangedAdvancedMenuOneClickTrading = 7014,
        SettingsChangedGeneralAppearance = 7021,
        SettingsChangedGeneralMenuBarDecimalPlaces = 7031,
        SettingsChangedGeneralMenuBarAccountBalance = 7032,
        SettingsChangedGeneralMenuBarUnrealizedDailyPnl = 7033,
        SettingsChangedGeneralMenuBarDailyPnl = 7034,
        SettingsChangedGeneralMenuBarTotalCapital = 7035,
        SettingsChangedGeneralMenuBarMargin = 7036,
        SettingsChangedGeneralMenuBarFreeCapital = 7037,
        SettingsChangedGeneralMenuBarMarginPerc = 7038,
        SettingsChangedGeneralOrderSettingsMinMaxQuantity = 7041,
        SettingsChangedGeneralOrderSettingsConfirmationPlacing = 7051,
        SettingsChangedGeneralOrderSettingsConfirmationChanging = 7052,
        SettingsChangedGeneralOrderSettingsConfirmationCancelling = 7053,
        SettingsChangedGeneralOrderSettingsConfirmationCancellingAll = 7054,
        SettingsChangedGeneralOrderSettingsConfirmationPositionClose = 7055,
        SettingsChangedGeneralOrderSettingsConfirmationPositionCloseAll = 7056,
        SettingsChangedGeneralOrderSettingsAcknowledgementPlacing = 7061,
        SettingsChangedGeneralOrderSettingsAcknowledgementChanging = 7062,
        SettingsChangedGeneralOrderSettingsAcknowledgementCancelling = 7063,
        SettingsChangedGeneralOrderSettingsAcknowledgementCancellingAll = 7064,
        SettingsChangedGeneralOrderSettingsAcknowledgementPositionClose = 7065,
        SettingsChangedGeneralOrderSettingsAcknowledgementPositionCloseAll = 7066,
        SettingsChangedGeneralOrderSettingsDefaultParametersInstrument = 7071,
        SettingsChangedGeneralOrderSettingsDefaultParametersQuantity = 7072,
        SettingsChangedGeneralOrderSettingsDefaultParametersTakeProfit = 7073,
        SettingsChangedGeneralOrderSettingsDefaultParametersStopLoss = 7074,
        SettingsChangedGeneralOrderSettingsDefaultParametersTrailingStop = 7075,
        SettingsChangedGeneralOrderSettingsDefaultParametersForcedOpen = 7076,
        SettingsChangedTabRenamedNews = 7081,
        SettingsChangedTabRenamedActivities = 7082,
        SettingsChangedTabRenamedCashMovements = 7083,
        SettingsChangedTabRenamedOpenOrders = 7084,
        SettingsChangedTabRenamedExecutedOrders = 7085,
        SettingsChangedTabRenamedOtherOrders = 7086,
        SettingsChangedTabRenamedOpenPositions = 7087,
        SettingsChangedTabRenamedClosedPositions = 7088,
        SettingsChangedTabRenamedWatchlists = 7089,


    }
}
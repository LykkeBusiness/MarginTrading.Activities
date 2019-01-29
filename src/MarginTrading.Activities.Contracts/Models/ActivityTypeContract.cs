using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.MarginTrading.Activities.Contracts.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActivityTypeContract
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
        SessionTermination = 6020,
        SessionSignOut = 6030,
        SessionSwitchedToOnBehalfTrading = 6040,
        SessionConnectedByOnBehalfInvestor = 6050,
        SessionConnectedByOnBehalfSupport = 6060,
        SessionDisconnectedByOnBehalfInvestor = 6070,
        SessionDisconnectedByOnBehalfSupport = 6080,
        SessionSwitchedToOwnAssetAccount = 6090,

        //Settings
        SettingsAny = 7000
    }
}
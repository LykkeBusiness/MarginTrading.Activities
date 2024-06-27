using MarginTrading.Activities.Services.MessageHandlers;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.Backend.Contracts.Positions;

using NUnit.Framework;

namespace MarginTrading.Activities.Tests;

public class PositionsProjectionTests
{
    [Test]
    [TestCase(PositionHistoryTypeContract.Open, OriginatorTypeContract.Investor, null, false)]
    [TestCase(PositionHistoryTypeContract.Open, OriginatorTypeContract.OnBehalf, null, true)]
    [TestCase(PositionHistoryTypeContract.Open, OriginatorTypeContract.System, null, false)]
    [TestCase(PositionHistoryTypeContract.Close, OriginatorTypeContract.Investor, OriginatorTypeContract.Investor, false)]
    [TestCase(PositionHistoryTypeContract.Close, OriginatorTypeContract.Investor, OriginatorTypeContract.OnBehalf, true)]
    [TestCase(PositionHistoryTypeContract.Close, OriginatorTypeContract.Investor, OriginatorTypeContract.System, false)]
    [TestCase(PositionHistoryTypeContract.Close, OriginatorTypeContract.OnBehalf, OriginatorTypeContract.Investor, false)]
    [TestCase(PositionHistoryTypeContract.Close, OriginatorTypeContract.OnBehalf, OriginatorTypeContract.OnBehalf, true)]
    [TestCase(PositionHistoryTypeContract.Close, OriginatorTypeContract.OnBehalf, OriginatorTypeContract.System, false)]
    [TestCase(PositionHistoryTypeContract.Close, OriginatorTypeContract.System, OriginatorTypeContract.Investor, false)]
    [TestCase(PositionHistoryTypeContract.Close, OriginatorTypeContract.System, OriginatorTypeContract.OnBehalf, true)]
    [TestCase(PositionHistoryTypeContract.Close, OriginatorTypeContract.System, OriginatorTypeContract.System, false)]
    public void CheckIfOnBehalf_ShouldReturnCorrectValues_BasedOnOriginator(
        PositionHistoryTypeContract historyType,
        OriginatorTypeContract openOriginator, 
        OriginatorTypeContract? closeOriginator, 
        bool expectedResult)
    {
        var positionHistoryEvent = new PositionHistoryEvent
        {
            EventType = historyType,
            PositionSnapshot = new PositionContract
            {
                OpenOriginator = openOriginator,
                CloseOriginator = closeOriginator
            }
        };
            
        var result = PositionsHistoryHandler.CheckIfOnBehalf(positionHistoryEvent);
            
        Assert.AreEqual(expectedResult, result);
    }
}
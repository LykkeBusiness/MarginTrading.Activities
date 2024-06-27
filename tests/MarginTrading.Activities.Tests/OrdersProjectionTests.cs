using MarginTrading.Activities.Services.MessageHandlers;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;

using NUnit.Framework;

namespace MarginTrading.Activities.Tests;

public class OrdersProjectionTests
{
    [Test]
    public void CheckIfOnBehalf_ShouldReturnFalse_IfInputIsNotJsonString()
    {
        var orderHistoryEvent = new OrderHistoryEvent
        {
            OrderSnapshot = new OrderContract
            {
                AdditionalInfo = "not-json-string"
            }
        };

        var result = OrdersHistoryHandler.CheckIfOnBehalf(orderHistoryEvent);

        Assert.False(result);
    }

    [Test]
    public void CheckIfOnBehalf_ShouldReturnFalse_IfInputIsEmptyObject()
    {
        var orderHistoryEvent = new OrderHistoryEvent
        {
            OrderSnapshot = new OrderContract
            {
                AdditionalInfo = "{}"
            }
        };

        var result = OrdersHistoryHandler.CheckIfOnBehalf(orderHistoryEvent);

        Assert.False(result);
    }

    [Test]
    public void CheckIfOnBehalf_ShouldReturnFalse_IfIsOnBehalfPropertyDoesntExist()
    {
        var orderHistoryEvent = new OrderHistoryEvent
        {
            OrderSnapshot = new OrderContract
            {
                AdditionalInfo = @"{""CreatedBy"": ""user1"", ""CreatedAt"": ""2023-04-26T09:44""}"
            }
        };

        var result = OrdersHistoryHandler.CheckIfOnBehalf(orderHistoryEvent);

        Assert.False(result);
    }

    [Test]
    public void CheckIfOnBehalf_ShouldReturnFalse_IfIsOnBehalfPropertyInvalid()
    {
        var orderHistoryEvent = new OrderHistoryEvent
        {
            OrderSnapshot = new OrderContract
            {
                AdditionalInfo = @"{""CreatedBy"": ""user1"", ""CreatedAt"": ""2023-04-26T09:44"", ""IsOnBehalf"": """"}"
            }
        };

        var result = OrdersHistoryHandler.CheckIfOnBehalf(orderHistoryEvent);

        Assert.False(result);
    }

    [Test]
    public void CheckIfOnBehalf_ShouldReturnFalse_IfIsOnBehalfPropertySetToFalse()
    {
        var orderHistoryEvent = new OrderHistoryEvent
        {
            OrderSnapshot = new OrderContract
            {
                AdditionalInfo = @"{""CreatedBy"": ""user1"", ""CreatedAt"": ""2023-04-26T09:44"", ""IsOnBehalf"": false}"
            }
        };

        var result = OrdersHistoryHandler.CheckIfOnBehalf(orderHistoryEvent);

        Assert.False(result);
    }

    [Test]
    public void CheckIfOnBehalf_ShouldReturnTrue_IfIsOnBehalfPropertySetToTrue()
    {
        var orderHistoryEvent = new OrderHistoryEvent
        {
            OrderSnapshot = new OrderContract
            {
                AdditionalInfo = @"{""CreatedBy"": ""user1"", ""CreatedAt"": ""2023-04-26T09:44"", ""IsOnBehalf"": true}"
            }
        };

        var result = OrdersHistoryHandler.CheckIfOnBehalf(orderHistoryEvent);

        Assert.True(result);
    }
}
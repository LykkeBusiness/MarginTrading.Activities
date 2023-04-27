using Common.Log;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Activities.Services.Projections;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.Backend.Contracts.Positions;
using Moq;
using NUnit.Framework;

namespace MarginTrading.Activites.Tests
{
    public class PositionsProjectionTests
    {
        [Test]
        public void CheckIfOnBehalf_ShouldReturnFalse_IfInputIsNotJsonString()
        {
            var positionHistoryEvent = new PositionHistoryEvent
            {
                PositionSnapshot = new PositionContract
                {
                    AdditionalInfo = "not-json-string"
                }
            };
            
            var sut = CreateSut();
            
            var result = sut.CheckIfOnBehalf(positionHistoryEvent);
            
            Assert.False(result);
        }

        [Test]
        public void CheckIfOnBehalf_ShouldReturnFalse_IfInputIsEmptyObject()
        {
            var positionHistoryEvent = new PositionHistoryEvent
            {
                PositionSnapshot = new PositionContract
                {
                    AdditionalInfo = "{}"
                }
            };
            
            var sut = CreateSut();
            
            var result = sut.CheckIfOnBehalf(positionHistoryEvent);
            
            Assert.False(result);
        }

        [Test]
        public void CheckIfOnBehalf_ShouldReturnFalse_IfIsOnBehalfPropertyDoesntExist()
        {
            var positionHistoryEvent = new PositionHistoryEvent
            {
                PositionSnapshot = new PositionContract
                {
                    AdditionalInfo = @"{""CreatedBy"": ""user1"", ""CreatedAt"": ""2023-04-26T09:44""}"
                }
            };
            
            var sut = CreateSut();
            
            var result = sut.CheckIfOnBehalf(positionHistoryEvent);
            
            Assert.False(result);
        }

        [Test]
        public void CheckIfOnBehalf_ShouldReturnFalse_IfIsOnBehalfPropertyInvalid()
        {
            var positionHistoryEvent = new PositionHistoryEvent
            {
                PositionSnapshot = new PositionContract
                {
                    AdditionalInfo = @"{""CreatedBy"": ""user1"", ""CreatedAt"": ""2023-04-26T09:44"", ""IsOnBehalf"": """"}"
                }
            };
            
            var sut = CreateSut();
            
            var result = sut.CheckIfOnBehalf(positionHistoryEvent);
            
            Assert.False(result);
        }

        [Test]
        public void CheckIfOnBehalf_ShouldReturnFalse_IfIsOnBehalfPropertySetToFalse()
        {
            var positionHistoryEvent = new PositionHistoryEvent
            {
                PositionSnapshot = new PositionContract
                {
                    AdditionalInfo = @"{""CreatedBy"": ""user1"", ""CreatedAt"": ""2023-04-26T09:44"", ""IsOnBehalf"": false}"
                }
            };
            
            var sut = CreateSut();
            
            var result = sut.CheckIfOnBehalf(positionHistoryEvent);
            
            Assert.False(result);
        }

        [Test]
        public void CheckIfOnBehalf_ShouldReturnTrue_IfIsOnBehalfPropertySetToTrue()
        {
            var positionHistoryEvent = new PositionHistoryEvent
            {
                PositionSnapshot = new PositionContract
                {
                    AdditionalInfo = @"{""CreatedBy"": ""user1"", ""CreatedAt"": ""2023-04-26T09:44"", ""IsOnBehalf"": true}"
                }
            };
            
            var sut = CreateSut();
            
            var result = sut.CheckIfOnBehalf(positionHistoryEvent);
            
            Assert.True(result);
        }

        private PositionsProjection CreateSut()
        {
            var mockRabbitMqSubscriberService = new Mock<IRabbitMqSubscriberService>();
            var mockActivitiesSender = new Mock<IActivitiesSender>();
            var mockIdentityGenerator = new Mock<IIdentityGenerator>();
            var mockLog = new Mock<ILog>();
            var mockAssetPairCacheService = new Mock<IAssetPairsCacheService>();
            
            return new PositionsProjection(
                mockRabbitMqSubscriberService.Object,
                new ActivitiesSettings(),
                mockActivitiesSender.Object,
                mockIdentityGenerator.Object,
                mockLog.Object,
                mockAssetPairCacheService.Object);
        }
    }
}
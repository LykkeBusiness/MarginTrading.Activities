using Common.Log;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Activities.Services.Projections;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using Moq;
using NUnit.Framework;

namespace MarginTrading.Activites.Tests
{
    public class OrdersProjectionTests
    {
        [Test]
        [TestCase(OriginatorTypeContract.Investor, false)]
        [TestCase(OriginatorTypeContract.OnBehalf, true)]
        [TestCase(OriginatorTypeContract.System, false)]
        public void CheckIfOnBehalf_ShouldReturnTheCorrectValue_BasedOnOriginator(OriginatorTypeContract originator,
            bool expectedResult)
        {
            var orderHistoryEvent = new OrderHistoryEvent
            {
                OrderSnapshot = new OrderContract
                {
                    Originator = originator
                }
            };
            
            var sut = CreateSut();
            
            var result = sut.CheckIfOnBehalf(orderHistoryEvent);
            
            Assert.AreEqual(expectedResult, result);
        }
        
        private OrdersProjection CreateSut()
        {
            var mockRabbitMqSubscriberService = new Mock<IRabbitMqSubscriberService>();
            var mockActivitiesSender = new Mock<IActivitiesSender>();
            var mockIdentityGenerator = new Mock<IIdentityGenerator>();
            var mockLog = new Mock<ILog>();
            var mockAssetPairCacheService = new Mock<IAssetPairsCacheService>();
            
            return new OrdersProjection(
                mockRabbitMqSubscriberService.Object,
                new ActivitiesSettings(),
                mockActivitiesSender.Object,
                mockIdentityGenerator.Object,
                mockLog.Object,
                mockAssetPairCacheService.Object);
        }
    }
}
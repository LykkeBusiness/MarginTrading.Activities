using System;
using System.Collections.Generic;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Activities.Services.Projections;
using Moq;
using NUnit.Framework;

namespace MarginTrading.Activites.Tests
{
    public class CashMovementProjectionTests
    {
        private Mock<IActivitiesSender> _activitySenderMock;
        private Mock<IIdentityGenerator> _identityGeneratorMock;

        private static DepositSucceededEvent DepositSucceededEvent = new DepositSucceededEvent(Guid.NewGuid().ToString(), DateTime.UtcNow, "", "", 55.2m);
        private static IEnumerable<TestCaseData> DepositSucceededTestCase
        {
            get 
            {
                yield return new TestCaseData(DepositSucceededEvent);
            }
        }

        [SetUp]
        public void Setup()
        {
            _activitySenderMock = new Mock<IActivitiesSender>();
            _identityGeneratorMock = new Mock<IIdentityGenerator>();
        }

        [Test, TestCaseSource(nameof(DepositSucceededTestCase))]
        public void DepositSucceededEvent_ShouldResultIn_PublishingAccurateActivity(DepositSucceededEvent e)
        {
            var activityId = "activityId1";
            _identityGeneratorMock.Setup(o => o.GenerateId()).Returns(activityId);
            
            var projection = CreateSut(_activitySenderMock.Object, _identityGeneratorMock.Object);
            projection.Handle(e);
            
            _activitySenderMock.Verify(m => m.PublishActivity(
                 It.Is<IActivity>(
                    a => a.AccountId == e.AccountId &&
                    a.Id == activityId &&
                    a.Instrument == string.Empty &&
                    a.EventSourceId == e.AccountId &&
                    a.Timestamp == e.EventTimestamp && 
                    a.Event == Activities.Core.Domain.ActivityType.AccountDepositSucceeded && 
                    a.DescriptionAttributes[0] == e.Amount.ToString())));
        }
        
        private CashMovementProjection CreateSut(IActivitiesSender activitySender, IIdentityGenerator identityGenerator)
        {
            return new CashMovementProjection(activitySender, identityGenerator);
        }
    }
}
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

        private static DepositSucceededEvent DepositSucceededEvent = 
            new DepositSucceededEvent(Guid.NewGuid().ToString(), DateTime.UtcNow, "clientId1", "accountId1", 55.2m);

        private static DepositFailedEvent DepositFailedEvent = 
            new DepositFailedEvent(Guid.NewGuid().ToString(), DateTime.UtcNow, "clientId2", "accountId2", 1000.0m);

        private static WithdrawalSucceededEvent WithdrawalSucceededEvent = 
            new WithdrawalSucceededEvent(Guid.NewGuid().ToString(), DateTime.UtcNow, "clientId3", "accountId3", 500.55m);

        private static WithdrawalFailedEvent WithdrawalFailedEvent = 
            new WithdrawalFailedEvent(Guid.NewGuid().ToString(), DateTime.UtcNow, "reason", "accountId4", "clientId4", 650.30m);

        private static IEnumerable<TestCaseData> DepositSucceededTestCase
        {
            get 
            {
                yield return new TestCaseData(DepositSucceededEvent);
            }
        }

        private static IEnumerable<TestCaseData> DepositFailedTestCase
        {
            get 
            {
                yield return new TestCaseData(DepositFailedEvent);
            }
        }

        private static IEnumerable<TestCaseData> WithdrawalSucceededTestCase
        {
            get 
            {
                yield return new TestCaseData(WithdrawalSucceededEvent);
            }
        }

        private static IEnumerable<TestCaseData> WithdrawalFailedTestCase
        {
            get 
            {
                yield return new TestCaseData(WithdrawalFailedEvent);
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

        [Test, TestCaseSource(nameof(DepositFailedTestCase))]
        public void DepositFailed_ShouldResultIn_PublishingAccurateActivity(DepositFailedEvent e)
        {
            var activityId = "activityId2";
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
                    a.Event == Activities.Core.Domain.ActivityType.AccountDepositFailed && 
                    a.DescriptionAttributes[0] == e.Amount.ToString())));
        }

        [Test, TestCaseSource(nameof(WithdrawalSucceededTestCase))]
        public void WithdrawalSucceeded_ShouldResultIn_PublishingAccurateActivity(WithdrawalSucceededEvent e)
        {
            var activityId = "activityId3";
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
                    a.Event == Activities.Core.Domain.ActivityType.AccountWithdrawalSucceeded && 
                    a.DescriptionAttributes[0] == e.Amount.ToString())));
        }

        [Test, TestCaseSource(nameof(WithdrawalFailedTestCase))]
        public void WithdrawalFailed_ShouldResultIn_PublishingAccurateActivity(WithdrawalFailedEvent e)
        {
            var activityId = "activityId4";
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
                    a.Event == Activities.Core.Domain.ActivityType.AccountWithdrawalFailed && 
                    a.DescriptionAttributes[0] == e.Amount.ToString())));
        }
        
        private CashMovementProjection CreateSut(IActivitiesSender activitySender, IIdentityGenerator identityGenerator)
        {
            return new CashMovementProjection(activitySender, identityGenerator);
        }
    }
}
using System;
using System.Collections.Generic;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.Activities.Services.Projections;
using Moq;
using NUnit.Framework;

namespace MarginTrading.Activites.Tests
{
    public class CashMovementProjectionTests
    {
        private ActivitiesSenderStub _activitiesSenderStub;
        private Mock<IIdentityGenerator> _identityGeneratorMock;

        private static DepositSucceededEvent DepositSucceededEvent = 
            new DepositSucceededEvent(Guid.NewGuid().ToString(), DateTime.UtcNow, "clientId1", "accountId1", 55.2m, "EUR");

        private static DepositFailedEvent DepositFailedEvent = 
            new DepositFailedEvent(Guid.NewGuid().ToString(), DateTime.UtcNow, "clientId2", "accountId2", 1000.0m, "EUR");

        private static WithdrawalSucceededEvent WithdrawalSucceededEvent = 
            new WithdrawalSucceededEvent(Guid.NewGuid().ToString(), DateTime.UtcNow, "clientId3", "accountId3", 500.55m, "EUR");

        private static WithdrawalFailedEvent WithdrawalFailedEvent = 
            new WithdrawalFailedEvent(Guid.NewGuid().ToString(), DateTime.UtcNow, "reason", "accountId4", "clientId4", 650.30m, "EUR");

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
            _identityGeneratorMock = new Mock<IIdentityGenerator>();
            _activitiesSenderStub = new ActivitiesSenderStub();
        }

        [Test, TestCaseSource(nameof(DepositSucceededTestCase))]
        public void DepositSucceededEvent_ShouldResultIn_PublishingAccurateActivity(DepositSucceededEvent e)
        {
            var activityId = "activityId1";
            _identityGeneratorMock.Setup(o => o.GenerateId()).Returns(activityId);
            var projection = CreateSut(_identityGeneratorMock.Object);
            
            // Act
            projection.Handle(e);

            var actualActivity = _activitiesSenderStub.GetLastPublishedActivity();

            Assert.AreEqual(1, _activitiesSenderStub.Activities.Count);
            
            Assert.AreEqual(expected: e.AccountId, actual: actualActivity.AccountId);
            Assert.AreEqual(expected: activityId, actual: actualActivity.Id);
            Assert.AreEqual(expected: string.Empty, actual: actualActivity.Instrument);
            Assert.AreEqual(expected: e.AccountId, actual: actualActivity.EventSourceId);
            Assert.AreEqual(expected: e.EventTimestamp, actual: actualActivity.Timestamp);
            Assert.AreEqual(expected: ActivityType.AccountDepositSucceeded, actual: actualActivity.Event);
            Assert.AreEqual(expected: e.Amount.ToString(), actual: actualActivity.DescriptionAttributes[0]);
            
        }

        [Test, TestCaseSource(nameof(DepositFailedTestCase))]
        public void DepositFailed_ShouldResultIn_PublishingAccurateActivity(DepositFailedEvent e)
        {
            var activityId = "activityId2";
            _identityGeneratorMock.Setup(o => o.GenerateId()).Returns(activityId);
            var projection = CreateSut(_identityGeneratorMock.Object);
            
            // Act
            projection.Handle(e);
            
            var actualActivity = _activitiesSenderStub.GetLastPublishedActivity();

            Assert.AreEqual(1, _activitiesSenderStub.Activities.Count);
            
            Assert.AreEqual(expected: e.AccountId, actual: actualActivity.AccountId);
            Assert.AreEqual(expected: activityId, actual: actualActivity.Id);
            Assert.AreEqual(expected: string.Empty, actual: actualActivity.Instrument);
            Assert.AreEqual(expected: e.AccountId, actual: actualActivity.EventSourceId);
            Assert.AreEqual(expected: e.EventTimestamp, actual: actualActivity.Timestamp);
            Assert.AreEqual(expected: ActivityType.AccountDepositFailed, actual: actualActivity.Event);
            Assert.AreEqual(expected: e.Amount.ToString(), actual: actualActivity.DescriptionAttributes[0]);
        }

        [Test, TestCaseSource(nameof(WithdrawalSucceededTestCase))]
        public void WithdrawalSucceeded_ShouldResultIn_PublishingAccurateActivity(WithdrawalSucceededEvent e)
        {
            var activityId = "activityId3";
            _identityGeneratorMock.Setup(o => o.GenerateId()).Returns(activityId);
            var projection = CreateSut(_identityGeneratorMock.Object);
            
            // Act
            projection.Handle(e);

            var actualActivity = _activitiesSenderStub.GetLastPublishedActivity();

            Assert.AreEqual(1, _activitiesSenderStub.Activities.Count);
            
            Assert.AreEqual(expected: e.AccountId, actual: actualActivity.AccountId);
            Assert.AreEqual(expected: activityId, actual: actualActivity.Id);
            Assert.AreEqual(expected: string.Empty, actual: actualActivity.Instrument);
            Assert.AreEqual(expected: e.AccountId, actual: actualActivity.EventSourceId);
            Assert.AreEqual(expected: e.EventTimestamp, actual: actualActivity.Timestamp);
            Assert.AreEqual(expected: ActivityType.AccountWithdrawalSucceeded, actual: actualActivity.Event);
            Assert.AreEqual(expected: e.Amount.ToString(), actual: actualActivity.DescriptionAttributes[0]);
        }

        [Test, TestCaseSource(nameof(WithdrawalFailedTestCase))]
        public void WithdrawalFailed_ShouldResultIn_PublishingAccurateActivity(WithdrawalFailedEvent e)
        {
            var activityId = "activityId4";
            _identityGeneratorMock.Setup(o => o.GenerateId()).Returns(activityId);
            var projection = CreateSut(_identityGeneratorMock.Object);

            //Act
            projection.Handle(e);
            
            var actualActivity = _activitiesSenderStub.GetLastPublishedActivity();
            
            Assert.AreEqual(1, _activitiesSenderStub.Activities.Count);

            Assert.AreEqual(expected: e.AccountId, actual: actualActivity.AccountId);
            Assert.AreEqual(expected: activityId, actual: actualActivity.Id);
            Assert.AreEqual(expected: string.Empty, actual: actualActivity.Instrument);
            Assert.AreEqual(expected: e.AccountId, actual: actualActivity.EventSourceId);
            Assert.AreEqual(expected: e.EventTimestamp, actual: actualActivity.Timestamp);
            Assert.AreEqual(expected: ActivityType.AccountWithdrawalFailed, actual: actualActivity.Event);
            Assert.AreEqual(expected: e.Amount.ToString(), actual: actualActivity.DescriptionAttributes[0]);
        }
        
        private CashMovementProjection CreateSut(IIdentityGenerator identityGenerator, IAccountsService accountsServiceArg = null)
        {
            IAccountsService accountsService = new Mock<IAccountsService>().Object;
            
            if(accountsServiceArg != null)
            {
                accountsService = accountsServiceArg;
            }

            return new CashMovementProjection(_activitiesSenderStub, identityGenerator, accountsService);
        }
    }
}
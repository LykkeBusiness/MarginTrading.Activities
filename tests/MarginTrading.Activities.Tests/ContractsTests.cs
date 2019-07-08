// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Lykke.MarginTrading.Activities.Contracts.Models;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Extensions;
using NUnit.Framework;

namespace MarginTrading.Activities.Tests
{
    [TestFixture]
    public class ContractsTests
    {
        [Test]
        public void Test_Activities_Type_Contract()
        {
            var domainContractValues = Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>();

            foreach (var activityType in domainContractValues)
            {
                Assert.DoesNotThrow(() => activityType.ToType<ActivityTypeContract>());
            }
        }        
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.Activities.Services;
using Moq;
using NUnit.Framework;

namespace MarginTrading.Activites.Tests
{
    class AccountIdAndNameTestCase : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { };
            yield return new object[] { };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class AccountServiceTests
    {
        private static IEnumerable<TestCaseData> AccountIdAndNameTestCase
        {
            get 
            {
                yield return new TestCaseData(new AccountContract() { Id = "id-1", AccountName = "accountName-a1" }, "accountName-a1");
                yield return new TestCaseData(new AccountContract() { Id = "id-2" }, "id-2");
            }
        }

        [Test, TestCaseSource(nameof(AccountIdAndNameTestCase))]
        public async Task GetEitherAccountNameOrAccountId_ShouldReturnAccountName_WhenApplicable(AccountContract account, string expected)
        {
            var mockAccountsApi = new Mock<IAccountsApi>();
            mockAccountsApi.Setup(mock => mock.GetById(It.IsAny<string>())).ReturnsAsync(account);

            var sut = CreateSut(mockAccountsApi.Object);
            var result = await sut.GetEitherAccountNameOrAccountId(account.Id);
            
            Assert.AreEqual(expected, result);
        }
        
        private AccountService CreateSut(IAccountsApi accountsApiArg = null)
        {
            IAccountsApi accountsApi = new Mock<IAccountsApi>().Object;
            
            if(accountsApiArg != null)
            {
                accountsApi = accountsApiArg;
            }

            return new AccountService(accountsApi);
        }
    }
}
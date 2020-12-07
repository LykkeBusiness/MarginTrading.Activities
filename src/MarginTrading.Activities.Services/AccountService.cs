// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services
{
    public class AccountService : IAccountsService
    {
        private readonly IAccountsApi _accountsApi;

        public AccountService(IAccountsApi accountsApi)
        {
            _accountsApi = accountsApi;
        }

        public async Task<string> GetAccountNameByAccountId(string id)
        {
            var response = await _accountsApi.GetById(id);

            return response?.AccountName;
        }
    }
}
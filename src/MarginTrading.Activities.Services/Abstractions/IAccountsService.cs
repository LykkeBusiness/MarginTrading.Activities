// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

namespace MarginTrading.Activities.Services.Abstractions
{
    public interface IAccountsService
    {
        Task<string> GetAccountNameByAccountId(string id);
        
        /// <summary>
        /// Returns AccountName if there's an AccountName for the account, AccountId if not.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string> GetEitherAccountNameOrAccountId(string id);
    }
}
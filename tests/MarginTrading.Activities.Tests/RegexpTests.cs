// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;
using NUnit.Framework;

namespace MarginTrading.Activities.Tests
{
    public class RegexpTests
    {
        [Test]
        public void Test()
        {
            var str = @"[2022-07-08T06:02:26.3169007+02:00] [INF] [:AccountsProjection:{
  ""ChangeTimestamp"": ""2022-07-08T04:02:26.3115772Z"",
  ""Source"": ""Tax command"",
  ""Account"": {
    ""Id"": ""202222088"",
    ""ClientId"": ""7a2d031c2a1c4ed6a7e88045b95bb2d6"",
    ""TradingConditionId"": ""Default"",
    ""BaseAssetId"": ""EUR"",
    ""Balance"": 379.500000000000,
    ""WithdrawTransferLimit"": 0.000000000000,
    ""LegalEntity"": ""Default"",
    ""IsDisabled"": false,
    ""ModificationTimestamp"": ""2022-07-08T04:02:26.3115772Z"",
    ""IsWithdrawalDisabled"": false,
    ""IsDeleted"": false,
    ""AccountName"": null,
    ""AdditionalInfo"": ""{}""
  },
  ""EventType"": ""BalanceUpdated"",
  ""BalanceChange"": {
    ""Id"": ""Deal_SLE756YWWS"",
    ""ChangeTimestamp"": ""2022-07-08T04:02:26.3115772Z"",
    ""AccountId"": ""202222088"",
    ""ClientId"": ""7a2d031c2a1c4ed6a7e88045b95bb2d6"",
    ""ChangeAmount"": 0.000,
    ""Balance"": 379.500000000000,
    ""WithdrawTransferLimit"": 0.000000000000,
    ""Comment"": ""Taxes over Deal"",
    ""ReasonType"": ""Tax"",
    ""EventSourceId"": ""SLE756YWWS"",
    ""LegalEntity"": ""Default"",
    ""AuditLog"": ""{\""TotalTaxes\"":0.000,\""CapitalGainsTax\"":0.000,\""SolidaritySurcharge\"":0.000,\""ChurchTaxGroup1\"":0.000,\""ChurchTaxGroup2\"":0.000}"",
    ""Instrument"": ""CFD_US_WTISep22"",
    ""TradingDate"": ""2022-07-06T00:00:00Z""
  },
  ""OperationId"": ""Deal_SLE756YWWS"",
  ""ActivitiesMetadata"": null
}] -  AccountChangedEvent";

            var regexp = new Regex("(AccountsProjection)(.*?)(AccountChangedEvent)", RegexOptions.Singleline);

            var res = regexp.Match(str);
            var val = res.Value;
            var start = val.IndexOf('{');
            var end = val.LastIndexOf('}');
            var json = val.Substring(start, end - start + 1);
        }
    }
}
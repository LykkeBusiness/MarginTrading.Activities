// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace Lykke.MarginTrading.Activities.Contracts.Models
{
    [UsedImplicitly]
    public class LykkeErrorResponse
    {
        public string ErrorMessage { get; set; }

        public override string ToString() => ErrorMessage;
    }
}
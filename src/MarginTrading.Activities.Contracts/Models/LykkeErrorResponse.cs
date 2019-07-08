// Copyright (c) 2019 Lykke Corp.

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
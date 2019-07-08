// Copyright (c) 2019 Lykke Corp.

using JetBrains.Annotations;

namespace Lykke.MarginTrading.Activities.Contracts.Api
{
    [UsedImplicitly] // from startup.cs only in release configuration
    public class ErrorResponse
    {
        [UsedImplicitly]
        public string ErrorMessage { get; set; }

        [UsedImplicitly]
        public string Details { get; set; }
    }
}
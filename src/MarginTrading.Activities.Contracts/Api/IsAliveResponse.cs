// Copyright (c) 2019 Lykke Corp.

namespace Lykke.MarginTrading.Activities.Contracts.Api
{
    public class IsAliveResponse
    {
        public string Version { get; set; }
        public string Env { get; set; }
        public bool IsDebug { get; set; }
        public string Name { get; set; }
    }
}
﻿using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase;

namespace MarginTrading.Activities.Broker
{
    [UsedImplicitly]
    public class Program: WebAppProgramBase<Startup>
    {
        public static void Main(string[] args)
        {
            RunOnPort(5082);
        }
    }
}

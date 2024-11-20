// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.MarginTrading.BrokerBase;

namespace MarginTrading.Activities.Broker
{
    [UsedImplicitly]
    public class Program: WebAppProgramBase<Startup>
    {
        //test
        public static void Main(string[] args)
        {
            RunOnPort(5082, true);
        }
    }
}
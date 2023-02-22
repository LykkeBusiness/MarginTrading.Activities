// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.Activities.Core.Domain
{
    public enum ActivityCategory
    {
        None = 0,
        Order = 1,
        Position = 2,
        Adjustment = 3,
        MarginControl = 4,
        Account = 5,
        Session = 6,
        Settings = 7,
        CloseAll = 8,
        PriceAlert = 9,
        CashMovement = 10,
    }
}
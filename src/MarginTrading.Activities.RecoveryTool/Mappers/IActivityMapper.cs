// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.RecoveryTool.Model;

namespace MarginTrading.Activities.RecoveryTool.Mappers
{
    public interface IActivityMapper
    {
        Task<List<IActivity>> Map(DomainEvent domainEvent);
    }
}
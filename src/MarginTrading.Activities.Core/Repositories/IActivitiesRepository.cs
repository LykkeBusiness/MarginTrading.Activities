// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.Activities.Core.Domain.Abstractions;

namespace MarginTrading.Activities.Core.Repositories
{
    public interface IActivitiesRepository
    {
        Task AddAsync(IActivity activity);
    }
}
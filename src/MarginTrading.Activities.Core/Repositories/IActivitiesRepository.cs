// Copyright (c) 2019 Lykke Corp.

using System.Threading.Tasks;
using MarginTrading.Activities.Core.Domain.Abstractions;

namespace MarginTrading.Activities.Core.Repositories
{
    public interface IActivitiesRepository
    {
        Task AddAsync(IActivity activity);
    }
}
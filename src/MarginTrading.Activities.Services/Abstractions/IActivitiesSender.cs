using Lykke.MarginTrading.Activities.Contracts.Models;
using MarginTrading.Activities.Core.Domain.Abstractions;

namespace MarginTrading.Activities.Services.Abstractions
{
    public interface IActivitiesSender
    {
        void PublishActivity(IActivity activity);
    }
}
using System.Threading.Tasks;
using Common;
using MarginTrading.Activities.Core.Domain.Abstractions;

namespace MarginTrading.Activities.Services
{
    public class ActivitiesPublisher : IMessageProducer<IActivity>
    {
        public Task ProduceAsync(IActivity message)
        {
            throw new System.NotImplementedException();
        }
    }
}
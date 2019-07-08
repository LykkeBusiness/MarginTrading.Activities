// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Lykke.RabbitMqBroker.Subscriber;
using MarginTrading.Activities.Core.Settings;

namespace MarginTrading.Activities.Services.Abstractions
{
    public interface IRabbitMqSubscriberService
    {
        void Subscribe<TMessage>(RabbitMqSettings settings, bool isDurable, Func<TMessage, Task> handler,
            IMessageDeserializer<TMessage> deserializer);

        IMessageDeserializer<TMessage> GetJsonDeserializer<TMessage>();
        IMessageDeserializer<TMessage> GetMsgPackDeserializer<TMessage>();
    }
}
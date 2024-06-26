// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Axle.Contracts;

using JetBrains.Annotations;

using Lykke.RabbitMqBroker.Subscriber;

using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services.MessageHandlers
{
    [UsedImplicitly]
    internal sealed class SessionActivityHandler : IMessageHandler<SessionActivity>
    {
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly IDateService _dateService;
        private readonly IAccountsService _accountsService;

        public SessionActivityHandler(
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            IDateService dateService,
            IAccountsService accountsService)
        {
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
            _dateService = dateService;
            _accountsService = accountsService;
        }

        public async Task Handle(SessionActivity message)
        {
            ActivityType activityType;
            var descriptionAttributes = new List<string>();
            var relatedIds = Array.Empty<string>();
            var accountName = await _accountsService.GetEitherAccountNameOrAccountId(message.AccountId);

            switch (message.Type)
            {
                case SessionActivityType.Login:
                    activityType = ActivityType.SessionLogIn;
                    descriptionAttributes.AddRange(GetDescriptionForLogInLogOut(message, accountName));
                    break;
                case SessionActivityType.SignOut:
                    activityType = ActivityType.SessionSignOut;
                    descriptionAttributes.AddRange(GetDescriptionForLogInLogOut(message, accountName));
                    break;
                case SessionActivityType.TimeOut:
                    activityType = ActivityType.SessionTimeOutTermination;
                    descriptionAttributes.AddRange(GetDescriptionForTermination(message));
                    break;
                case SessionActivityType.DifferentDeviceTermination:
                    activityType = ActivityType.SessionDifferentDeviceTermination;
                    descriptionAttributes.AddRange(GetDescriptionForTermination(message));
                    break;
                case SessionActivityType.ManualTermination:
                    activityType = ActivityType.SessionManualTermination;
                    descriptionAttributes.AddRange(GetDescriptionForTermination(message));
                    break;
                case SessionActivityType.OnBehalfSupportConnected:
                    activityType = ActivityType.SessionConnectedByOnBehalfSupport;
                    descriptionAttributes.AddRange(GetDescriptionForOnBehalfSupport(message, accountName));
                    break;
                case SessionActivityType.OnBehalfSupportDisconnected:
                    activityType = ActivityType.SessionDisconnectedByOnBehalfSupport;
                    descriptionAttributes.AddRange(GetDescriptionForOnBehalfSupport(message, accountName));
                    break;
                default:
                    return;
            }

            var activity = new Activity(
                _identityGenerator.GenerateId(),
                message.AccountId,
                string.Empty,
                message.SessionId.ToString(),
                _dateService.Now(),
                activityType,
                descriptionAttributes.ToArray(),
                relatedIds
            );

            _cqrsSender.PublishActivity(activity);
        }
        
        private static IEnumerable<string> GetDescriptionForTermination(SessionActivity sessionEvent)
        {
            return new []
            {
                sessionEvent.SessionId.ToString(), sessionEvent.UserName,
            };
        }

        private static IEnumerable<string> GetDescriptionForLogInLogOut(SessionActivity sessionEvent, string accountName)
        {
            return new []
            {
                sessionEvent.UserName, accountName, sessionEvent.SessionId.ToString(),
            };
        }

        private static IEnumerable<string> GetDescriptionForOnBehalfSupport(SessionActivity sessionEvent, string accountName)
        {
            return new []
            {
                accountName, sessionEvent.UserName,
            };
        }
    }
}
// Copyright (c) 2019 Lykke Corp.

using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Axle.Contracts;
using Common.Log;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services.Projections
{
    public class SessionActivityProjection : IStartable
    {
        private readonly IRabbitMqSubscriberService _rabbitMqSubscriberService;
        private readonly ActivitiesSettings _settings;
        private readonly IActivitiesSender _cqrsSender;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly IDateService _dateService;
        private readonly ILog _log;
        
        public SessionActivityProjection(IRabbitMqSubscriberService rabbitMqSubscriberService,
            ActivitiesSettings settings,
            IActivitiesSender cqrsSender,
            IIdentityGenerator identityGenerator,
            IDateService dateService,
            ILog log)
        {
            _rabbitMqSubscriberService = rabbitMqSubscriberService;
            _settings = settings;
            _cqrsSender = cqrsSender;
            _identityGenerator = identityGenerator;
            _dateService = dateService;
            _log = log;
        }
        
        public void Start()
        {
            _rabbitMqSubscriberService.Subscribe(_settings.Consumers.SessionActivity,
                true,
                HandleSessionActivityEvent,
                _rabbitMqSubscriberService.GetMsgPackDeserializer<SessionActivity>());
        }

        private Task HandleSessionActivityEvent(SessionActivity sessionEvent)
        {
            var activityType = ActivityType.None;
            var descriptionAttributes = new List<string>();
            var relatedIds = new string[0];
            
            switch (sessionEvent.Type)
            {
                case SessionActivityType.Login:
                    activityType = ActivityType.SessionLogIn;
                    descriptionAttributes.AddRange(GetDescriptionForLogInLogOut(sessionEvent));
                    break;
                case SessionActivityType.SignOut:
                    activityType = ActivityType.SessionSignOut;
                    descriptionAttributes.AddRange(GetDescriptionForLogInLogOut(sessionEvent));
                    break;
                case SessionActivityType.TimeOut:
                    activityType = ActivityType.SessionTimeOutTermination;
                    descriptionAttributes.AddRange(GetDescriptionForTermination(sessionEvent));
                    break;
                case SessionActivityType.DifferentDeviceTermination:
                    activityType = ActivityType.SessionDifferentDeviceTermination;
                    descriptionAttributes.AddRange(GetDescriptionForTermination(sessionEvent));
                    break;
                case SessionActivityType.ManualTermination:
                    activityType = ActivityType.SessionManualTermination;
                    descriptionAttributes.AddRange(GetDescriptionForTermination(sessionEvent));
                    break;
                case SessionActivityType.OnBehalfSupportConnected:
                    activityType = ActivityType.SessionConnectedByOnBehalfSupport;
                    descriptionAttributes.AddRange(GetDescriptionForOnBehalfSupport(sessionEvent));
                    break;
                case SessionActivityType.OnBehalfSupportDisconnected:
                    activityType = ActivityType.SessionDisconnectedByOnBehalfSupport;
                    descriptionAttributes.AddRange(GetDescriptionForOnBehalfSupport(sessionEvent));
                    break;
                default:
                    return Task.CompletedTask;
            }

            var activity = new Activity(
                _identityGenerator.GenerateId(),
                sessionEvent.AccountId,
                string.Empty,
                sessionEvent.SessionId.ToString(),
                _dateService.Now(),
                activityType,
                descriptionAttributes.ToArray(),
                relatedIds
            );

            _cqrsSender.PublishActivity(activity);
            
            return Task.CompletedTask;
        }

        private static IEnumerable<string> GetDescriptionForTermination(SessionActivity sessionEvent)
        {
            return new []
            {
                sessionEvent.SessionId.ToString(), sessionEvent.UserName,
            };
        }

        private static IEnumerable<string> GetDescriptionForLogInLogOut(SessionActivity sessionEvent)
        {
            return new []
            {
                sessionEvent.UserName, sessionEvent.AccountId, sessionEvent.SessionId.ToString(),
            };
        }

        private static IEnumerable<string> GetDescriptionForOnBehalfSupport(SessionActivity sessionEvent)
        {
            return new []
            {
                sessionEvent.AccountId, sessionEvent.UserName,
            };
        }
    }
}
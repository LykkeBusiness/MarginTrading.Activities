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
                _rabbitMqSubscriberService.GetJsonDeserializer<SessionActivity>());
        }

        private Task HandleSessionActivityEvent(SessionActivity sessionEvent)
        {
            var activityType = ActivityType.None;
            var descriptionAttributes = new string[0];
            var relatedIds = new string[0];
            
            switch (sessionEvent.Type)
            {
                case SessionActivityType.Login:
                    activityType = ActivityType.SessionLogIn;
                    break;
                case SessionActivityType.SignOut:
                    activityType = ActivityType.SessionSignOut;
                    break;
                case SessionActivityType.TimeOut:
                    activityType = ActivityType.SessionTimeOutTermination;
                    break;
                case SessionActivityType.DifferentDeviceTermination:
                    activityType = ActivityType.SessionDifferentDeviceTermination;
                    break;
                case SessionActivityType.ManualTermination:
                    activityType = ActivityType.SessionManualTermination;
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
                descriptionAttributes,
                relatedIds
            );

            _cqrsSender.PublishActivity(activity);
            
            return Task.CompletedTask;
        }
    }
}
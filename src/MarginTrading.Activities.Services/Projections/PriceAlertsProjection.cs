// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using Lykke.Snow.PriceAlerts.Contract.Models.Events;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activities.Services.Projections
{
    public class PriceAlertsProjection
    {
        private readonly IAssetPairsCacheService _assetPairsCache;
        private readonly IIdentityGenerator _identityGenerator;
        private readonly IActivitiesSender _cqrsSender;

        public PriceAlertsProjection(IAssetPairsCacheService assetPairsCache,
            IIdentityGenerator identityGenerator,
            IActivitiesSender cqrsSender)
        {
            _assetPairsCache = assetPairsCache;
            _identityGenerator = identityGenerator;
            _cqrsSender = cqrsSender;
        }

        [UsedImplicitly]
        public Task Handle(PriceAlertChangedEvent e)
        {
            Activity activity;

            switch (e.ChangeType)
            {
                case ChangeType.Creation:
                    activity = FromCreated(e);
                    break;
                case ChangeType.Edition:
                    activity = FromEdited(e);
                    break;
                case ChangeType.Deletion:
                    // not used
                    return Task.CompletedTask;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            _cqrsSender.PublishActivity(activity);
            return Task.CompletedTask;
        }


        private Activity FromEdited(PriceAlertChangedEvent e)
        {
            var priceAlert = e.NewValue;
            var activityType = Map(priceAlert.Status);

            var activity = new Activity(_identityGenerator.GenerateId(),
                priceAlert.AccountId,
                priceAlert.ProductId,
                priceAlert.Id,
                e.Timestamp,
                activityType,
                GetDescriptionAttributes(activityType, priceAlert),
                new[] {priceAlert.Id},
                e.CorrelationId
            );

            return activity;
        }

        private ActivityType Map(AlertStatusContract status)
        {
            return status switch
            {
                AlertStatusContract.Active => ActivityType.PriceAlertUpdated,
                AlertStatusContract.Triggered => ActivityType.PriceAlertTriggered,
                AlertStatusContract.Expired => ActivityType.PriceAlertExpired,
                AlertStatusContract.Cancelled => ActivityType.PriceAlertCancelled,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unable to map price alert status to activity type")
            };
        }

        private Activity FromCreated(PriceAlertChangedEvent e)
        {
            var priceAlert = e.NewValue;

            var activity = new Activity(_identityGenerator.GenerateId(),
                priceAlert.AccountId,
                priceAlert.ProductId,
                priceAlert.Id,
                e.Timestamp,
                ActivityType.PriceAlertCreated,
                GetDescriptionAttributes(ActivityType.PriceAlertCreated, priceAlert),
                new[] {priceAlert.Id},
                e.CorrelationId
            );

            return activity;
        }

        private string[] GetDescriptionAttributes(ActivityType activityType, PriceAlertContract priceAlert)
        {
            var product = _assetPairsCache.TryGetAssetPair(priceAlert.ProductId);

            switch (activityType)
            {
                case ActivityType.PriceAlertCreated:
                case ActivityType.PriceAlertUpdated:
                case ActivityType.PriceAlertExpired:
                case ActivityType.PriceAlertCancelled:
                    return new[]
                    {
                        product.Name,
                        priceAlert.PriceType.ToString(),
                        priceAlert.Price.ToUiString(product.Accuracy),
                        product.QuoteAssetId,
                        priceAlert.Direction.ToString(),
                        priceAlert.Comment,
                        GetValidity(priceAlert.Validity),
                    };

                case ActivityType.PriceAlertTriggered:
                    return new[]
                    {
                        product.Name,
                        priceAlert.PriceType.ToString(),
                        priceAlert.Price.ToUiString(product.Accuracy),
                        product.QuoteAssetId,
                        priceAlert.Direction.ToString(),
                        priceAlert.Comment,
                    };

                default: return new string[0];
            }
        }

        private static string GetValidity(DateTime? validity)
        {
            return validity.HasValue ? validity.Value.ToString("g", CultureInfo.InvariantCulture) : "GTC";
        }
    }
}
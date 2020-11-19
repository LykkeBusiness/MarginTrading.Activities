// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.Activities.Core.Constants;
using MarginTrading.Activities.Core.Settings;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.AssetService.Contracts.AssetPair;
using MarginTrading.AssetService.Contracts.Enums;
using MarginTrading.AssetService.Contracts.Products;

namespace MarginTrading.Activities.Services.Projections
{
    /// <summary>
    /// Listens to <see cref="AssetPairChangedEvent"/>s and builds a projection inside of the
    /// <see cref="IAssetPairsCacheService"/>
    /// </summary>
    [UsedImplicitly]
    public class ProductChangedProjection
    {
        private readonly IAssetPairsCacheService _assetPairsCache;
        private readonly DefaultLegalEntitySettings _legalEntitySettings;
        private readonly ILog _log;

        public ProductChangedProjection(
            IAssetPairsCacheService assetPairsCache,
            DefaultLegalEntitySettings legalEntitySettings,
            ILog log)
        {
            _assetPairsCache = assetPairsCache;
            _legalEntitySettings = legalEntitySettings;
            _log = log;
        }

        [UsedImplicitly]
        public async Task Handle(ProductChangedEvent @event)
        {
            //deduplication is not required, it's ok if an object is updated multiple times
            switch (@event.ChangeType)
            {
                case ChangeType.Creation:
                case ChangeType.Edition:
                    if (!@event.NewValue.IsStarted) return;
                    break;
                case ChangeType.Deletion:
                    if (!@event.OldValue.IsStarted) return;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (@event.Timestamp < _assetPairsCache.InitTimestamp)
            {
                _log.WriteInfo(nameof(ProductChangedProjection), nameof(Handle),
                    $"Product changed event with eventId: {@event.EventId}, will be ignored cause event Timestamp is before cache init timestamp:{_assetPairsCache.InitTimestamp}");
                return;
            }

            if (@event.ChangeType == ChangeType.Deletion)
            {
                _assetPairsCache.Remove(@event.OldValue.ProductId);
            }
            else
            {
                _assetPairsCache.AddOrUpdate(Map(@event.NewValue, _legalEntitySettings.DefaultLegalEntity));
            }
        }

        private static AssetPairContract Map(ProductContract product, string legalEntity)
        {
            return new AssetPairContract
            {
                Id = product.ProductId,
                Name = product.Name,
                BaseAssetId = product.ProductId,
                QuoteAssetId = product.TradingCurrency,
                Accuracy = AssetPairConstants.Accuracy,
                MarketId = product.Market,
                LegalEntity = legalEntity,
                BasePairId = AssetPairConstants.BasePairId,
                MatchingEngineMode = (MatchingEngineModeContract)AssetPairConstants.MatchingEngineMode,
                StpMultiplierMarkupBid = AssetPairConstants.StpMultiplierMarkupBid,
                StpMultiplierMarkupAsk = AssetPairConstants.StpMultiplierMarkupAsk,
                IsSuspended = product.IsSuspended,
                IsFrozen = product.IsFrozen,
                IsDiscontinued= product.IsDiscontinued,
                FreezeInfo = new FreezeInfoContract
                {
                    Comment = product.FreezeInfo.Comment,
                    Reason = (FreezeReasonContract)product.FreezeInfo.Reason,
                    UnfreezeDate = product.FreezeInfo.UnfreezeDate,
                }
            };
        }
    }
}
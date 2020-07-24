// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.AssetService.Contracts.AssetPair;

namespace MarginTrading.Activities.Services.Projections
{
    /// <summary>
    /// Listens to <see cref="AssetPairChangedEvent"/>s and builds a projection inside of the
    /// <see cref="IAssetPairsCacheService"/>
    /// </summary>
    [UsedImplicitly]
    public class AssetPairProjection
    {
        private readonly IAssetPairsCacheService _assetPairsCache;
        private readonly ILog _log;

        public AssetPairProjection(
            IAssetPairsCacheService assetPairsCache,
            ILog log)
        {
            _assetPairsCache = assetPairsCache;
            _log = log;
        }

        [UsedImplicitly]
        public async Task Handle(AssetPairChangedEvent @event)
        {
            //deduplication is not required, it's ok if an object is updated multiple times
            if (@event.AssetPair?.Id == null)
            {
                await _log.WriteWarningAsync(nameof(AssetPairProjection), nameof(Handle),
                    "AssetPairChangedEvent contained no asset pair id");
                return;
            }

            if (IsDelete(@event))
            {
                _assetPairsCache.Remove(@event.AssetPair.Id);
            }
            else
            {
                _assetPairsCache.AddOrUpdate(@event.AssetPair);
            }
        }

        private static bool IsDelete(AssetPairChangedEvent @event)
        {
            return @event.AssetPair.BaseAssetId == null || @event.AssetPair.QuoteAssetId == null;
        }
    }
}
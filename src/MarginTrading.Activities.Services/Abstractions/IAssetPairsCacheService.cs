// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.AssetService.Contracts.AssetPair;

namespace MarginTrading.Activities.Services.Abstractions
{
    public interface IAssetPairsCacheService
    {
        //TODO: if contract will change, create internal domain model
        AssetPairContract TryGetAssetPair(string assetPairId);

        void AddOrUpdate(AssetPairContract assetPair);

        void Remove(string assetPairId);
    }
}
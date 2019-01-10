using MarginTrading.SettingsService.Contracts.AssetPair;

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
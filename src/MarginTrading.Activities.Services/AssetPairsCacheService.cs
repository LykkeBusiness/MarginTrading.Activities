// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using MarginTrading.Activities.Services.Abstractions;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.AssetPair;

namespace MarginTrading.Activities.Services
{
    public class AssetPairsCacheService : IAssetPairsCacheService, IStartable
    {
        private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

        private Dictionary<string, AssetPairContract> _assetPairs = new Dictionary<string, AssetPairContract>();

        private readonly IAssetPairsApi _assetPairsApi;
        private readonly IAssetsApi _assetsApi;

        public AssetPairsCacheService(
            IAssetPairsApi assetPairsApi,
            IAssetsApi assetsApi)
        {
            _assetPairsApi = assetPairsApi;
            _assetsApi = assetsApi;
        }

        public AssetPairContract TryGetAssetPair(string assetPairId)
        {
            _readerWriterLockSlim.EnterReadLock();

            try
            {
                return _assetPairs.TryGetValue(assetPairId, out var result)
                    ? result
                    : null;
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        public void AddOrUpdate(AssetPairContract assetPair)
        {
            _readerWriterLockSlim.EnterWriteLock();

            try
            {
                var asset = _assetsApi.Get(assetPair.Id).GetAwaiter().GetResult();
                if (asset != null)
                {
                    assetPair.Name = asset.Name;
                }
                _assetPairs[assetPair.Id] = assetPair;
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Remove(string assetPairId)
        {
            _readerWriterLockSlim.EnterWriteLock();

            try
            {
                _assetPairs.Remove(assetPairId);
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Start()
        {
            _readerWriterLockSlim.EnterWriteLock();

            try
            {
                _assetPairs = _assetPairsApi.List().GetAwaiter().GetResult().ToDictionary(a => a.Id);

                var assets = _assetsApi.List().GetAwaiter().GetResult();
                foreach (KeyValuePair<string, AssetPairContract> assetPair in _assetPairs)
                {
                    var asset = assets.FirstOrDefault(x => x.Id == assetPair.Key);
                    if (asset != null)
                    {
                        assetPair.Value.Name = asset.Name;
                    }
                }
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }
    }
}
using Autofac;
using MarginTrading.Activities.Core.Caches;
using MarginTrading.Activities.Core.Domain;
using MarginTrading.SettingsService.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MarginTrading.Activities.Services.Caches
{
    public class AssetsCache : IAssetsCache, IStartable
    {
        private int _maxAccuracy = 8;

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private Dictionary<string, Asset> _cache = new Dictionary<string, Asset>();

        private readonly IAssetsApi _assetsApi;

        public AssetsCache(IAssetsApi assetsApi)
        {
            _assetsApi = assetsApi;
        }

        public string GetName(string id)
        {
            _lock.EnterReadLock();

            try
            {
                return id != null && _cache.TryGetValue(id, out var result)
                    ? result.Name
                    : id;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public int GetAccuracy(string id)
        {
            _lock.EnterReadLock();

            try
            {
                return id != null && _cache.TryGetValue(id, out var result)
                    ? result.Accuracy
                    : _maxAccuracy;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Asset GetAsset(string id)
        {
            _lock.EnterReadLock();

            try
            {
                return id != null && _cache.TryGetValue(id, out var result)
                    ? result
                    : new Asset(id, id, _maxAccuracy);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Start()
        {
            _lock.EnterWriteLock();

            try
            {
                _cache = _assetsApi.List().GetAwaiter().GetResult()
                    .ToDictionary(x => x.Id, s => new Asset(s.Id, s.Name, s.Accuracy));
                _maxAccuracy = _cache.Any()
                    ? _cache.Max(x => x.Value.Accuracy)
                    : 8;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
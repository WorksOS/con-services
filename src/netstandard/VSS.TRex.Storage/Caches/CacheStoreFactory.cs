using Apache.Ignite.Core.Cache.Store;
using Apache.Ignite.Core.Common;
using System;

namespace VSS.TRex.Storage.Caches
{
    /// <summary>
    /// The cache store factory responsible for creating a cache store tailored for storing immutable representations
    /// of information in data models
    /// </summary>
    [Serializable]
    public class CacheStoreFactory : IFactory<ICacheStore>
    {
        private bool IsMutable { get; set; }
        private bool IsSpatial { get; set; }

        public CacheStoreFactory()
        {
        }

        public CacheStoreFactory(bool isSpatial, bool isMutable) : this()
        {
            IsMutable = isMutable;
            IsSpatial = isSpatial;
        }

        public ICacheStore CreateInstance()
        {
            return IsSpatial ?
                new TRexSpatialCacheStore(IsMutable ? "(Mutable)" : "(Immutable)")  :
                new TRexNonSpatialCacheStore(IsMutable ? "(Mutable)" : "(Immutable)") as ICacheStore; 
        }
    }
}

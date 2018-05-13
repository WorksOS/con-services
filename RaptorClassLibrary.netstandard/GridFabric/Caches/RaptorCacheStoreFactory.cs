using Apache.Ignite.Core.Cache.Store;
using Apache.Ignite.Core.Common;
using System;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    /// <summary>
    /// The cache store factory responsible for creating a cache store tailored for storing immutable representations
    /// of information in data models
    /// </summary>
    [Serializable]
    public class RaptorCacheStoreFactory : IFactory<ICacheStore>
    {
        private bool IsMutable { get; set; }
        private bool IsSpatial { get; set; }

        public RaptorCacheStoreFactory()
        {
        }

        public RaptorCacheStoreFactory(bool isSpatial, bool isMutable) : this()
        {
            IsMutable = isMutable;
            IsSpatial = isSpatial;
        }

        public ICacheStore CreateInstance()
        {
            return IsSpatial ?
                new RaptorSpatialCacheStore(IsMutable ? "(Mutable)" : "(Immutable)")  :
                new RaptorNonSpatialCacheStore(IsMutable ? "(Mutable)" : "(Immutable)") as ICacheStore; 
        }
    }
}

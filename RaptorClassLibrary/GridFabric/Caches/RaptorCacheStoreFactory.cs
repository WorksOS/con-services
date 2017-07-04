using Apache.Ignite.Core.Cache.Store;
using Apache.Ignite.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    /// The cache store factory responsible for creating a cache store tailored for storing immutable representations
    /// of information in data models
    /// </summary>
    [Serializable]
    public class RaptorCacheStoreFactory : IFactory<ICacheStore>
    {
        private bool IsMutable { get; set; } = false;
        private bool IsSpatial { get; set; } = false;

        public RaptorCacheStoreFactory() : base()
        {
        }

        public RaptorCacheStoreFactory(bool isSpatial, bool isMutable) : this()
        {
            IsMutable = IsMutable;
            IsSpatial = isSpatial;
        }

        public ICacheStore CreateInstance()
        {
            return IsSpatial ?
                new RaptorSpatialCacheStore(IsMutable ? "(Mutable)" : "(Immutable)") as ICacheStore :
                new RaptorNonSpatialCacheStore(IsMutable ? "(Mutable)" : "(Immutable)") as ICacheStore; 
        }
    }
}

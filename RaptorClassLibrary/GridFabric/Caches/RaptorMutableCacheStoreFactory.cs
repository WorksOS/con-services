using Apache.Ignite.Core.Cache.Store;
using Apache.Ignite.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    /// <summary>
    /// The cache store factory responsible for creating a cache store tailored for storing mutable representations
    /// of information in data models
    /// </summary>
    [Serializable]
    public class RaptorMutableCacheStoreFactory : IFactory<ICacheStore>
    {
        public ICacheStore CreateInstance()
        {
            return new RaptorMutableCacheStore();
        }
    }
}

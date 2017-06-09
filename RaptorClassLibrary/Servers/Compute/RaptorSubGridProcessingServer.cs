using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Servers.Compute
{
    /// <summary>
    /// A server type that represents a server useful for context processing sets of SubGrid information. This is essentially an analogue of
    /// the PSNode servers in legacy Raptor and contains both a cache of data and processing against it in response to client context server requests.
    /// </summary>
    public class RaptorSubGridProcessingServer : RaptorCacheComputeServer
    {
        public override void ConfigureRaptorGrid(IgniteConfiguration cfg)
        {
            base.ConfigureRaptorGrid(cfg);
            cfg.UserAttributes.Add("Role", "PSNode");
        }

        public override void ConfigureRaptorCache(CacheConfiguration cfg)
        {
            base.ConfigureRaptorCache(cfg);
        }

        public override void ConfigureSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureSpatialCache(cfg);
        }

        /// <summary>
        /// Overridden spatial cache instantiation method. This method never creates a new cache but will only get an already existing spatial data cache
        /// </summary>
        /// <param name="CacheCfg"></param>
        /// <returns></returns>
        public override ICache<String, MemoryStream> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return base.InstantiateSpatialCacheReference(CacheCfg);
            // return raptorGrid.GetCache<String, MemoryStream>(CacheCfg.Name);
        }

        /// <summary>
        /// Overridden raptor cache instantiation method. This method never creates a new cache but will only get an already existing spatial data cache
        /// </summary>
        /// <param name="CacheCfg"></param>
        /// <returns></returns>
        public override ICache<String, MemoryStream> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return base.InstantiateRaptorCacheReference(CacheCfg);
            // return raptorGrid.GetCache<String, MemoryStream>(CacheCfg.Name);
        }

        public RaptorSubGridProcessingServer() : base()
        {
        }
    }
}

using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using log4net;
using System.Reflection;
using VSS.VisionLink.Raptor.GridFabric.Affinity;

namespace VSS.VisionLink.Raptor.Servers.Compute
{
    /// <summary>
    /// A server type that represents a server useful for context processing sets of SubGrid information. This is essentially an analogue of
    /// the PSNode servers in legacy Raptor and contains both a cache of data and processing against it in response to client context server requests.
    /// Note: These servers typically access the immutable representations of the spatial data for performance reasons, as configured
    /// in the server constructor.
    /// </summary>
    public class RaptorSubGridProcessingServer : RaptorImmutableCacheComputeServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void ConfigureRaptorGrid(IgniteConfiguration cfg)
        {
            base.ConfigureRaptorGrid(cfg);

            cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.PSNODE}", "True");
        }

        public override void ConfigureNonSpatialMutableCache(CacheConfiguration cfg)
        {
            base.ConfigureNonSpatialMutableCache(cfg);
        }

        public override void ConfigureNonSpatialImmutableCache(CacheConfiguration cfg)
        {
            base.ConfigureNonSpatialImmutableCache(cfg);
        }

        public override void ConfigureMutableSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureMutableSpatialCache(cfg);
        }

        public override void ConfigureImmutableSpatialCache(CacheConfiguration cfg)
        {
            base.ConfigureImmutableSpatialCache(cfg);
        }

        /// <summary>
        /// Overridden spatial cache instantiation method. This method never creates a new cache but will only get an already existing spatial data cache
        /// </summary>
        /// <param name="CacheCfg"></param>
        /// <returns></returns>
        public override ICache<SubGridSpatialAffinityKey, byte[]> InstantiateSpatialCacheReference(CacheConfiguration CacheCfg)
        {
            return base.InstantiateSpatialCacheReference(CacheCfg);
            // return raptorGrid.GetCache<String, byte[]>(CacheCfg.Name);
        }

        /// <summary>
        /// Overridden raptor cache instantiation method. This method never creates a new cache but will only get an already existing spatial data cache
        /// </summary>
        /// <param name="CacheCfg"></param>
        /// <returns></returns>
        public override ICache<string, byte[]> InstantiateRaptorCacheReference(CacheConfiguration CacheCfg)
        {
            return base.InstantiateRaptorCacheReference(CacheCfg);
            // return raptorGrid.GetCache<String, MemoryStream>(CacheCfg.Name);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaptorSubGridProcessingServer()
        {
        }

        /// <summary>
        /// Sets up the loocal server configuration to reflect the requirements of subgrid processing
        /// </summary>
        public override void SetupServerSpecificConfiguration()
        {
            // Enable use of immutable data pools when processing requests
            RaptorServerConfig.Instance().UseMutableSpatialData = false;
            RaptorServerConfig.Instance().UseMutableNonSpatialData = false;
        }
    }
}

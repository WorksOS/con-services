using Apache.Ignite.Core;
using log4net;
using System.Reflection;

namespace VSS.TRex.Servers.Compute
{
    /// <summary>
    /// A server type that represents a server useful for context processing sets of SubGrid information. This is essentially an analogue of
    /// the PSNode servers in legacy Raptor and contains both a cache of data and processing against it in response to client context server requests.
    /// Note: These servers typically access the immutable representations of the spatial data for performance reasons, as configured
    /// in the server constructor.
    /// </summary>
    public class SubGridProcessingServer : ImmutableCacheComputeServer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void ConfigureRaptorGrid(IgniteConfiguration cfg)
        {
            base.ConfigureRaptorGrid(cfg);

            cfg.UserAttributes.Add($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{ServerRoles.PSNODE}", "True");
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SubGridProcessingServer()
        {
        }

        /// <summary>
        /// Sets up the loocal server configuration to reflect the requirements of subgrid processing
        /// </summary>
        public override void SetupServerSpecificConfiguration()
        {
            // Enable use of immutable data pools when processing requests
            TRexServerConfig.Instance().UseMutableSpatialData = false;
            TRexServerConfig.Instance().UseMutableNonSpatialData = false;
        }
    }
}

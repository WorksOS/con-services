using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.GridFabric.ComputeFuncs
{
    /// <summary>
    ///  Represents a request that can be made against the design profiler cluster group in the Raptor grid
    /// </summary>
    public class CacheComputeComputeFunc<TArgument, TResponse> : BaseRaptorRequest<TArgument, TResponse>
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public CacheComputeComputeFunc() : base(RaptorGrids.RaptorImmutableGridName(), ServerRoles.PSNODE)
        {
        }
    }
}

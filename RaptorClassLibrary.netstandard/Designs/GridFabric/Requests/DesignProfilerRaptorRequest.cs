using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Requests;

namespace VSS.VisionLink.DesignProfiling.GridFabric.Requests
{
    /// <summary>
    ///  Represents a request that can be made against the design profiler cluster group in the Raptor grid
    /// </summary>
    public class DesignProfilerRaptorRequest<TArgument, TResponse> : BaseRaptorRequest<TArgument, TResponse>
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public DesignProfilerRaptorRequest() : base(TRexGrids.ImmutableGridName(), "DesignProfiler")
        {
        }
    }
}

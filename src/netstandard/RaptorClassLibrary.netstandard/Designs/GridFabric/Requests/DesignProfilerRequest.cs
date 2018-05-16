using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Requests;

namespace VSS.VisionLink.DesignProfiling.GridFabric.Requests
{
    /// <summary>
    ///  Represents a request that can be made against the design profiler cluster group in the TRex grid
    /// </summary>
    public class DesignProfilerRequest<TArgument, TResponse> : BaseRequest<TArgument, TResponse>
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public DesignProfilerRequest() : base(TRexGrids.ImmutableGridName(), "DesignProfiler")
        {
        }
    }
}

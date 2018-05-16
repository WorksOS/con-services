using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers;

namespace VSS.TRex.GridFabric.Requests
{
    /// <summary>
    ///  Represents a request that can be made against the design profiler cluster group in the TRex grid
    /// </summary>
    public class ApplicationServicePoolRequest<TArgument, TResponse> : BaseRequest<TArgument, TResponse>
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public ApplicationServicePoolRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
        {
        }
    }
}

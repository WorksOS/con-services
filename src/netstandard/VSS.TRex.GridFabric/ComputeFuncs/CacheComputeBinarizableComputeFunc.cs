using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.GridFabric.ComputeFuncs
{
  /// <summary>
  ///  Represents a request that can be made against the design profiler cluster group in the TRex grid
  /// </summary>
  public abstract class CacheComputeBinarizableComputeFunc<TArgument, TResponse> : BaseBinarizableRequest<TArgument, TResponse>
  {
    /// <summary>
    /// Default no-arg constructor that sets up cluster and compute projections available for use
    /// </summary>
    public CacheComputeBinarizableComputeFunc() : base(TRexGrids.ImmutableGridName(), ServerRoles.PSNODE)
    {
    }
  }
}

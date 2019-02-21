using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.CoordinateSystems.GridFabric.Requests
{
  /// <summary>
  ///  Represents a request that can be made against the coordinate system cluster group in the TRex grid.
  /// </summary>
  public abstract class CoordinateSystemRequest<TArgument, TResponse> : BaseRequest<TArgument, TResponse>
  {
    /// <summary>
    /// Default no-arg constructor that sets up cluster and compute projections available for use
    /// </summary>
    public CoordinateSystemRequest() : base(TRexGrids.MutableGridName(), ServerRoles.TAG_PROCESSING_NODE)
    {
    }
  }
}

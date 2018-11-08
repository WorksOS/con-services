using VSS.TRex.Exports.Surfaces.GridDecimator;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Exports.Surfaces.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a surface to be constructed from elevation data
  /// </summary>
  public class TINSurfaceRequest : GenericASNodeRequest<TINSurfaceRequestArgument, TINSurfaceRequestComputeFunc, TINSurfaceResult>
    // Declare class like this to delegate the request to the cluster compute layer
    //    public class PatchRequest : GenericPSNodeBroadcastRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
  {
    /// <summary>
    /// Default constructor that configures the request to be sent to the TIN export projection on the immutable data grid
    /// </summary>
    public TINSurfaceRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.TIN_SURFACE_EXPORT_ROLE)
    {
    }
  }
}

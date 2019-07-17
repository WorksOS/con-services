using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.QuantizedMesh.GridFabric.Arguments;
using VSS.TRex.QuantizedMesh.GridFabric.ComputeFuncs;
using VSS.TRex.QuantizedMesh.GridFabric.Responses;

namespace VSS.TRex.QuantizedMesh.GridFabric.Requests
{
  /// <summary>
  /// Sends a request to the grid for a tile to be rendered
  /// </summary>
  public class QuantizedMeshRequest : GenericASNodeRequest<QuantizedMeshRequestArgument, QuantizedMeshRequestComputeFunc, QuantizedMeshResponse>
  // Declare class like this to delegate the request to the cluster compute layer
  {
    public QuantizedMeshRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.QNANTIZED_MESH_NODE)
    {

    }
  }
}

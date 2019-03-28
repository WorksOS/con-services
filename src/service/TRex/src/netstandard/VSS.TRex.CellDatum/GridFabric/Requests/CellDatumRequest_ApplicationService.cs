using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.CellDatum.GridFabric.Requests
{
  /// <summary>
  /// Defines the contract for the cell datum request made to the applications service
  /// </summary>
  public class CellDatumRequest_ApplicationService : GenericASNodeRequest
    <CellDatumRequestArgument_ApplicationService, CellDatumRequestComputeFunc_ApplicationService, CellDatumResponse_ApplicationService>
  {
    public CellDatumRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}

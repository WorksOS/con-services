using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.CellDatum.GridFabric.Requests
{
  public class CellPassesRequest_ApplicationService : GenericASNodeRequest
    <CellPassesRequestArgument_ApplicationService, CellPassesRequestComputeFunc_ApplicationService, CellPassesResponse>
  {
    public CellPassesRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}

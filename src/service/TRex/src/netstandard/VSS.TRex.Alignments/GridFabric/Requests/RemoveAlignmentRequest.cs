using VSS.TRex.Alignments.GridFabric.Arguments;
using VSS.TRex.Alignments.GridFabric.ComputeFuncs;
using VSS.TRex.Alignments.GridFabric.Responses;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Alignments.GridFabric.Requests
{
  public class RemoveAlignmentRequest : GenericASNodeRequest<RemoveAlignmentArgument, RemoveAlignmentComputeFunc, RemoveAlignmentResponse>
  {
    public RemoveAlignmentRequest() : base(TRexGrids.MutableGridName(), ServerRoles.DATA_MUTATION_ROLE)
    {
    }
  }
}

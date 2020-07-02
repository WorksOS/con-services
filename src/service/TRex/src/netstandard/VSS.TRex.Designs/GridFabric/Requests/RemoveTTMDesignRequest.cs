using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  public class RemoveTTMDesignRequest : GenericASNodeRequest<RemoveTTMDesignArgument, RemoveTTMDesignComputeFunc, RemoveTTMDesignResponse>
  {
    public RemoveTTMDesignRequest() : base(TRexGrids.MutableGridName(), ServerRoles.DATA_MUTATION_ROLE)
    {
    }
  }
}

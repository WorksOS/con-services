using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.Requests
{
    /// <summary>
    /// A request that may be issued to compute a volume
    /// </summary>
    public class ProgressiveVolumesRequest_ApplicationService : GenericASNodeRequest<ProgressiveVolumesRequestArgument, ProgressiveVolumesRequestComputeFunc_ApplicationService, ProgressiveVolumesResponse> 
    {
      public ProgressiveVolumesRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
      {

      }
  }
}

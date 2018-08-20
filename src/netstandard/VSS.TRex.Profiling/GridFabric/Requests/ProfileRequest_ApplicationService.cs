using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.ComputeFuncs;
using VSS.TRex.Profiling.GridFabric.Responses;

namespace VSS.TRex.Profiling.GridFabric.Requests
{
  /// <summary>
  /// Defines the contract for the profile request made to the applications service
  /// </summary>
  public class ProfileRequest_ApplicationService : GenericASNodeRequest<ProfileRequestArgument_ApplicationService, ProfileRequestComputeFunc_ApplicationService, ProfileRequestResponse>
  {
    public ProfileRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE_PROFILER)
    {
    }
  }
}

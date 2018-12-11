using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.ComputeFuncs;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling.GridFabric.Requests
{
  /// <summary>
  /// Defines the contract for the profile request made to the compute cluster
  /// </summary>
  public class ProfileRequest_ClusterCompute<T> : GenericPSNodeBroadcastRequest<ProfileRequestArgument_ClusterCompute, ProfileRequestComputeFunc_ClusterCompute<T>, ProfileRequestResponse<T>> where T : class, IProfileCellBase, new()
  {
  }
}

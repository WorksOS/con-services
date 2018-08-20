using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Profiling.Executors;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Responses;

namespace VSS.TRex.Profiling.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The logic Ignite executes on cluster compute nodes for profile requests
  /// </summary>
  public class ProfileRequestComputeFunc_ClusterCompute : BaseComputeFunc,
    IComputeFunc<ProfileRequestArgument_ClusterCompute, ProfileRequestResponse>
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ProfileRequestComputeFunc_ClusterCompute>();

    public ProfileRequestComputeFunc_ClusterCompute() : base(TRexGrids.ImmutableGridName(), ServerRoles.PSNODE)
    {
    }

    public ProfileRequestResponse Invoke(ProfileRequestArgument_ClusterCompute arg)
    {
      Log.LogInformation("In ProfileRequestComputeFunc.Invoke()");

      try
      {
        ComputeProfileExecutor_ClusterCompute Executor = new ComputeProfileExecutor_ClusterCompute
        (arg.ProjectID, arg.ProfileTypeRequired, arg.NEECoords, arg.Filters,
          arg.DesignDescriptor, arg.ReturnAllPassesAndLayers);

        Log.LogInformation("Executing profiler.Execute()");

        return Executor.Execute();
      }
      finally
      {
        Log.LogInformation("Exiting ProfileRequestComputeFunc.Invoke()");
      }
    }
  }
}

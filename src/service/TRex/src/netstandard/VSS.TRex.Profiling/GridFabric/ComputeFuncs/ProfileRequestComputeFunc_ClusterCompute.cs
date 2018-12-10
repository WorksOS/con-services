using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Profiling.Executors;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Interfaces;

namespace VSS.TRex.Profiling.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The logic Ignite executes on cluster compute nodes for profile requests
  /// </summary>
  public class ProfileRequestComputeFunc_ClusterCompute<T> : BaseComputeFunc, IComputeFunc<ProfileRequestArgument_ClusterCompute, ProfileRequestResponse<T>> where T: class, IProfileCellBase, new()
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ProfileRequestComputeFunc_ClusterCompute<T>>();

    public ProfileRequestResponse<T> Invoke(ProfileRequestArgument_ClusterCompute arg)
    {
      Log.LogInformation("In ProfileRequestComputeFunc.Invoke()");

      try
      {
        ComputeProfileExecutor_ClusterCompute<T> Executor = new ComputeProfileExecutor_ClusterCompute<T>
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

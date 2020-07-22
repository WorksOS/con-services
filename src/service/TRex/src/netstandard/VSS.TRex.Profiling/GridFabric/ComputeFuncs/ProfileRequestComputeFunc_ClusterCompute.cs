using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ProfileRequestComputeFunc_ClusterCompute<T>>();

    public ProfileRequestResponse<T> Invoke(ProfileRequestArgument_ClusterCompute arg)
    {
      _log.LogInformation("In ProfileRequestComputeFunc.Invoke()");

      try
      {
        var executor = new ComputeProfileExecutor_ClusterCompute<T>(arg.ProfileStyle, arg.ProjectID, arg.ProfileTypeRequired, arg.NEECoords, arg.Filters,
          arg.ReferenceDesign, arg.ReturnAllPassesAndLayers, arg.VolumeType, arg.Overrides, arg.LiftParams);

        _log.LogInformation("Executing profiler.ExecuteAsync()");

        return executor.ExecuteAsync().WaitAndUnwrapException();
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception requesting profile at cluster compute layer");
        return new ProfileRequestResponse<T> { ResultStatus = Types.RequestErrorStatus.Exception };
      }
      finally
      {
        _log.LogInformation("Exiting ProfileRequestComputeFunc.Invoke()");
      }
    }
  }
}

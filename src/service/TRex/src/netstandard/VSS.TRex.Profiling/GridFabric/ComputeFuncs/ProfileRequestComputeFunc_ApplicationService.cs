using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Profiling.Executors;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Responses;

namespace VSS.TRex.Profiling.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The logic Ignite executes on application service node for profile requests
  /// </summary>
  public class ProfileRequestComputeFunc_ApplicationService : BaseComputeFunc, IComputeFunc<ProfileRequestArgument_ApplicationService, ProfileRequestResponse>
  {
    private static ILogger Log = Logging.Logger.CreateLogger<ProfileRequestComputeFunc_ApplicationService>();

    public ProfileRequestComputeFunc_ApplicationService()
    {
    }

    /// <summary>
    /// Delegates processing of the profile like to the cluster compute layer, then aggregates together the fractional responses
    /// received from each participating node in the query
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public ProfileRequestResponse Invoke(ProfileRequestArgument_ApplicationService arg)
    {
      Log.LogInformation("In Invoke()");

      try
      {
        ComputeProfileExecutor_ApplicatonService Executor = new ComputeProfileExecutor_ApplicatonService();
        return Executor.Execute(arg);
      }
      finally
      {
        Log.LogInformation("Out Invoke()");
      }
    }
  }
}


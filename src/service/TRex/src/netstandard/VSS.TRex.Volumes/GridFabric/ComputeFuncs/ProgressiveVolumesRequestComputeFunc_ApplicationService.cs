using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Executors;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.ComputeFuncs
{
  public class ProgressiveVolumesRequestComputeFunc_ApplicationService : BaseComputeFunc, IComputeFunc<ProgressiveVolumesRequestArgument, ProgressiveVolumesResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProgressiveVolumesRequestComputeFunc_ApplicationService>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public ProgressiveVolumesRequestComputeFunc_ApplicationService()
    {
    }

    /// <summary>
    /// Invokes the simple volumes request with the given simple volumes request argument
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public ProgressiveVolumesResponse Invoke(ProgressiveVolumesRequestArgument arg)
    {
      Log.LogInformation($"In {nameof(Invoke)}");

      try
      {
        var executor = new ProgressiveVolumesExecutor();
        return executor.ExecuteAsync(arg).WaitAndUnwrapException();
      }
      finally
      {
        Log.LogInformation($"Exiting {nameof(Invoke)}");
      }
    }
  }
}

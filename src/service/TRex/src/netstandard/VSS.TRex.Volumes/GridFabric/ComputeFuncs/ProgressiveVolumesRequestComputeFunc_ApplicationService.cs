using System;
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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ProgressiveVolumesRequestComputeFunc_ApplicationService>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public ProgressiveVolumesRequestComputeFunc_ApplicationService()
    {
    }

    /// <summary>
    /// Invokes the progressive volumes request with the given progressive volumes request argument
    /// </summary>
    public ProgressiveVolumesResponse Invoke(ProgressiveVolumesRequestArgument arg)
    {
      _log.LogInformation($"In {nameof(Invoke)}");

      try
      {
        // Volumes requests can be a significant resource commitment. Ensure TPaaS will be listening...
        PerformTPaaSRequestLivelinessCheck(arg);

        var executor = new ProgressiveVolumesExecutor();
        return executor.ExecuteAsync(arg).WaitAndUnwrapException();
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception requesting progressive volume at application layer");
        return new ProgressiveVolumesResponse { ResultStatus = Types.RequestErrorStatus.Exception, Volumes = null };
      }
      finally
      {
        _log.LogInformation($"Exiting {nameof(Invoke)}");
      }
    }
  }
}

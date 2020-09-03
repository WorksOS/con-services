using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Executors;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.ComputeFuncs
{
  /// <summary>
  /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
  /// perform sub grid processing.
  /// </summary>
  public class SimpleVolumesRequestComputeFunc_ApplicationService : BaseComputeFunc, IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SimpleVolumesRequestComputeFunc_ApplicationService>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public SimpleVolumesRequestComputeFunc_ApplicationService()
    {
    }

    /// <summary>
    /// Invokes the simple volumes request with the given simple volumes request argument
    /// </summary>
    public SimpleVolumesResponse Invoke(SimpleVolumesRequestArgument arg)
    {
      _log.LogInformation($"In {nameof(Invoke)}");

      try
      {
        // Volumes requests can be a significant resource commitment. Ensure TPaaS will be listening...
        PerformTPaaSRequestLivelinessCheck(arg);

        var executor = new SimpleVolumesExecutor();
        var response = executor.ExecuteAsync(arg).WaitAndUnwrapException();

        _log.LogInformation($"Simple volumes result is {response}");

        return response;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception requesting progressive volume at application layer");
        return new SimpleVolumesResponse { ResponseCode = SubGridRequestsResponseResult.Exception };
      }
      finally
      {
        _log.LogInformation($"Exiting {nameof(Invoke)}");
      }
    }
  }
}

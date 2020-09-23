using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.Exports.Patches.Executors;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;
using System;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// The grid compute function responsible for coordinating sub grids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class PatchRequestComputeFunc : BaseComputeFunc, IComputeFunc<PatchRequestArgument, PatchRequestResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<PatchRequestComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public PatchRequestComputeFunc()
    {
    }

    public PatchRequestResponse Invoke(PatchRequestArgument arg)
    {
      _log.LogInformation("In PatchRequestComputeFunc.Invoke()");

      try
      {
        // Export requests can be a significant resource commitment. Ensure TPaaS will be listening...
        PerformTPaaSRequestLivelinessCheck(arg);

        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // sub grid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);

        _log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        var request = new PatchExecutor(arg.ProjectID, arg.Mode, arg.Filters, arg.ReferenceDesign,
          arg.TRexNodeID, arg.DataPatchNumber, arg.DataPatchSize, arg.LiftParams);

        _log.LogInformation("Executing request.ExecuteAsync()");

        if (!request.ExecuteAsync().WaitAndUnwrapException())
          _log.LogError("Request execution failed");

        return request.PatchSubGridsResponse;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception requesting patch");
        return new PatchRequestResponse { ResultStatus = Types.RequestErrorStatus.Exception };
      }
      finally
      {
        _log.LogInformation("Exiting PatchRequestComputeFunc.Invoke()");
      }
    }
  }
}

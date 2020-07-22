using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.Gridded.Executors;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Reports.Gridded.GridFabric
{
  /// <summary>
  /// The grid compute function responsible for coordinating sub grids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class GriddedReportRequestComputeFunc : BaseComputeFunc, IComputeFunc<GriddedReportRequestArgument, GriddedReportRequestResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<GriddedReportRequestComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public GriddedReportRequestComputeFunc()
    {
    }

    public GriddedReportRequestResponse Invoke(GriddedReportRequestArgument arg)
    {
      _log.LogInformation("In GridRequestComputeFunc.Invoke()");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // sub grid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);
        _log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        var request = new GriddedReportComputeFuncExecutor(arg);

        _log.LogInformation("Executing request.ExecuteAsync()");

        if (!request.ExecuteAsync().WaitAndUnwrapException())
          _log.LogError("Request execution failed");
        
        return request.GriddedReportRequestResponse;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred in GriddedReportRequestComputeFunc.Invoke()");
        return new GriddedReportRequestResponse { ResultStatus = Types.RequestErrorStatus.Exception };
      }
      finally
      {
        _log.LogInformation("Exiting GridRequestComputeFunc.Invoke()");
      }
    }
  }
}

using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.Exports.CSV.Executors;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  /// <summary>
  /// The grid compute function responsible for coordinating sub grids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class CSVExportRequestComputeFunc : BaseComputeFunc, IComputeFunc<CSVExportRequestArgument, CSVExportRequestResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<CSVExportRequestComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public CSVExportRequestComputeFunc()
    {
    }

    public CSVExportRequestResponse Invoke(CSVExportRequestArgument arg)
    {
      _log.LogInformation($"In {nameof(Invoke)}");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // sub grid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);
        _log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        var request = new CSVExportComputeFuncExecutor(arg);

        _log.LogInformation("Executing request.ExecuteAsync()");

        if (!request.ExecuteAsync().WaitAndUnwrapException())
          _log.LogError("Request execution failed");
        
        return request.CSVExportRequestResponse;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception exporting CSV file");
        return new CSVExportRequestResponse { ResultStatus = Types.RequestErrorStatus.Exception };
      }
      finally
      {
        _log.LogInformation($"Out {nameof(Invoke)}");
      }
    }
  }
}

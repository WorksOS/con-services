using System.IO;
using System.Reflection;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.StationOffset.Executors;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Reports.StationOffset.GridFabric
{
  /// <summary>
  /// The StationOffset compute function responsible for coordinating subgrids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class StationOffsetReportRequestComputeFunc : BaseComputeFunc, IComputeFunc<StationOffsetReportRequestArgument, StationOffsetReportRequestResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public StationOffsetReportRequestComputeFunc()
    {
    }

    public StationOffsetReportRequestResponse Invoke(StationOffsetReportRequestArgument arg)
    {
      Log.LogInformation("In StationOffsetRequestComputeFunc.Invoke()");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // subgrid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);
        Log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        var request = new StationOffsetReportComputeFuncExecutor(arg);

        Log.LogInformation("Executing request.Execute()");

        if (!request.Execute())
          Log.LogError($"Request execution failed");
        
        return request.StationOffsetReportRequestResponse;
      }
      finally
      {
        Log.LogInformation("Exiting StationOffsetRequestComputeFunc.Invoke()");
      }
    }
  }
}

using System.IO;
using System.Reflection;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.Gridded.Executors;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Reports.Gridded.GridFabric
{
  /// <summary>
  /// The grid compute function responsible for coordinating subgrids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class GriddedReportRequestComputeFunc : BaseComputeFunc, IComputeFunc<GriddedReportRequestArgument, GriddedReportRequestResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public GriddedReportRequestComputeFunc()
    {
    }

    public GriddedReportRequestResponse Invoke(GriddedReportRequestArgument arg)
    {
      Log.LogInformation("In GridRequestComputeFunc.Invoke()");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // subgrid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);
        Log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        var request = new GriddedReportExecutor(arg);

        Log.LogInformation("Executing request.Execute()");

        if (!request.Execute())
          Log.LogError($"Request execution failed");
        
        return request.GriddedReportRequestResponse;
      }
      finally
      {
        Log.LogInformation("Exiting PatchRequestComputeFunc.Invoke()");
      }
    }
  }
}

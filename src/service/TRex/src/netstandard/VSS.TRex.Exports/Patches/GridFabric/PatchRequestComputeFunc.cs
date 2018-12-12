using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Exports.Patches.Executors;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// The grid compute function responsible for coordinating subgrids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class PatchRequestComputeFunc : BaseComputeFunc, IComputeFunc<PatchRequestArgument, PatchRequestResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public PatchRequestComputeFunc()
    {
    }

    public PatchRequestResponse Invoke(PatchRequestArgument arg)
    {
      Log.LogInformation("In PatchRequestComputeFunc.Invoke()");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // subgrid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);

        Log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        PatchExecutor request = new PatchExecutor(arg.ProjectID, arg.Mode, arg.Filters, arg.ReferenceDesignUID,
          arg.TRexNodeID, arg.DataPatchNumber, arg.DataPatchSize);

        Log.LogInformation("Executing request.Execute()");

        if (!request.Execute())
          Log.LogError($"Request execution failed");

        return request.PatchSubGridsResponse;
      }
      finally
      {
        Log.LogInformation("Exiting PatchRequestComputeFunc.Invoke()");
      }
    }
  }
}

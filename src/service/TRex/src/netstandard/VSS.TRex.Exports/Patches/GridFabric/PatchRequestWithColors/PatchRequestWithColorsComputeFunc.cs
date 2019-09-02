using System.Reflection;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.Exports.Patches.Executors;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Exports.Patches.GridFabric.PatchRequestWithColors
{
  /// <summary>
  /// The grid compute function responsible for coordinating subgrids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class PatchRequestWithColorsComputeFunc : BaseComputeFunc, IComputeFunc<PatchRequestWithColorsArgument, PatchRequestWithColorsResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public PatchRequestWithColorsComputeFunc()
    {
    }

    public PatchRequestWithColorsResponse Invoke(PatchRequestWithColorsArgument arg)
    {
      Log.LogInformation("In PatchRequestWithColorsComputeFunc.Invoke()");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // subgrid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);

        Log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        var request = new PatchWithColorsExecutor(/*arg.ProjectID, arg.Mode, arg.Filters, arg.ReferenceDesign,
          arg.TRexNodeID, arg.DataPatchNumber, arg.DataPatchSize, arg.LiftParams*/);

        Log.LogInformation("Executing request.ExecuteAsync()");

        if (!request.ExecuteAsync().WaitAndUnwrapException())
          Log.LogError($"Request execution failed");

        return request.PatchSubGridsResponse;
      }
      finally
      {
        Log.LogInformation("Exiting PatchRequestWithColorsComputeFunc.Invoke()");
      }
    }
  }
}

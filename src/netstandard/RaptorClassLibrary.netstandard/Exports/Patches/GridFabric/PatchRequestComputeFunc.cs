using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Rendering.Patches.Executors;
using VSS.TRex.Servers;

namespace VSS.TRex.Exports.Patches.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The grid compute function responsible for coordinating subgrids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  [Serializable]
  public class PatchRequestComputeFunc : BaseComputeFunc, IComputeFunc<PatchRequestArgument, PatchRequestResponse>
  {
    [NonSerialized]
    private static readonly ILogger
      Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Default no-arg constructor that orients the request to the available ASNODE servers on the immutable grid projection
    /// </summary>
    public PatchRequestComputeFunc() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }

    public PatchRequestResponse Invoke(PatchRequestArgument arg)
    {
      Log.LogInformation("In PatchRequestComputeFunc.Invoke()");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // subgrid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(Storage.StorageMutability.Immutable);

        Log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        PatchExecutor request = new PatchExecutor(arg.DataModelID, arg.Mode, arg.Filters, arg.CutFillDesignID,
          arg.TRexNodeID, arg.DataPatchNumber, arg.DataPatchSize);

        Log.LogInformation("Executing request.Execute()");

        request.Execute();
        return request.PatchSubGridsResponse;
      }
      finally
      {
        Log.LogInformation("Exiting PatchRequestComputeFunc.Invoke()");
      }
    }
  }
}

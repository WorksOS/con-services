using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.QuantizedMesh.GridFabric.Arguments;
using VSS.TRex.QuantizedMesh.GridFabric.Responses;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;
using VSS.TRex.QuantizedMesh.Executors;
using VSS.TRex.IO.Helpers;

namespace VSS.TRex.QuantizedMesh.GridFabric.ComputeFuncs
{
  public class QuantizedMeshRequestComputeFunc : BaseComputeFunc, IComputeFunc<QuantizedMeshRequestArgument, QuantizedMeshResponse>
  {

    private static readonly ILogger Log = Logging.Logger.CreateLogger<QuantizedMeshRequestComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public QuantizedMeshRequestComputeFunc()
    {
    }

    /// <summary>
    /// Quantized Mesh Response.
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public QuantizedMeshResponse Invoke(QuantizedMeshRequestArgument arg)
    {

      Log.LogInformation("In QuantizedMeshRequestComputeFunc.Invoke()");

      try
      {
        
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // subgrid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);

        Log.LogDebug($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        var request = new QMTileExecutor(arg.ProjectID, arg.Filters, arg.X, arg.Y, arg.Z, arg.DisplayMode, arg.HasLighting, arg.TRexNodeID);

        Log.LogInformation("Executing request.Execute()");

        if (!request.ExecuteAsync().WaitAndUnwrapException())
          Log.LogError("Request execution failed");

        return request.QMTileResponse; 

      }
      finally
      {
        Log.LogDebug($"Exiting QuantizedMeshRequestComputeFunc.Invoke()");
      }
    }

  }
}

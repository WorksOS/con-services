using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Geometry;
using VSS.TRex.QuantizedMesh.Executors;
using VSS.TRex.QuantizedMesh.GridFabric.Arguments;
using VSS.TRex.QuantizedMesh.GridFabric.Responses;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;
using VSS.TRex.QuantizedMesh.Abstractions;
using VSS.TRex.GridFabric.ComputeFuncs;

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

    public QuantizedMeshResponse Invoke(QuantizedMeshRequestArgument arg)
    {
      DateTime startTime = DateTime.UtcNow;

      Log.LogInformation("In QuantizedMeshRequestComputeFunc.Invoke()");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // sub grid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);

        Log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        RenderQMTile render = new RenderQMTile
            (arg.ProjectID,
             new XYZ(arg.Extents.MinX, arg.Extents.MinY),
             new XYZ(arg.Extents.MaxX, arg.Extents.MaxY),
             arg.CoordsAreGrid,
             arg.Filters,
             arg.TRexNodeID);

        Log.LogInformation("Executing render.Execute()");

        IQuantizedMeshTile qm = render.Execute();
        

        Log.LogInformation($"Render status = {render.ResultStatus}");

        if (qm == null)
        {
          Log.LogInformation("Null quantized mesh returned by executor");
        }

        //var response = new QuantizedMeshResponse();
        var response = new DummyQMResponse();
   //     response.TileQMData = qm;
        return response;
      }
      finally
      {
        Log.LogInformation($"Exiting QuantizedMeshRequestComputeFunc.Invoke() in {DateTime.UtcNow - startTime}");
      }
    }

  }
}

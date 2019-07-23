using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System.Drawing;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Executors;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The grid compute function responsible for coordinating rendering of a tile on a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class TileRenderRequestComputeFunc : BaseComputeFunc, IComputeFunc<TileRenderRequestArgument, TileRenderResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<TileRenderRequestComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public TileRenderRequestComputeFunc()
    {
    }

    public TileRenderResponse Invoke(TileRenderRequestArgument arg)
    {
      var startTime = DateTime.UtcNow;

      Log.LogInformation("In TileRenderRequestComputeFunc.Invoke()");

      try
      {
        // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
        // sub grid results to it.
        arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);

        Log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

        var render = new RenderOverlayTile
            (arg.ProjectID,
             arg.Mode,
             new XYZ(arg.Extents.MinX, arg.Extents.MinY),
             new XYZ(arg.Extents.MaxX, arg.Extents.MaxY),
             arg.CoordsAreGrid,
             arg.PixelsX, arg.PixelsY,
             arg.Filters,
             arg.ReferenceDesign,
             arg.Palette,
             Color.Black,
             arg.TRexNodeID);

        Log.LogInformation("Executing render.ExecuteAsync()");

        var bmp = render.ExecuteAsync().WaitAndUnwrapException();
        Log.LogInformation($"Render status = {render.ResultStatus}");

        if (bmp == null)
        {
          Log.LogInformation("Null bitmap returned by executor");
        }

        // Get the rendering factory from the DI context
        var RenderingFactory = DIContext.Obtain<IRenderingFactory>();
        var response = RenderingFactory.CreateTileRenderResponse(bmp?.GetBitmap()) as TileRenderResponse;
        if (response != null)
          response.ResultStatus = render.ResultStatus;

        return response;
      }
      finally
      {
         Log.LogInformation($"Exiting TileRenderRequestComputeFunc.Invoke() in {DateTime.UtcNow - startTime}");
      }
    }
  }
}

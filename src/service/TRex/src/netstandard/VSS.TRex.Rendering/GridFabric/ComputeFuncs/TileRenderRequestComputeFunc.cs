﻿using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System.Drawing;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.Executors;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;
using SkiaSharp;
using VSS.TRex.IO.Helpers;
using System.Diagnostics;

namespace VSS.TRex.Rendering.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The grid compute function responsible for coordinating rendering of a tile on a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class TileRenderRequestComputeFunc : BaseComputeFunc, IComputeFunc<TileRenderRequestArgument, TileRenderResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<TileRenderRequestComputeFunc>();

    // Warn on tile renders that take more than this time to service (20 seconds)
    private static readonly TimeSpan _tileRequestTimeSpanWarnLimit = new TimeSpan(0, 0, 20);

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public TileRenderRequestComputeFunc()
    {
    }

    public TileRenderResponse Invoke(TileRenderRequestArgument arg)
    {
      try
      {
        // Tile requests can be a significant resource commitment. Ensure TPaaS will be listening...
        PerformTPaaSRequestLivelinessCheck(arg);

        var requestStopWatch = Stopwatch.StartNew();

        _log.LogInformation("In TileRenderRequestComputeFunc.Invoke()");

        try
        {
          // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
          // sub grid results to it.
          arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);

          _log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

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
            arg.TRexNodeID,
            arg.LiftParams,
            arg.VolumeType);

          _log.LogInformation("Executing render.ExecuteAsync()");

          using var bmp = render.ExecuteAsync().WaitAndUnwrapException();
          _log.LogInformation($"Render status = {render.ResultStatus}");

          if (bmp == null)
          {
            _log.LogInformation("Null bitmap returned by executor");

            return new TileRenderResponse
            {
              TileBitmapData = null,
              ResultStatus = render.ResultStatus
            };
          }

          using var image = SKImage.FromBitmap(bmp);
          using var data = image.Encode(SKEncodedImageFormat.Png, 100);
          using var stream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();
          data.SaveTo(stream);

          return new TileRenderResponse
          {
            TileBitmapData = stream.ToArray(),
            ResultStatus = render.ResultStatus
          };
        }
        finally
        {
          _log.LogInformation($"Exiting TileRenderRequestComputeFunc.Invoke() in {requestStopWatch.Elapsed}");

          // Flag tile renders that take more than 20 seconds to render...
          if (requestStopWatch.Elapsed > _tileRequestTimeSpanWarnLimit)
          {
            _log.LogInformation($"Tile render request required more than {_tileRequestTimeSpanWarnLimit} to complete");
          }
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred in TileRenderRequestComputeFunc.Invoke()");
        return new TileRenderResponse { ResultStatus = Types.RequestErrorStatus.Exception };
      }
    }
  }
}

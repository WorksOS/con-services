using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Draw = System.Drawing;
using System.Reflection;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Executors;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Servers;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.Rendering.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The grid compute function responsible for coordinating rendering of a tile on a server compute node in response to 
    /// a client server instance requesting it.
    /// </summary>
    public class TileRenderRequestComputeFunc : BaseComputeFunc, IComputeFunc<TileRenderRequestArgument, TileRenderResponse>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Default no-arg constructor that orients the request to the available ASNODE servers on the immutable grid projection
        /// </summary>
        public TileRenderRequestComputeFunc() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
        {
        }

        public TileRenderResponse Invoke(TileRenderRequestArgument arg)
        {
            Log.LogInformation("In TileRenderRequestComputeFunc.Invoke()");

            try
            {
                // Supply the TRex ID of the Ignite node currently running this code to permit processing contexts to send
                // subgrid results to it.
                arg.TRexNodeID = TRexNodeID.ThisNodeID(StorageMutability.Immutable);

                Log.LogInformation($"Assigned TRexNodeId from local node is {arg.TRexNodeID}");

                RenderOverlayTile render = new RenderOverlayTile
                    (arg.ProjectID,
                     arg.Mode,
                     new XYZ(arg.Extents.MinX, arg.Extents.MinY),
                     new XYZ(arg.Extents.MaxX, arg.Extents.MaxY),
                     arg.CoordsAreGrid,
                     arg.PixelsX, arg.PixelsY,
                     arg.Filter1, arg.Filter2,
                     arg.ReferenceDesignID,
                     Draw.Color.Black,
                     arg.TRexNodeID);

                Log.LogInformation("Executing render.Execute()");

                IBitmap bmp = render.Execute();
                Log.LogInformation($"Render status = {render.ResultStatus}");

                if (bmp == null)
                {
                    Log.LogInformation("Null bitmap returned by executor");
                }

                // Get the rendering factory from the DI context
                IRenderingFactory RenderingFactory = DIContext.Obtain<IRenderingFactory>();
                return RenderingFactory.CreateTileRenderResponse(bmp?.GetBitmap()) as TileRenderResponse;
            }
            finally
            {
                Log.LogInformation("Exiting TileRenderRequestComputeFunc.Invoke()");
            }
        }
    }
}

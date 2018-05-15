using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Drawing;
using System.Reflection;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Executors;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Servers;

namespace VSS.TRex.Rendering.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The grid compute function responsible for coordinating rendering of a tile on a server compute node in response to 
    /// a client server instance requesting it.
    /// </summary>
    [Serializable]
    public class TileRenderRequestComputeFunc : BaseRaptorComputeFunc, IComputeFunc<TileRenderRequestArgument, TileRenderResponse>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default no-arg constructor that orients the request to the available ASNODE servers on the immutable grid projection
        /// </summary>
        public TileRenderRequestComputeFunc() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
        {
        }

        public TileRenderResponse Invoke(TileRenderRequestArgument arg)
        {
            Log.Info("In TileRenderRequestComputeFunc.Invoke()");

            try
            {
                // Supply the Raptor ID of the Ignite node currently running this code to permit processing contexts to send
                // subgrid results to it.
                arg.RaptorNodeID = RaptorNodeID.ThisNodeID(Storage.StorageMutability.Immutable);

                Log.InfoFormat("Assigned RaptorNodeID from local node is {0}", arg.RaptorNodeID);

                RenderOverlayTile render = new RenderOverlayTile
                    (arg.SiteModelID,
                     arg.Mode,
                     new XYZ(arg.Extents.MinX, arg.Extents.MinY),
                     new XYZ(arg.Extents.MaxX, arg.Extents.MaxY),
                     arg.CoordsAreGrid,
                     arg.PixelsX, arg.PixelsY,
                     arg.Filter1, arg.Filter2,
                     arg.CutFillDesignID,//arg.CutFillDesign,
                     Color.Black,
                     arg.RaptorNodeID);

                Log.Info("Executing render.Execute()");

                IBitmap bmp = render.Execute();
                Log.Info($"Render status = {render.ResultStatus}");

                if (bmp == null)
                {
                    Log.Info("Null bitmap returned by executor");
                }

                return DIContext.RenderingFactory.CreateTileRenderResponse(bmp?.GetBitmap()) as TileRenderResponse;
            }
            finally
            {
                Log.Info("Exiting TileRenderRequestComputeFunc.Invoke()");
            }
        }
    }
}

using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The grid compute function responsible for coordinating rendering of a tile on a server compute node in response to 
    /// a client server instance requesting it.
    /// </summary>
    [Serializable]
    public class TileRenderRequestComputeFunc : IComputeFunc<TileRenderRequestArgument, Bitmap>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Bitmap Invoke(TileRenderRequestArgument arg)
        {
            Log.Info("In TileRenderRequestComputeFunc.Invoke()");

            try
            {
                // Supply the Raptor OF the Ignite node currently running this code to permit processing contexts to send
                // subgrid results to it.
                arg.RaptorNodeID = Ignition.GetIgnite(RaptorGrids.RaptorGridName()).GetCluster().GetLocalNode().GetAttribute<string>("RaptorNodeID");

                Log.InfoFormat("Assigned RaptorNodeID from local node is {0}", arg.RaptorNodeID);

                RenderOverlayTile render = new RenderOverlayTile
                    (arg.SiteModelID,
                     arg.Mode,
                     new XYZ(arg.Extents.MinX, arg.Extents.MinY),
                     new XYZ(arg.Extents.MaxX, arg.Extents.MaxY),
                     arg.CoordsAreGrid,
                     arg.PixelsX, arg.PixelsY,
                     arg.Filter1, arg.Filter2,
                     arg.RaptorNodeID);

                Log.Info("Executing render.Execute()");

                return render.Execute();
            }
            finally
            {
                Log.Info("Exiting TileRenderRequestComputeFunc.Invoke()");
            }
        }
    }
}

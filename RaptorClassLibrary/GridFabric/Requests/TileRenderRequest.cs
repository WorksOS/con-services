using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
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
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// Sends a request to the grid for a tile to be rendered
    /// </summary>
    public class TileRenderRequest : ApplicationServicePoolRequest
    {
        /// <summary>
        /// Renders a bitmap according to the parameters in its argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public Bitmap Execute(TileRenderRequestArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<TileRenderRequestArgument, Bitmap> func = new TileRenderRequestComputeFunc();

            Task<Bitmap> taskResult = _compute.ApplyAsync(func, arg);

            //Bitmap result = compute.Apply(func, arg);

            // Send the appropriate response to the caller
            return taskResult.Result;
        }
    }
}

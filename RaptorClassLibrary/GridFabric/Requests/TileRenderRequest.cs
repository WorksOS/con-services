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
    public static class TileRenderRequest
    {
        public static Bitmap Execute(TileRenderRequestArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<TileRenderRequestArgument, Bitmap> func = new TileRenderRequestComputeFunc();

            // Get a reference to the Ignite cluster
            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorGridName());

            // Get a reference to the compute cluster group and send the request to it for processing
            // Note: Broadcast will block until all compute nodes receiving the request have responded, or
            // until the internal Ignite timeout expires

            IClusterGroup group = ignite.GetCluster().ForRemotes().ForServers().ForAttribute("Role", "ASNode");
            ICompute compute = group.GetCompute();

            Task<Bitmap> taskResult = compute.ApplyAsync(func, arg);

            //Bitmap result = compute.Apply(func, arg);

            // Send the appropriate response to the caller
            return taskResult.Result;
        }
    }
}

using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.DesignProfiling.GridFabric.Requests;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.Surfaces.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Surfaces.GridFabric.ComputeFuncs;

namespace VSS.VisionLink.Raptor.Surfaces.GridFabric.Requests
{
    public class SurfaceElevationPatchRequest : DesignProfilerRaptorRequest
    {
        public ClientHeightAndTimeLeafSubGrid Execute(SurfaceElevationPatchArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<SurfaceElevationPatchArgument, byte[] /*ClientHeightAndTimeLeafSubGrid*/> func = new SurfaceElevationPatchComputeFunc();

            /*ClientHeightAndTimeLeafSubGrid */ byte[] result = _compute.Apply(func, arg);

            if (result == null)
            {
                return null;
            }

            ClientHeightAndTimeLeafSubGrid clientResult = new ClientHeightAndTimeLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, SubGridTree.DefaultCellSize, SubGridTree.DefaultIndexOriginOffset);
            clientResult.FromByteArray(result);
            return clientResult;

            //  Task<ClientHeightAndTimeLeafSubGrid> taskResult = compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            //            return taskResult.Result;
        }

    }
}

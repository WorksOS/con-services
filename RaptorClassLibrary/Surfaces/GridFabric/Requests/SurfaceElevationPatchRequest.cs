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
        private static SurveyedSurfaceResultCache _cache = new SurveyedSurfaceResultCache();

        public SurfaceElevationPatchRequest()
        {

        }

        public ClientHeightAndTimeLeafSubGrid Execute(SurfaceElevationPatchArgument arg)
        {
            // Check the item is available in the cache
            ClientHeightAndTimeLeafSubGrid clientResult = _cache.get(arg);

            if (clientResult != null)
            {
                // It was presnet in the cache, return it
                return clientResult;
            }

            // Request the subgrid from the surveyed surface engine

            // Construct the function to be used
            IComputeFunc<SurfaceElevationPatchArgument, byte[] /*ClientHeightAndTimeLeafSubGrid*/> func = new SurfaceElevationPatchComputeFunc();

            /*ClientHeightAndTimeLeafSubGrid */ byte[] result = _compute.Apply(func, arg);

            if (result == null)
            {
                return null;
            }

            clientResult = new ClientHeightAndTimeLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, SubGridTree.DefaultCellSize, SubGridTree.DefaultIndexOriginOffset);
            clientResult.FromBytes(result);

            _cache.Put(arg, clientResult);

            return clientResult;

            //  Task<ClientHeightAndTimeLeafSubGrid> taskResult = compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            // return taskResult.Result;
        }

    }
}

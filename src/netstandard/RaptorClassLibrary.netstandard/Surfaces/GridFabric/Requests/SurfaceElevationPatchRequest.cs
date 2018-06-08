using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Surfaces.GridFabric.Arguments;
using VSS.TRex.Surfaces.GridFabric.ComputeFuncs;
using VSS.TRex.Types;
using VSS.VisionLink.DesignProfiling.GridFabric.Requests;

namespace VSS.TRex.Surfaces.GridFabric.Requests
{
    public class SurfaceElevationPatchRequest : DesignProfilerRequest<SurfaceElevationPatchArgument, IClientLeafSubGrid>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Shared static cache of surface elevation subgrids
        /// </summary>
        private static SurveyedSurfaceResultCache _cache = new SurveyedSurfaceResultCache();

        /// <summary>
        /// Local reference to the client subgrid factory
        /// </summary>
        [NonSerialized]
        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.Factory();

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SurfaceElevationPatchRequest()
        {
        }

        public override IClientLeafSubGrid Execute(SurfaceElevationPatchArgument arg)
        {
            // Check the item is available in the cache
            ClientHeightAndTimeLeafSubGrid cachedResult = _cache.Get(arg);

            if (cachedResult != null)
            {
                // It was present in the cache, return it
                return cachedResult;
            }

            // Request the subgrid from the surveyed surface engine

            // Construct the function to be used, but override the procesing map in the argument to specify that all cells are required as the result 
            // will be cached
            IComputeFunc<SurfaceElevationPatchArgument, byte[]> func = new SurfaceElevationPatchComputeFunc();

            byte[] result = null;
            arg.ProcessingMap.Fill();

            try
            {
                result = _Compute.Apply(func, arg);
            }
            catch (ClusterGroupEmptyException e)
            {
                Log.LogWarning($"Grid error, retrying: {typeof(SurfaceElevationPatchRequest)} threw {typeof(ClusterGroupEmptyException)}\n:{e}");
                AcquireIgniteTopologyProjections();

                try
                {
                    result = _Compute.Apply(func, arg);
                }
                catch (ClusterGroupEmptyException e2)
                {
                    Log.LogError($"Grid error, failing: {typeof(SurfaceElevationPatchRequest)} threw {typeof(ClusterGroupEmptyException)}\n:{e2}");
                }
            }

            if (result == null)
            {
                return null;
            }

            IClientLeafSubGrid clientResult = ClientLeafSubGridFactory.GetSubGrid(arg.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations ? GridDataType.CompositeHeights : GridDataType.HeightAndTime);
            clientResult.FromBytes(result);

            // Fow now, only cache non-composite elevation subgrids
            if (arg.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations)
              _cache.Put(arg, clientResult as ClientHeightAndTimeLeafSubGrid);

            return clientResult;

            //  Task<ClientHeightAndTimeLeafSubGrid> taskResult = compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            // return taskResult.Result;
        }
    }
}

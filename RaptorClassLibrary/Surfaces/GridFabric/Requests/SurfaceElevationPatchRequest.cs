using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.DesignProfiling.GridFabric.Requests;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Surfaces.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Surfaces.GridFabric.ComputeFuncs;

namespace VSS.VisionLink.Raptor.Surfaces.GridFabric.Requests
{
    public class SurfaceElevationPatchRequest : DesignProfilerRaptorRequest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Shared static cache of surface elevation subgrids
        /// </summary>
        private static SurveyedSurfaceResultCache _cache = new SurveyedSurfaceResultCache();

        [NonSerialized]
        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SurfaceElevationPatchRequest()
        {

        }

        public ClientHeightAndTimeLeafSubGrid Execute(SurfaceElevationPatchArgument arg)
        {
            // Check the item is available in the cache
            ClientHeightAndTimeLeafSubGrid cachedResult = _cache.get(arg);

            if (cachedResult != null)
            {
                // It was presnet in the cache, return it
                return cachedResult;

                //ClientHeightAndTimeLeafSubGrid clientResultFromCache = ClientLeafSubGridFactory.GetSubGrid(Types.GridDataType.HeightAndTime) as ClientHeightAndTimeLeafSubGrid;
                //return clientResultFromCache;
            }

            // Request the subgrid from the surveyed surface engine

            // Construct the function to be used, but override the procesing map in the argument to specify that all cells are required as the result 
            // will be cached
            IComputeFunc<SurfaceElevationPatchArgument, byte[] /*ClientHeightAndTimeLeafSubGrid*/> func = new SurfaceElevationPatchComputeFunc();

            byte[] result = null;
            arg.ProcessingMap.Fill();

            try
            {
                /*ClientHeightAndTimeLeafSubGrid */ result = _Compute.Apply(func, arg);
            }
            catch (ClusterGroupEmptyException e)
            {
                Log.Warn($"Grid error, retrying: {typeof(SurfaceElevationPatchRequest)} threw {typeof(ClusterGroupEmptyException)}\n:{e}");
                AcquireIgniteTopologyProjections();

                try
                {
                    /*ClientHeightAndTimeLeafSubGrid */ result = _Compute.Apply(func, arg);
                }
                catch (ClusterGroupEmptyException e2)
                {
                    Log.Error($"Grid error, failing: {typeof(SurfaceElevationPatchRequest)} threw {typeof(ClusterGroupEmptyException)}\n:{e2}");
                }
            }

            if (result == null)
            {
                return null;
            }

            ClientHeightAndTimeLeafSubGrid clientResult = ClientLeafSubGridFactory.GetSubGrid(Types.GridDataType.HeightAndTime) as ClientHeightAndTimeLeafSubGrid;
            clientResult.FromBytes(result);

            _cache.Put(arg, clientResult);

            return clientResult;

            //  Task<ClientHeightAndTimeLeafSubGrid> taskResult = compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            // return taskResult.Result;
        }
    }
}

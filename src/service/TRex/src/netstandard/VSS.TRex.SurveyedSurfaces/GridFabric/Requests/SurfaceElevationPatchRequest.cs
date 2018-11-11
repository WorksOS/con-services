using System;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.Requests
{
    public class SurfaceElevationPatchRequest : DesignProfilerRequest<SurfaceElevationPatchArgument, IClientLeafSubGrid>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Reference to the general subgrid result cache
        /// </summary>
        private ITRexSpatialMemoryCache _cache;

        /// <summary>
        /// The cache context to be used for all requests made through this request instance
        /// </summary>
        private ITRexSpatialMemoryCacheContext _context;

        /// <summary>
        /// Local reference to the client subgrid factory
        /// </summary>
        private static IClientLeafSubgridFactory clientLeafSubGridFactory;

        private IClientLeafSubgridFactory ClientLeafSubGridFactory
          => clientLeafSubGridFactory ?? (clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubgridFactory>());

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SurfaceElevationPatchRequest(Guid projectUid, string cacheFingerprint)
        {
          _cache = DIContext.Obtain<ITRexSpatialMemoryCache>();
          _context = _cache?.LocateOrCreateContext(projectUid, cacheFingerprint);
        }
     
        private SurfaceElevationPatchRequest()
        {
        }

        public override IClientLeafSubGrid Execute(SurfaceElevationPatchArgument arg)
        {
            // Check the item is available in the cache
            if (_context?.Get(arg.OTGCellBottomLeftX, arg.OTGCellBottomLeftY) is ClientHeightAndTimeLeafSubGrid cachedResult)
            {
                // It was present in the cache, return it
                return cachedResult;
            }

            // Request the subgrid from the surveyed surface engine

            // Construct the function to be used, but override the processing map in the argument to specify that all cells are required as the result 
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
              if (_context != null)
                _cache?.Add(_context, clientResult);

            return clientResult;

            //  Task<ClientHeightAndTimeLeafSubGrid> taskResult = compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            // return taskResult.Result;
        }
    }
}

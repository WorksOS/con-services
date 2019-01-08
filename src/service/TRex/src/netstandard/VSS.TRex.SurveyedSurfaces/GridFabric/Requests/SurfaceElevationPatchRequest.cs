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
        private readonly ITRexSpatialMemoryCache _cache;

        /// <summary>
        /// The cache context to be used for all requests made through this request instance
        /// </summary>
        private readonly ITRexSpatialMemoryCacheContext _context;

        /// <summary>
        /// Local reference to the client subgrid factory
        /// </summary>
        private static IClientLeafSubGridFactory clientLeafSubGridFactory;

        private IClientLeafSubGridFactory ClientLeafSubGridFactory
          => clientLeafSubGridFactory ?? (clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>());

        /// <summary>
        /// The static compute function used for surface elevation patch requests
        /// </summary>
        private static readonly IComputeFunc<SurfaceElevationPatchArgument, byte[]> _computeFunc = new SurfaceElevationPatchComputeFunc();

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SurfaceElevationPatchRequest(ITRexSpatialMemoryCache cache, ITRexSpatialMemoryCacheContext context) : this()
        {
          _cache = cache; 
          _context = context; 
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
            byte[] result = null;
            arg.ProcessingMap.Fill();

            try
            {
                result = _Compute.Apply(_computeFunc, arg);
            }
            catch (ClusterGroupEmptyException e)
            {
                Log.LogError(e, $"Grid error, retrying: {typeof(SurfaceElevationPatchRequest)} threw {typeof(ClusterGroupEmptyException)}:");
                AcquireIgniteTopologyProjections();

                try
                {
                    result = _Compute.Apply(_computeFunc, arg);
                }
                catch (ClusterGroupEmptyException e2)
                {
                    Log.LogError(e2, $"Grid error, failing: {typeof(SurfaceElevationPatchRequest)} threw {typeof(ClusterGroupEmptyException)}:");
                }
            }

            if (result == null)
            {
                return null;
            }

            IClientLeafSubGrid clientResult = ClientLeafSubGridFactory.GetSubGrid(arg.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations ? GridDataType.CompositeHeights : GridDataType.HeightAndTime);
            clientResult.FromBytes(result);

            // Fow now, only cache non-composite elevation subgrids
            if (arg.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations && _context != null)
                _cache?.Add(_context, clientResult);

            return clientResult;

            //  Task<ClientHeightAndTimeLeafSubGrid> taskResult = compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
            // return taskResult.Result;
        }
    }
}

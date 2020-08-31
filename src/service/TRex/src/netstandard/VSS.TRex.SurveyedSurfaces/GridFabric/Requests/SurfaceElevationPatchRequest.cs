using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.Requests
{
  public class SurfaceElevationPatchRequest : DesignProfilerServicePoolRequest<ISurfaceElevationPatchArgument, IClientLeafSubGrid>, ISurfaceElevationPatchRequest
  {
    private readonly SurfaceElevationPatchRequestCache _cache;

    /// <summary>
    /// Reference to the client sub grid factory
    /// </summary>
    private readonly IClientLeafSubGridFactory _clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>();

    /// <summary>
    /// The compute function used for surface elevation patch requests
    /// </summary>
    private readonly IComputeFunc<ISurfaceElevationPatchArgument, ISerialisedByteArrayWrapper> _computeFunc = new SurfaceElevationPatchComputeFunc();

    public SurfaceElevationPatchRequest(ITRexSpatialMemoryCache cache, ITRexSpatialMemoryCacheContext context) : this()
    {
      if (cache != null && context != null)
        _cache = new SurfaceElevationPatchRequestCache(cache, context, _clientLeafSubGridFactory);
    }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SurfaceElevationPatchRequest()
    {
    }

    public override IClientLeafSubGrid Execute(ISurfaceElevationPatchArgument arg)
    {
      var cachingSupported = arg.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations && _cache != null;

      // Check the item is available in the cache
      if (cachingSupported && _cache?.Get(arg.OTGCellBottomLeftX, arg.OTGCellBottomLeftY) is IClientLeafSubGrid cacheResult)
        return _cache.ExtractFromCachedItem(cacheResult, arg.ProcessingMap, arg.SurveyedSurfacePatchType);

      SubGridTreeBitmapSubGridBits savedMap = null;

      // Always request the full sub grid from the surveyed surface engine unless composite elevations are requested
      if (cachingSupported)
      {
        savedMap = arg.ProcessingMap;
        arg.ProcessingMap = SubGridTreeBitmapSubGridBits.FullMask;
      }

      var subGridInvalidationVersion = cachingSupported ? _cache.InvalidationVersion : 0;

      IClientLeafSubGrid clientResult = null;
      var result = Compute.Apply(_computeFunc, arg);

      if (result?.Bytes != null)
      {
        clientResult = _clientLeafSubGridFactory.GetSubGrid(arg.SurveyedSurfacePatchType == SurveyedSurfacePatchType.CompositeElevations ? GridDataType.CompositeHeights : GridDataType.HeightAndTime);
        clientResult.FromBytes(result.Bytes);

        // For now, only cache non-composite elevation sub grids
        if (cachingSupported)
          _cache?.Add(clientResult, subGridInvalidationVersion);

        if (savedMap != null)
          clientResult = _cache.ExtractFromCachedItem(clientResult, savedMap, arg.SurveyedSurfacePatchType);
      }

      return clientResult;
    }

    public override Task<IClientLeafSubGrid> ExecuteAsync(ISurfaceElevationPatchArgument arg)
    {
      return Task.Run(() => Execute(arg));
    }
  }
}

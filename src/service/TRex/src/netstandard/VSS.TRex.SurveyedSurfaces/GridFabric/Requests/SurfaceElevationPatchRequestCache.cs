using VSS.TRex.Caching.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.Requests
{
  /// <summary>
  /// Provides a cache suitable for use for elevation results from surveyed sub grid requests
  /// </summary>
  public class SurfaceElevationPatchRequestCache
  {
    /// <summary>
    /// Reference to the general sub grid result cache
    /// </summary>
    private readonly ITRexSpatialMemoryCache _cache;

    /// <summary>
    /// The cache context to be used for all requests made through this request instance
    /// </summary>
    private readonly ITRexSpatialMemoryCacheContext _context;

    private readonly IClientLeafSubGridFactory _clientLeafSubGridFactory;

    public SurfaceElevationPatchRequestCache(ITRexSpatialMemoryCache cache, ITRexSpatialMemoryCacheContext context, IClientLeafSubGridFactory clientLeafSubGridFactory)
    {
      _cache = cache;
      _context = context;
      _clientLeafSubGridFactory = clientLeafSubGridFactory;
    }

    public IClientLeafSubGrid ExtractFromCachedItem(IClientLeafSubGrid cachedItem, SubGridTreeBitmapSubGridBits map, SurveyedSurfacePatchType patchType)
    {
      var resultItem = _clientLeafSubGridFactory.GetSubGridEx
      (patchType == SurveyedSurfacePatchType.CompositeElevations ? GridDataType.CompositeHeights : GridDataType.HeightAndTime,
        cachedItem.CellSize, cachedItem.Level, cachedItem.OriginX, cachedItem.OriginY);
      resultItem.AssignFromCachedPreProcessedClientSubGrid(cachedItem, map);

      return resultItem;
    }

    public ITRexMemoryCacheItem Get(int otgCellBottomLeftX, int otgCellBottomLeftY)
    {
      return _cache.Get(_context, otgCellBottomLeftX, otgCellBottomLeftY);
    }

    public CacheContextAdditionResult Add(IClientLeafSubGrid clientResult, long subGridInvalidationVersion)
    {
      return _cache.Add(_context, clientResult, subGridInvalidationVersion);
    }

    public long InvalidationVersion => _context.InvalidationVersion;
  }
}

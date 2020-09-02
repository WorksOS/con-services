using System.Threading.Tasks;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces.Executors;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.Requests
{
  public class SurfaceElevationPatchRequestViaLocalCompute : ISurfaceElevationPatchRequest
  {
    private readonly SurfaceElevationPatchRequestCache _cache;

    /// <summary>
    /// Reference to the client sub grid factory
    /// </summary>
    private readonly IClientLeafSubGridFactory _clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>();

    /// <summary>
    /// Design files references kept for the duration of the request utilizing this local compute scope
    /// </summary>
    private IDesignFiles _designFiles;

    /// <summary>
    /// Site model reference kept for the duration of the request utilizing this local compute scope
    /// </summary>
    private ISiteModel _sitemodel;

    /// <summary>
    /// Surveyed surfaces references kept for the duration of the request utilizing this local compute scope
    /// </summary>
    private ISurveyedSurfaces _surveyedSurfaces;

    public SurfaceElevationPatchRequestViaLocalCompute()
    {
    }

    public SurfaceElevationPatchRequestViaLocalCompute(ITRexSpatialMemoryCache cache, ITRexSpatialMemoryCacheContext context) : this()
    {
      if (cache != null && context != null)
        _cache = new SurfaceElevationPatchRequestCache(cache, context, _clientLeafSubGridFactory);
    }

    public async Task<IClientLeafSubGrid> ExecuteAsync(ISurfaceElevationPatchArgument arg) => Execute(arg); // Task.Run(() => Execute(arg));

    public IClientLeafSubGrid Execute(ISurfaceElevationPatchArgument arg)
    {
      if (arg.SurveyedSurfacePatchType > SurveyedSurfacePatchType.CompositeElevations)
        return null;

      var cachingSupported = arg.SurveyedSurfacePatchType != SurveyedSurfacePatchType.CompositeElevations && _cache != null;

      // Check the item is available in the cache
      if (cachingSupported && _cache?.Get(arg.OTGCellBottomLeftX, arg.OTGCellBottomLeftY) is IClientLeafSubGrid cacheResult)
      {
        return _cache.ExtractFromCachedItem(cacheResult, arg.ProcessingMap, arg.SurveyedSurfacePatchType);
      }

      SubGridTreeBitmapSubGridBits savedMap = null;

      // Always request the full sub grid from the surveyed surface engine unless composite elevations are requested
      if (cachingSupported)
      {
        savedMap = arg.ProcessingMap;
        arg.ProcessingMap = SubGridTreeBitmapSubGridBits.FullMask;
      }

      var subGridInvalidationVersion = cachingSupported ? _cache.InvalidationVersion : 0;

      var executor = new CalculateSurfaceElevationPatch();

      _sitemodel ??= DIContext.ObtainRequired<ISiteModels>().GetSiteModel(arg.SiteModelID);
      _designFiles ??= DIContext.ObtainRequired<IDesignFiles>();
      _surveyedSurfaces ??= _sitemodel.SurveyedSurfaces;

      var clientResult = executor.Execute(_sitemodel, arg.OTGCellBottomLeftX, arg.OTGCellBottomLeftY,
        arg.CellSize, arg.SurveyedSurfacePatchType, arg.IncludedSurveyedSurfaces,
        _designFiles, _surveyedSurfaces, arg.ProcessingMap);

      if (clientResult != null)
      {
        // For now, only cache non-composite elevation sub grids
        if (cachingSupported)
          _cache?.Add(clientResult, subGridInvalidationVersion);

        if (savedMap != null)
          clientResult = _cache.ExtractFromCachedItem(clientResult, savedMap, arg.SurveyedSurfacePatchType);
      }

      return clientResult;
    }

    /// <summary>
    /// No implementation for local compute requests
    /// </summary>
    public void WriteBinary(IBinaryWriter writer)
    {
    }

    /// <summary>
    /// No implementation for local compute requests
    /// </summary>
    public void ReadBinary(IBinaryReader reader)
    {
    }
  }
}

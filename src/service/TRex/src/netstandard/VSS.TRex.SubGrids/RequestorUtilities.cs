using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.Caching;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids
{
  public class RequestorUtilities : IRequestorUtilities
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger< RequestorUtilities>();

    private static readonly bool _enableGeneralSubGridResultCaching = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_GENERAL_SUBGRID_RESULT_CACHING", Consts.ENABLE_GENERAL_SUBGRID_RESULT_CACHING);

    private static ITRexSpatialMemoryCache subGridCache;

    /// <summary>
    /// The DI injected TRex spatial memory cache for general sub grid results
    /// </summary>
    private static ITRexSpatialMemoryCache SubGridCache => subGridCache ?? (subGridCache = DIContext.Obtain<ITRexSpatialMemoryCache>());

    private static Func<ISubGridRequestor> SubGridRequestorFactory = DIContext.Obtain<Func<ISubGridRequestor>>();

    private static Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest> SurfaceElevationPatchRequestFactory = 
      DIContext.Obtain<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>();

    /// <summary>
    /// Constructs a set of requester intermediaries that have various aspects of surveyed surfaces, filters and caches pre-calculated
    /// ready to be used to create per-Task requestor delegates
    /// </summary>
    /// <returns></returns>
    public (ICombinedFilter Filter,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      Guid[] FilteredSurveyedSurfacesAsArray,
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ITRexSpatialMemoryCacheContext CacheContext)[] ConstructRequestorIntermediaries(ISiteModel siteModel,
        IFilterSet filters,
        bool includeSurveyedSurfaceInformation,
        GridDataType gridDataType)
    {
      (ICombinedFilter Filter,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      Guid[] FilteredSurveyedSurfacesAsArray,
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ITRexSpatialMemoryCacheContext CacheContext) getIntermediary(ICombinedFilter filter)
      {
        // Construct the appropriate list of surveyed surfaces
        // Obtain local reference to surveyed surface list. If it is replaced while processing the
        // list then the local reference will still be valid allowing lock free read access to the list.
        ISurveyedSurfaces FilteredSurveyedSurfaces = null;
        ISurveyedSurfaces SurveyedSurfaceList = siteModel.SurveyedSurfaces;

        if (includeSurveyedSurfaceInformation && SurveyedSurfaceList?.Count > 0)
        {
          FilteredSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

          // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
          SurveyedSurfaceList.FilterSurveyedSurfaceDetails(filter.AttributeFilter.HasTimeFilter,
            filter.AttributeFilter.StartTime, filter.AttributeFilter.EndTime,
            filter.AttributeFilter.ExcludeSurveyedSurfaces(), FilteredSurveyedSurfaces,
            filter.AttributeFilter.SurveyedSurfaceExclusionList);

          // Ensure that the filtered surveyed surfaces are in a known ordered state
          FilteredSurveyedSurfaces.SortChronologically(filter.AttributeFilter.ReturnEarliestFilteredCellPass);
        }

        Guid[] FilteredSurveyedSurfacesAsArray = FilteredSurveyedSurfaces?.Count > 0 ? FilteredSurveyedSurfaces.Select(s => s.ID).ToArray() : new Guid[0];

        // Get a caching context for the sub grids returned by this requester, but only if the requested grid data type supports it
        ITRexSpatialMemoryCacheContext SubGridCacheContext = null;

        if (_enableGeneralSubGridResultCaching &&
            ClientLeafSubGrid.SupportsAssignationFromCachedPreProcessedClientSubgrid[(int)gridDataType])
        {
          SubGridCacheContext = SubGridCache.LocateOrCreateContext(siteModel.ID, SpatialCacheFingerprint.ConstructFingerprint(siteModel.ID, gridDataType, filter, FilteredSurveyedSurfacesAsArray));
        }

        return (filter, FilteredSurveyedSurfaces, FilteredSurveyedSurfacesAsArray,
          SurfaceElevationPatchRequestFactory(SubGridCache, SubGridCache.LocateOrCreateContext(siteModel.ID, SpatialCacheFingerprint.ConstructFingerprint(siteModel.ID, GridDataType.HeightAndTime, filter, FilteredSurveyedSurfacesAsArray))),
            SubGridCacheContext);
      }

      // Construct the intermediary requestor state
      return filters.Filters.Select(getIntermediary).ToArray();
    }

    /// <summary>
    /// Constructs the set of requestors, one per filter, required to query the data stacks
    /// </summary>
    /// <returns></returns>
    public ISubGridRequestor[] ConstructRequestors(ISiteModel siteModel,
      (ICombinedFilter Filter,
        ISurveyedSurfaces FilteredSurveyedSurfaces,
        Guid[] FilteredSurveyedSurfacesAsArray,
        ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
        ITRexSpatialMemoryCacheContext CacheContext)[] Intermediaries,
      AreaControlSet areaControlSet,
      ISubGridTreeBitMask prodDataMask)
    {
      ISiteModels siteModels = DIContext.Obtain<ISiteModels>();

      // Construct the resulting requestors
      return Intermediaries.Select(x =>
      {
        // Instantiate a single instance of the argument object for the surface elevation patch requests and populate it with 
        // the common elements for this set of sub grids being requested. We always want to request all surface elevations to 
        // promote cacheability.

        var surfaceElevationPatchArg = new SurfaceElevationPatchArgument
        {
          SiteModelID = siteModel.ID,
          CellSize = siteModel.Grid.CellSize,
          IncludedSurveyedSurfaces = x.FilteredSurveyedSurfacesAsArray,
          SurveyedSurfacePatchType = x.Filter.AttributeFilter.ReturnEarliestFilteredCellPass ? SurveyedSurfacePatchType.EarliestSingleElevation : SurveyedSurfacePatchType.LatestSingleElevation,
          ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled)
        };

        var requestor = SubGridRequestorFactory();
        requestor.Initialize(siteModel,
          siteModels.StorageProxy,
          x.Filter,
          false, // Override cell restriction
          BoundingIntegerExtent2D.Inverted(),
          SubGridTreeConsts.SubGridTreeLevels,
          int.MaxValue, // MaxCellPasses
          areaControlSet,
          new FilteredValuePopulationControl(),
          prodDataMask,
          SubGridCache,
          x.CacheContext,
          x.FilteredSurveyedSurfaces,
          x.FilteredSurveyedSurfacesAsArray,
          x.surfaceElevationPatchRequest,
          surfaceElevationPatchArg);

        return requestor;
      }).ToArray();
    }
  }
}

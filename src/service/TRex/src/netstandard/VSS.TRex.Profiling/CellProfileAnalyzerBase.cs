using System;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Responsible for orchestrating analysis of identified cells along the path of a profile line
  /// and deriving the profile related analytics for each cell
  /// </summary>
  public abstract class CellProfileAnalyzerBase<T> : ICellProfileAnalyzer<T> where T : class, IProfileCellBase
  {
    //private static ILogger Log = Logging.Logger.CreateLogger<CellProfileAnalyzerBase<T>>();

    /// <summary>
    /// Local reference to the client subgrid factory
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static IClientLeafSubGridFactory clientLeafSubGridFactory;

    protected IClientLeafSubGridFactory ClientLeafSubGridFactory
      => clientLeafSubGridFactory ?? (clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>());

    protected static Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest> SurfaceElevationPatchRequestFactory =
      DIContext.Obtain<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>();

    /// <summary>
    /// The storage proxy to use when requesting subgrids for profiling operations
    /// </summary>
    private IStorageProxy storageProxy;

    protected IStorageProxy StorageProxy => storageProxy ?? (storageProxy = DIContext.Obtain<ISiteModels>().StorageProxy);

    /// <summary>
    /// The subgrid of composite elevations calculate from the collection of surveyed surfaces
    /// relevant to the profiling query
    /// </summary>
    protected ClientCompositeHeightsLeafSubgrid CompositeHeightsGrid;

    protected IClientLeafSubGrid CompositeHeightsGridIntf;

    /// <summary>
    /// The subgrid-by-subgrid filter mask used to control selection os surveyed surface
    /// and other cell data for each subgrid
    /// </summary>
    protected SubGridTreeBitmapSubGridBits FilterMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    protected ISiteModel SiteModel;
    protected ISubGridTreeBitMask PDExistenceMap;

    protected IFilterSet FilterSet;

    /// <summary>
    /// The set of surveyed surfaces that match the time constraints of the supplied filter.
    /// </summary>
    protected ISurveyedSurfaces FilteredSurveyedSurfaces;

    /// <summary>
    /// The argument to be used when requesting composite elevation sub grids to support profile analysis
    /// </summary>
    protected ISurfaceElevationPatchArgument SurfaceElevationPatchArg;

    protected ISurfaceElevationPatchRequest SurfaceElevationPatchRequest;

    /// <summary>
    /// The design supplied as a result of an independent lookup outside the scope of this builder
    /// to find the design identified by the cellPassFilter.ElevationRangeDesignUID
    /// </summary>
    protected IDesign CellPassFilter_ElevationRangeDesign;

    /// <summary>
    /// The design to be used as a TIN surface design based 'cookie cutter' selection mask for production data
    /// </summary>
    protected IDesign SurfaceDesignMaskDesign;

    /// <summary>
    /// The design to be used as an alignment design surface based 'cookie cutter' selection mask for production data
    /// </summary>
    protected IDesign AlignmentDesignMaskDesign;

    public CellProfileAnalyzerBase()
    { }

    /// <summary>
    /// Constructs a profile lift builder that analyzes cells in a cell profile vector
    /// </summary>
    /// <param name="siteModel"></param>
    /// <param name="pDExistenceMap"></param>
    /// <param name="filterSet"></param>
    /// <param name="cellPassFilter_ElevationRangeDesign"></param>
    public CellProfileAnalyzerBase(ISiteModel siteModel,
      ISubGridTreeBitMask pDExistenceMap,
      IFilterSet filterSet,
      IDesign cellPassFilter_ElevationRangeDesign)
    {
      SiteModel = siteModel;
      PDExistenceMap = pDExistenceMap;
      FilterSet = filterSet;
      CellPassFilter_ElevationRangeDesign = cellPassFilter_ElevationRangeDesign;

      Initialise();
    }

    public virtual void Initialise()
    {
      // Todo: Only first filter in filter set is currently used for surface & alignment mask designs or surveyed surface restriction driven by date range
      var PassFilter = FilterSet.Filters[0].AttributeFilter;
      var CellFilter = FilterSet.Filters[0].SpatialFilter;

      if (CellFilter.SurfaceDesignMaskDesignUid != Guid.Empty)
      {
        SurfaceDesignMaskDesign = SiteModel.Designs.Locate(CellFilter.SurfaceDesignMaskDesignUid);
        if (SurfaceDesignMaskDesign == null)
          throw new ArgumentException($"Design {CellFilter.SurfaceDesignMaskDesignUid} not found in project {SiteModel.ID}");
      }

      if (CellFilter.AlignmentDesignMaskDesignUID != Guid.Empty)
      {
        AlignmentDesignMaskDesign = SiteModel.Designs.Locate(CellFilter.AlignmentDesignMaskDesignUID);
        if (AlignmentDesignMaskDesign == null)
          throw new ArgumentException($"Design {CellFilter.AlignmentDesignMaskDesignUID} not found in project {SiteModel.ID}");
      }

      if (SiteModel.SurveyedSurfaces?.Count > 0)
      {
        // Filter out any surveyed surfaces which don't match current filter (if any)
        // - realistically, this is time filters we're thinking of here

        FilteredSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

        SiteModel.SurveyedSurfaces?.FilterSurveyedSurfaceDetails(PassFilter.HasTimeFilter, PassFilter.StartTime,
          PassFilter.EndTime, PassFilter.ExcludeSurveyedSurfaces(), FilteredSurveyedSurfaces,
          PassFilter.SurveyedSurfaceExclusionList);

        if (FilteredSurveyedSurfaces?.Count == 0)
          FilteredSurveyedSurfaces = null;
      }

      // Instantiate a single instance of the argument object for the surface elevation patch requests to obtain composite
      // elevation sub grids and populate it with the common elements for this set of sub grids being requested.
      SurfaceElevationPatchArg = new SurfaceElevationPatchArgument
      {
        SiteModelID = SiteModel.ID,
        CellSize = SiteModel.Grid.CellSize,
        IncludedSurveyedSurfaces = FilteredSurveyedSurfaces?.Select(x => x.ID).ToArray() ?? new Guid[0],
        SurveyedSurfacePatchType = SurveyedSurfacePatchType.CompositeElevations,
        ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled)
      };

      var _cache = DIContext.Obtain<ITRexSpatialMemoryCache>();
      var _context = _cache?.LocateOrCreateContext(SiteModel.ID, SurfaceElevationPatchArg.CacheFingerprint());

      SurfaceElevationPatchRequest = SurfaceElevationPatchRequestFactory(_cache, _context);
    }

    /// <summary>
    /// Performs the build action of processing the cells in the profile cell vector
    /// </summary>
    /// <param name="ProfileCells"></param>
    /// <param name="cellPassIterator"></param>
    /// <returns></returns>
    public abstract bool Analyze(List<T> ProfileCells, ISubGridSegmentCellPassIterator cellPassIterator);
  }
}

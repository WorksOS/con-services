using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Threading.Tasks;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids
{
  // ReSharper disable once IdentifierTypo
  public class SubGridRequestor : ISubGridRequestor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubGridRequestor>();

    /// <summary>
    /// Local reference to the client sub grid factory
    /// </summary>
    private IClientLeafSubGridFactory _clientLeafSubGridFactory;

    private IClientLeafSubGridFactory ClientLeafSubGridFactory => _clientLeafSubGridFactory ??= DIContext.Obtain<IClientLeafSubGridFactory>();

    private ISubGridRetriever _retriever;
    private ISiteModel _siteModel;
    private GridDataType _gridDataType;
    private ICombinedFilter _filter;
    private readonly ICellPassAttributeFilterProcessingAnnex _filterAnnex = new CellPassAttributeFilterProcessingAnnex();
    private ISurfaceElevationPatchRequest _surfaceElevationPatchRequest;
    private bool _hasOverrideSpatialCellRestriction;
    private BoundingIntegerExtent2D _overrideSpatialCellRestriction;
    private bool _prodDataRequested;
    private bool _surveyedSurfaceDataRequested;
    private IClientLeafSubGrid _clientGrid;
    private ISurfaceElevationPatchArgument _surfaceElevationPatchArg;

    public SubGridTreeBitmapSubGridBits CellOverrideMask { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
    private AreaControlSet _areaControlSet;

    // For height requests, the ProcessingMap is ultimately used to indicate which elevations were provided from a surveyed surface (if any)
    private SubGridTreeBitmapSubGridBits _processingMap;

    private ISurveyedSurfaces _filteredSurveyedSurfaces;

    private bool _returnEarliestFilteredCellPass;

    private ITRexSpatialMemoryCache _subGridCache;
    private ITRexSpatialMemoryCacheContext _subGridCacheContext;

    private IDesignWrapper _elevationRangeDesign;
    private IDesign _surfaceDesignMaskDesign;

    private IClientHeightLeafSubGrid _designElevations;
    private IClientHeightLeafSubGrid _surfaceDesignMaskElevations;

    // ReSharper disable once IdentifierTypo
    public SubGridRequestor() { }

    /// <summary>
    /// Constructor that accepts the common parameters around a set of sub grids the requester will be asked to process
    /// and initializes the requester state ready to start processing individual sub grid requests.
    /// </summary>
    public void Initialize(ISubGridsRequestArgument subGridsRequestArgument,
                           ISiteModel siteModel,
                           GridDataType gridDataType,
                           IStorageProxy storageProxy,
                           ICombinedFilter filter,
                           bool hasOverrideSpatialCellRestriction,
                           BoundingIntegerExtent2D overrideSpatialCellRestriction,
                           int maxNumberOfPassesToReturn,
                           AreaControlSet areaControlSet,
                           IFilteredValuePopulationControl populationControl,
                           ISubGridTreeBitMask pdExistenceMap,
                           ITRexSpatialMemoryCache subGridCache,
                           ITRexSpatialMemoryCacheContext subGridCacheContext,
                           ISurveyedSurfaces filteredSurveyedSurfaces,
                           ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
                           ISurfaceElevationPatchArgument surfaceElevationPatchArgument,
                           IOverrideParameters overrides,
                           ILiftParameters liftParams)
    {
      _siteModel = siteModel;
      _gridDataType = gridDataType;
      _filter = filter;

      _hasOverrideSpatialCellRestriction = hasOverrideSpatialCellRestriction;
      _overrideSpatialCellRestriction = overrideSpatialCellRestriction;

      _retriever = DIContext.Obtain<ISubGridRetrieverFactory>().Instance(subGridsRequestArgument,
                                      siteModel,
                                       gridDataType,
                                       storageProxy,
                                       filter,
                                       _filterAnnex,
                                       hasOverrideSpatialCellRestriction,
                                       overrideSpatialCellRestriction,
                                       maxNumberOfPassesToReturn,
                                       areaControlSet,
                                       populationControl,
                                       pdExistenceMap,
                                       subGridCacheContext,
                                       overrides,
                                       liftParams);

      _returnEarliestFilteredCellPass = _filter.AttributeFilter.ReturnEarliestFilteredCellPass;
      _areaControlSet = areaControlSet;
      _processingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      _surfaceElevationPatchArg = surfaceElevationPatchArgument;
      _surfaceElevationPatchRequest = surfaceElevationPatchRequest;

      _subGridCache = subGridCache;
      _subGridCacheContext = subGridCacheContext;

      _filteredSurveyedSurfaces = filteredSurveyedSurfaces;

      var elevRangeDesignFilter = _filter.AttributeFilter.ElevationRangeDesign;
      if (elevRangeDesignFilter.DesignID != Guid.Empty)
      {
        var design = _siteModel.Designs.Locate(elevRangeDesignFilter.DesignID);
        if (design == null)
          _log.LogError($"ElevationRangeDesign {elevRangeDesignFilter.DesignID} is unknown in project {siteModel.ID}");
        else
          _elevationRangeDesign = new DesignWrapper(elevRangeDesignFilter, design);
      }
      
      if (_filter.SpatialFilter.IsDesignMask)
        _surfaceDesignMaskDesign = _siteModel.Designs.Locate(_filter.SpatialFilter.SurfaceDesignMaskDesignUid);

      _filter.AttributeFilter.SiteModel = _siteModel;
    }

    /// <summary>
    /// InitialiseFilterContext performs any required filter initialization and configuration
    /// that is external to the filter prior to engaging in cell by cell processing of this sub grid
    /// </summary>
    private async Task<bool> InitialiseFilterContext()
    {
      if (_filter == null)
        return true;

      if (_filter.AttributeFilter.HasElevationRangeFilter)
      {
        _filterAnnex.ClearElevationRangeFilterInitialization();

        // If the elevation range filter uses a design then the design elevations
        // for the sub grid need to be calculated and supplied to the filter

        if (_elevationRangeDesign != null)
        {
          // Query the design to get the patch of elevations calculated from the design
          var getDesignHeightsResult = await _elevationRangeDesign.Design.GetDesignHeights(
            _siteModel.ID, _elevationRangeDesign.Offset, _clientGrid.OriginAsCellAddress(), _clientGrid.CellSize);
          _designElevations = getDesignHeightsResult.designHeights;

          if ((getDesignHeightsResult.errorCode != DesignProfilerRequestResult.OK && getDesignHeightsResult.errorCode != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              || _designElevations == null)
            return false;

          _filterAnnex.InitializeElevationRangeFilter(_filter.AttributeFilter, _designElevations);
        }
      }

      if (_filter.AttributeFilter.HasDesignFilter)
      {
        // SIGLogMessage.PublishNoODS(Nil, Format('#D# InitialiseFilterContext RequestDesignElevationPatch for Design %s',[CellFilter.DesignFilter.FileName]), ...);
        // Query the DesignProfiler service to get the patch of elevations calculated

        //Spatial design filter - don't care about offset
        var getDesignHeightsResult = await _surfaceDesignMaskDesign.GetDesignHeights(_siteModel.ID, 0, _clientGrid.OriginAsCellAddress(), _clientGrid.CellSize);
        _surfaceDesignMaskElevations = getDesignHeightsResult.designHeights;

        if ((getDesignHeightsResult.errorCode != DesignProfilerRequestResult.OK && getDesignHeightsResult.errorCode != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
             || _surfaceDesignMaskElevations == null)
        {
          _log.LogError($"#D# InitialiseFilterContext RequestDesignElevationPatch for Design {_surfaceDesignMaskDesign.DesignDescriptor.FileName} failed");
          return false;
        }
      }

      return true;
    }

    private void ModifyFilterMapBasedOnAdditionalSpatialFiltering()
    {
      // If we have DesignElevations at this point, then a Lift filter is in operation and
      // we need to use it to constrain the returned client grid to the extents of the design elevations
      if (_designElevations != null)
        _clientGrid.FilterMap.ForEachSetBit((x, y) => _clientGrid.FilterMap.SetBitValue(x, y, _designElevations.CellHasValue((byte)x, (byte)y)));

      if (_surfaceDesignMaskElevations != null)
        _clientGrid.FilterMap.ForEachSetBit((x, y) => _clientGrid.FilterMap.SetBitValue(x, y, _surfaceDesignMaskElevations.CellHasValue((byte)x, (byte)y)));
    }

    /// <summary>
    /// // Note: There is an assumption you have already checked on a existence map that there is a sub grid for this address
    /// </summary>
    /// <returns></returns>
    private ServerRequestResult PerformDataExtraction()
    {
      // Determine if there is a suitable pre-calculated result present in the general sub grid result cache.
      // If there is, then apply the filter mask to the cached data and copy it to the client grid
      var cachedSubGrid = (IClientLeafSubGrid)_subGridCacheContext?.Get(_clientGrid.CacheOriginX, _clientGrid.CacheOriginY);

      // If there was a cached sub grid located, assign its contents according the client grid mask into the client grid and return it
      if (cachedSubGrid != null)
      {
        // Log.LogInformation($"Acquired sub grid {CachedSubGrid.Moniker()} for client sub grid {ClientGrid.Moniker()} in data model {SiteModel.ID} from result cache");

        // Compute the matching filter mask that the full processing would have computed
        if (SubGridFilterMasks.ConstructSubGridCellFilterMask(_clientGrid, _siteModel, _filter, CellOverrideMask,
          _hasOverrideSpatialCellRestriction, _overrideSpatialCellRestriction, _clientGrid.ProdDataMap, _clientGrid.FilterMap))
        {
          ModifyFilterMapBasedOnAdditionalSpatialFiltering();

          // Use that mask to copy the relevant cells from the cache to the client sub grid
          _clientGrid.AssignFromCachedPreProcessedClientSubGrid(cachedSubGrid, _clientGrid.FilterMap);

          return ServerRequestResult.NoError;
        }

        return ServerRequestResult.FailedToComputeDesignFilterPatch;
      }

      var result = _retriever.RetrieveSubGrid(_clientGrid, CellOverrideMask);

      // If a sub grid was retrieved and this is a supported data type in the cache then add it to the cache
      if (result == ServerRequestResult.NoError && _subGridCacheContext != null)
      {
        // Don't add sub grids computed using a non-trivial WMS sieve to the general sub grid cache
        if (_areaControlSet.PixelXWorldSize == 0 && _areaControlSet.PixelYWorldSize == 0)
        {
          //Log.LogInformation($"Adding sub grid {ClientGrid.Moniker()} in data model {SiteModel.ID} to result cache");

          // Add the newly computed client sub grid to the cache by creating a clone of the client and adding it...
          var clientGrid2 = ClientLeafSubGridFactory.GetSubGrid(_clientGrid.GridDataType);
          clientGrid2.Assign(_clientGrid);
          clientGrid2.AssignFromCachedPreProcessedClientSubGrid(_clientGrid);

          if (!_subGridCache.Add(_subGridCacheContext, clientGrid2))
          {
            _log.LogWarning($"Failed to add sub grid {clientGrid2.Moniker()}, data model {_siteModel.ID} to sub grid result cache context [FingerPrint:{_subGridCacheContext.FingerPrint}], returning sub grid to factory as not added to cache");
            ClientLeafSubGridFactory.ReturnClientSubGrid(ref clientGrid2);
          }
        }
      }

      if (result == ServerRequestResult.NoError)
        ModifyFilterMapBasedOnAdditionalSpatialFiltering();

      //if <config>.Debug_ExtremeLogSwitchB then  SIGLogMessage.PublishNoODS(Nil, 'Completed call to RetrieveSubGrid()');

      return result;
    }

    private bool ApplyElevationRangeFilter(int x, int y, float z)
    {
      _filterAnnex.InitializeFilteringForCell(_filter.AttributeFilter, (byte) x, (byte) y);
      return _filterAnnex.FiltersElevation(z);
    }

    /// <summary>
    /// Annotates height information with elevations from surveyed surfaces
    /// </summary>
    private async Task<ServerRequestResult> PerformHeightAnnotation()
    {
      if ((_filteredSurveyedSurfaces?.Count ?? 0) == 0)
      {
        return ServerRequestResult.NoError;
      }

      var result = ServerRequestResult.NoError;

      // TODO: Add Debug_SwitchOffCompositeSurfaceGenerationFromSurveyedSurfaces to configuration
      // if <config>.Debug_SwitchOffCompositeSurfaceGenerationFromSurveyedSurfaces then Exit;

      ModifyFilterMapBasedOnAdditionalSpatialFiltering();

      if (!_clientGrid.UpdateProcessingMapForSurveyedSurfaces(_processingMap, _filteredSurveyedSurfaces as IList, _returnEarliestFilteredCellPass))
      {
        return ServerRequestResult.NoError;
      }

      if (_processingMap.IsEmpty())
      {
        return result;
      }

      try
      {
        // Hand client grid details, a mask of cells we need surveyed surface elevations for, and a temp grid to the Design Profiler
        _surfaceElevationPatchArg.SetOTGBottomLeftLocation(_clientGrid.OriginX, _clientGrid.OriginY);

        if (!(await _surfaceElevationPatchRequest.ExecuteAsync(_surfaceElevationPatchArg) is ClientHeightAndTimeLeafSubGrid surfaceElevations))
        {
          return result;
        }

        // Construct the elevation range filter lambda
        Func<int, int, float, bool> elevationRangeFilterLambda = null;
        if (_filter.AttributeFilter.HasElevationRangeFilter)
        {
          elevationRangeFilterLambda = ApplyElevationRangeFilter;
        }

        _clientGrid.PerformHeightAnnotation(_processingMap, _filteredSurveyedSurfaces as IList, _returnEarliestFilteredCellPass, surfaceElevations,  elevationRangeFilterLambda);

        result = ServerRequestResult.NoError;
      }
      finally
      {
        // TODO: Use client sub grid pool...
        //    PSNodeImplInstance.RequestProcessor.RepatriateClientGrid(TICSubGridTreeLeafSubGridBase(SurfaceElevations));
      }

      return result;
    }

    /// <summary>
    /// Responsible for coordinating the retrieval of production data for a sub grid from a site model and also annotating it with
    /// surveyed surface information for requests involving height data.
    /// </summary>
    public async Task<(ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)> RequestSubGridInternal(
      SubGridCellAddress subGridAddress,
      bool prodDataRequested,
      bool surveyedSurfaceDataRequested)
    {
      (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid) result = (ServerRequestResult.UnknownError, null);

      if (!(prodDataRequested || surveyedSurfaceDataRequested))
      {
        result.requestResult = ServerRequestResult.MissingInputParameters;
        return result;
      }

      _prodDataRequested = prodDataRequested;
      _surveyedSurfaceDataRequested = surveyedSurfaceDataRequested;

      // if <config>.Debug_ExtremeLogSwitchB then Log.LogDebug("About to call RetrieveSubGrid()");

      result.clientGrid = ClientLeafSubGridFactory.GetSubGridEx(
        Utilities.IntermediaryICGridDataTypeForDataType(_gridDataType, subGridAddress.SurveyedSurfaceDataRequested),
        _siteModel.CellSize, SubGridTreeConsts.SubGridTreeLevels,
        subGridAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask,
        subGridAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask);

      _clientGrid = result.clientGrid;

      if (ShouldInitialiseFilterContext() && !await InitialiseFilterContext())
      {
        result.requestResult = ServerRequestResult.FilterInitialisationFailure;
        return result;
      }

      if (_prodDataRequested)
      {
        if ((result.requestResult = PerformDataExtraction()) != ServerRequestResult.NoError)
          return result;
      }

      if (_surveyedSurfaceDataRequested)
      {
        // Construct the filter mask (e.g. spatial filtering) to be applied to the results of surveyed surface analysis
        result.requestResult = SubGridFilterMasks.ConstructSubGridCellFilterMask(_clientGrid, _siteModel, _filter, CellOverrideMask, _hasOverrideSpatialCellRestriction, _overrideSpatialCellRestriction, _clientGrid.ProdDataMap, _clientGrid.FilterMap)
          ? await PerformHeightAnnotation()
          : ServerRequestResult.FailedToComputeDesignFilterPatch;
      }

      return result;
    }

    /// <summary>
    /// Checks whether filter context should be initialized.
    /// </summary>
    private bool ShouldInitialiseFilterContext()
    {
      return _filter != null && (_filter.AttributeFilter.HasElevationRangeFilter || _filter.AttributeFilter.HasDesignFilter);
    }
  }
}

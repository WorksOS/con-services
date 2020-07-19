using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
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
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
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

    // For height requests, the ProcessingMap is ultimately used to indicate which elevations were provided from a surveyed surface (if any)
    private SubGridTreeBitmapSubGridBits _processingMap;

    private ISurveyedSurfaces _filteredSurveyedSurfaces;

    private bool _returnEarliestFilteredCellPass;

    private ITRexSpatialMemoryCache _subGridCache;
    private ITRexSpatialMemoryCacheContext[] _subGridCacheContexts;

    private IDesignWrapper _elevationRangeDesign;
    private IDesign _surfaceDesignMaskDesign;

    private float[,] _designElevations;
    private float[,] _surfaceDesignMaskElevations;

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
                           ITRexSpatialMemoryCacheContext[] subGridCacheContexts,
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
                                       subGridCacheContexts,
                                       overrides,
                                       liftParams);

      _returnEarliestFilteredCellPass = _filter.AttributeFilter.ReturnEarliestFilteredCellPass;
      _processingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      // Create a clone of the argument so that concurrently executing requestors don't trample over it
      _surfaceElevationPatchArg = new SurfaceElevationPatchArgument(
        surfaceElevationPatchArgument.SiteModelID,
        surfaceElevationPatchArgument.OTGCellBottomLeftX,
        surfaceElevationPatchArgument.OTGCellBottomLeftY,
        surfaceElevationPatchArgument.CellSize,
        surfaceElevationPatchArgument.SurveyedSurfacePatchType,
        null,
        null)
      {
        IncludedSurveyedSurfaces = surfaceElevationPatchArgument.IncludedSurveyedSurfaces
      };

      _surfaceElevationPatchRequest = surfaceElevationPatchRequest;

      _subGridCache = subGridCache;
      _subGridCacheContexts = subGridCacheContexts;

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

      // TODO: If the elevation range design and the surface design mask design are the same then only request the design elevations once.

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
          _designElevations = getDesignHeightsResult.designHeights.Cells;

          if ((getDesignHeightsResult.errorCode != DesignProfilerRequestResult.OK && getDesignHeightsResult.errorCode != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              || _designElevations == null)
            return false;

          _filterAnnex.InitializeElevationRangeFilter(_filter.AttributeFilter, _designElevations);
        }
      }

      if (_filter.SpatialFilter.HasSurfaceDesignMask())
      {
        // SIGLogMessage.PublishNoODS(Nil, Format('#D# InitialiseFilterContext RequestDesignElevationPatch for Design %s',[CellFilter.DesignFilter.FileName]), ...);
        // Query the DesignProfiler service to get the patch of elevations calculated

        //Spatial design filter - don't care about offset
        var getDesignHeightsResult = await _surfaceDesignMaskDesign.GetDesignHeights(_siteModel.ID, 0, _clientGrid.OriginAsCellAddress(), _clientGrid.CellSize);
        _surfaceDesignMaskElevations = getDesignHeightsResult.designHeights.Cells;

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
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (_designElevations != null)
        _clientGrid.FilterMap.ForEachSetBit((x, y) => _clientGrid.FilterMap.SetBitValue(x, y, _designElevations[x, y] != Consts.NullHeight));

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (_surfaceDesignMaskElevations != null)
        _clientGrid.FilterMap.ForEachSetBit((x, y) => _clientGrid.FilterMap.SetBitValue(x, y, _surfaceDesignMaskElevations[x, y] != Consts.NullHeight));
    }

    /// <summary>
    /// // Note: There is an assumption you have already checked on a existence map that there is a sub grid for this address
    /// </summary>
    private ServerRequestResult PerformDataExtraction()
    {
      // If there is a cache context for this sub grid, but the sub grid does not support assignation then complain
      var assignationSupported =  ClientLeafSubGrid.SupportsAssignationFromCachedPreProcessedClientSubGrid[(int)_clientGrid.GridDataType];

      var subGridCacheContext = _subGridCacheContexts?.FirstOrDefault(x => x.GridDataType == _clientGrid.GridDataType);

      if (subGridCacheContext != null && !assignationSupported)
      {
        throw new TRexException($"Client sub grid of type {_clientGrid.GridDataType} does not support assignation from cached sub grids but has a cache context enabled for it.");
      }

      if (subGridCacheContext != null && assignationSupported)
      {
        if (subGridCacheContext != null && _clientGrid.GridDataType != subGridCacheContext.GridDataType)
        {
          _log.LogWarning($"Client grid data type {_clientGrid.GridDataType} does not match type of sub grid cache context {subGridCacheContext.GridDataType}");
        }

        // Determine if there is a suitable pre-calculated result present in the general sub grid result cache.
        // If there is, then apply the filter mask to the cached data and copy it to the client grid
        var cachedSubGrid = (IClientLeafSubGrid)subGridCacheContext?.Get(_clientGrid.CacheOriginX, _clientGrid.CacheOriginY);

        // If there was a cached sub grid located, assign its contents according the client grid mask into the client grid and return it
        if (cachedSubGrid != null)
        {
          // Log.LogInformation($"Acquired sub grid {CachedSubGrid.Moniker()} for client sub grid {ClientGrid.Moniker()} in data model {SiteModel.ID} from result cache");

          // Check the cache supplied a tpe of sub grid we can use. If not (due to an issue), ignore the returned item and request the result directly
          if (_clientGrid.SupportsAssignationFrom(cachedSubGrid.GridDataType))
          {
            // Use the filter mask to copy the relevant cells from the cache to the client sub grid
            _clientGrid.AssignFromCachedPreProcessedClientSubGrid(cachedSubGrid, _clientGrid.FilterMap);

            return ServerRequestResult.NoError;
          }

          _log.LogError($"Sub grid retrieved from cache is not valid for assigning into client grid. Ignoring. Client sub grid = {_clientGrid.Moniker()}/{_clientGrid.GridDataType}. Cache sub grid = {cachedSubGrid.Moniker()}/{cachedSubGrid.GridDataType}");
        }
      }

      var result = _retriever.RetrieveSubGrid(_clientGrid, CellOverrideMask, out var sieveFilterInUse);

      // If a sub grid was retrieved and this is a supported data type in the cache then add it to the cache
      // If the sub grid does not support assignation from a precomputed sub grid then just return the result with 
      // no reference to the cache.
      if (result == ServerRequestResult.NoError && assignationSupported)
      {
        // Determine if this sub grid is suitable for storage in the cache
        // Don't add sub grids computed using a non-trivial WMS sieve to the general sub grid cache
        var shouldBeCached = subGridCacheContext != null && !sieveFilterInUse && (_clientGrid.GridDataType == subGridCacheContext.GridDataType);

        var clientGrid2 = ClientLeafSubGridFactory.GetSubGrid(_clientGrid.GridDataType);
        clientGrid2.Assign(_clientGrid);
        clientGrid2.AssignFromCachedPreProcessedClientSubGrid(_clientGrid, _clientGrid.FilterMap);

        if (shouldBeCached)
        {
          //Log.LogInformation($"Adding sub grid {ClientGrid.Moniker()} in data model {SiteModel.ID} to result cache");

          // Add the newly computed client sub grid to the cache by creating a clone of the client and adding it...
          if (!_subGridCache.Add(subGridCacheContext, _clientGrid))
          {
            _log.LogWarning($"Failed to add sub grid {clientGrid2.Moniker()}, data model {_siteModel.ID} to sub grid result cache context [FingerPrint:{subGridCacheContext.FingerPrint}], returning sub grid to factory as not added to cache");
            ClientLeafSubGridFactory.ReturnClientSubGrid(ref _clientGrid);
          }
        }

        _clientGrid = clientGrid2;
      }

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
        _surfaceElevationPatchArg.ProcessingMap.Assign(_processingMap);

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

        if (!_clientGrid.PerformHeightAnnotation(_processingMap, _filteredSurveyedSurfaces as IList, _returnEarliestFilteredCellPass, surfaceElevations,  elevationRangeFilterLambda))
          return ServerRequestResult.SubGridHeightAnnotationFailed;

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

      if (!SubGridFilterMasks.ConstructSubGridCellFilterMask(_clientGrid, _siteModel, _filter, CellOverrideMask,
        _hasOverrideSpatialCellRestriction, _overrideSpatialCellRestriction, _clientGrid.ProdDataMap, _clientGrid.FilterMap))
      {
        result.requestResult = ServerRequestResult.FailedToComputeDesignFilterPatch;
        ClientLeafSubGridFactory.ReturnClientSubGrid(ref result.clientGrid);
        return result;
      }

      ModifyFilterMapBasedOnAdditionalSpatialFiltering();

      if (_prodDataRequested)
      {
        if ((result.requestResult = PerformDataExtraction()) != ServerRequestResult.NoError)
          return result;
      }

      if (_surveyedSurfaceDataRequested)
      {
        // Construct the filter mask (e.g. spatial filtering) to be applied to the results of surveyed surface analysis. TODO: Is this another repeat of the cell filter map construction logic? FIX?
        result.requestResult = await PerformHeightAnnotation();
      }

      // Reassign _clientGrid to result as its reference may have been changed as a result of caching.
      result.clientGrid = _clientGrid;

      return result;
    }

    /// <summary>
    /// Checks whether filter context should be initialized.
    /// </summary>
    private bool ShouldInitialiseFilterContext()
    {
      return _filter != null && (_filter.AttributeFilter.HasElevationRangeFilter || _filter.SpatialFilter.HasSurfaceDesignMask());
    }
  }
}

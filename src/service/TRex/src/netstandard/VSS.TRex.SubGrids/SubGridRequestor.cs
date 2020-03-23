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
  public class SubGridRequestor : ISubGridRequestor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridRequestor>();

    /// <summary>
    /// Local reference to the client sub grid factory
    /// </summary>
    private IClientLeafSubGridFactory clientLeafSubGridFactory;

    private IClientLeafSubGridFactory ClientLeafSubGridFactory
       => clientLeafSubGridFactory ?? (clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>());

    private ISubGridRetriever retriever;
    private ISiteModel SiteModel;
    private GridDataType GridDataType;
    private ICombinedFilter Filter;
    private readonly ICellPassAttributeFilterProcessingAnnex FilterAnnex = new CellPassAttributeFilterProcessingAnnex();
    private ISurfaceElevationPatchRequest surfaceElevationPatchRequest;
    private bool HasOverrideSpatialCellRestriction;
    private BoundingIntegerExtent2D OverrideSpatialCellRestriction;
    private bool ProdDataRequested;
    private bool SurveyedSurfaceDataRequested;
    private IClientLeafSubGrid ClientGrid;
    private ISurfaceElevationPatchArgument SurfaceElevationPatchArg;

    public SubGridTreeBitmapSubGridBits CellOverrideMask { get; set; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled);
    private AreaControlSet AreaControlSet;

    // For height requests, the ProcessingMap is ultimately used to indicate which elevations were provided from a surveyed surface (if any)
    private SubGridTreeBitmapSubGridBits ProcessingMap;

    private ISurveyedSurfaces FilteredSurveyedSurfaces;

    private bool ReturnEarliestFilteredCellPass;

    private ITRexSpatialMemoryCache SubGridCache;
    private ITRexSpatialMemoryCacheContext SubGridCacheContext;

    private IDesignWrapper ElevationRangeDesign;
    private IDesign SurfaceDesignMaskDesign;

    private IClientHeightLeafSubGrid DesignElevations;
    private IClientHeightLeafSubGrid SurfaceDesignMaskElevations;

    public SubGridRequestor() { }

    /// <inheritdoc />
    /// <summary>
    /// Constructor that accepts the common parameters around a set of sub grids the requester will be asked to process
    /// and initializes the requester state ready to start processing individual sub grid requests.
    /// </summary>
    public void Initialize(ISiteModel siteModel,
                           GridDataType gridDataType,
                           IStorageProxy storageProxy,
                           ICombinedFilter filter,
                           bool hasOverrideSpatialCellRestriction,
                           BoundingIntegerExtent2D overrideSpatialCellRestriction,
                           int maxNumberOfPassesToReturn,
                           AreaControlSet areaControlSet,
                           IFilteredValuePopulationControl populationControl,
                           ISubGridTreeBitMask PDExistenceMap,
                           ITRexSpatialMemoryCache subGridCache,
                           ITRexSpatialMemoryCacheContext subGridCacheContext,
                           ISurveyedSurfaces filteredSurveyedSurfaces,
                           ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
                           ISurfaceElevationPatchArgument surfaceElevationPatchArgument,
                           IOverrideParameters overrides,
                           ILiftParameters liftParams)
    {
      SiteModel = siteModel;
      GridDataType = gridDataType;
      Filter = filter;

      HasOverrideSpatialCellRestriction = hasOverrideSpatialCellRestriction;
      OverrideSpatialCellRestriction = overrideSpatialCellRestriction;

      retriever = DIContext.Obtain<ISubGridRetrieverFactory>().Instance(siteModel,
                                       gridDataType,
                                       storageProxy,
                                       filter,
                                       FilterAnnex,
                                       hasOverrideSpatialCellRestriction,
                                       overrideSpatialCellRestriction,
                                       maxNumberOfPassesToReturn,
                                       areaControlSet,
                                       populationControl,
                                       PDExistenceMap,
                                       subGridCacheContext,
                                       overrides,
                                       liftParams);

      ReturnEarliestFilteredCellPass = Filter.AttributeFilter.ReturnEarliestFilteredCellPass;
      AreaControlSet = areaControlSet;
      ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      SurfaceElevationPatchArg = surfaceElevationPatchArgument;
      this.surfaceElevationPatchRequest = surfaceElevationPatchRequest;

      SubGridCache = subGridCache;
      SubGridCacheContext = subGridCacheContext;

      FilteredSurveyedSurfaces = filteredSurveyedSurfaces;

      var elevRangeDesignFilter = Filter.AttributeFilter.ElevationRangeDesign;
      if (elevRangeDesignFilter.DesignID != Guid.Empty)
      {
        var design = SiteModel.Designs.Locate(elevRangeDesignFilter.DesignID);
        if (design == null)
          Log.LogError($"ElevationRangeDesign {elevRangeDesignFilter.DesignID} is unknown in project {siteModel.ID}");
        else
          ElevationRangeDesign = new DesignWrapper(elevRangeDesignFilter, design);
      }
      
      if (Filter.SpatialFilter.IsDesignMask)
        SurfaceDesignMaskDesign = SiteModel.Designs.Locate(Filter.SpatialFilter.SurfaceDesignMaskDesignUid);

      Filter.AttributeFilter.SiteModel = SiteModel;
    }

    /// <summary>
    /// InitialiseFilterContext performs any required filter initialization and configuration
    /// that is external to the filter prior to engaging in cell by cell processing of this sub grid
    /// </summary>
    private async Task<bool> InitialiseFilterContext()
    {
      if (Filter == null)
        return true;

      if (Filter.AttributeFilter.HasElevationRangeFilter)
      {
        FilterAnnex.ClearElevationRangeFilterInitialization();

        // If the elevation range filter uses a design then the design elevations
        // for the sub grid need to be calculated and supplied to the filter

        if (ElevationRangeDesign != null)
        {
          // Query the design get the patch of elevations calculated
          var getDesignHeightsResult = await ElevationRangeDesign.Design.GetDesignHeights(
            SiteModel.ID, ElevationRangeDesign.Offset, ClientGrid.OriginAsCellAddress(), ClientGrid.CellSize);

          if ((getDesignHeightsResult.errorCode != DesignProfilerRequestResult.OK && getDesignHeightsResult.errorCode != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              || DesignElevations == null)
            return false;

          FilterAnnex.InitializeElevationRangeFilter(Filter.AttributeFilter, DesignElevations);
        }
      }

      if (Filter.AttributeFilter.HasDesignFilter)
      {
        // SIGLogMessage.PublishNoODS(Nil, Format('#D# InitialiseFilterContext RequestDesignElevationPatch for Design %s',[CellFilter.DesignFilter.FileName]), ...);
        // Query the DesignProfiler service to get the patch of elevations calculated

        //Spatial design filter - don't care about offset
        var getDesignHeightsResult = await SurfaceDesignMaskDesign.GetDesignHeights(SiteModel.ID, 0, ClientGrid.OriginAsCellAddress(), ClientGrid.CellSize);

        if ((getDesignHeightsResult.errorCode != DesignProfilerRequestResult.OK && getDesignHeightsResult.errorCode != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
             || SurfaceDesignMaskElevations == null)
        {
          Log.LogError($"#D# InitialiseFilterContext RequestDesignElevationPatch for Design {SurfaceDesignMaskDesign.DesignDescriptor.FileName} failed");
          return false;
        }
      }

      return true;
    }

    private void ModifyFilterMapBasedOnAdditionalSpatialFiltering()
    {
      // If we have DesignElevations at this point, then a Lift filter is in operation and
      // we need to use it to constrain the returned client grid to the extents of the design elevations
      if (DesignElevations != null)
        ClientGrid.FilterMap.ForEachSetBit((x, y) => ClientGrid.FilterMap.SetBitValue(x, y, DesignElevations.CellHasValue((byte)x, (byte)y)));

      if (SurfaceDesignMaskElevations != null)
        ClientGrid.FilterMap.ForEachSetBit((x, y) => ClientGrid.FilterMap.SetBitValue(x, y, SurfaceDesignMaskElevations.CellHasValue((byte)x, (byte)y)));
    }

    /// <summary>
    /// // Note: There is an assumption you have already checked on a existence map that there is a sub grid for this address
    /// </summary>
    /// <returns></returns>
    private ServerRequestResult PerformDataExtraction()
    {
      // Determine if there is a suitable pre-calculated result present in the general sub grid result cache.
      // If there is, then apply the filter mask to the cached data and copy it to the client grid
      var cachedSubGrid = (IClientLeafSubGrid)SubGridCacheContext?.Get(ClientGrid.CacheOriginX, ClientGrid.CacheOriginY);

      // If there was a cached sub grid located, assign its contents according the client grid mask into the client grid and return it
      if (cachedSubGrid != null)
      {
        // Log.LogInformation($"Acquired sub grid {CachedSubGrid.Moniker()} for client sub grid {ClientGrid.Moniker()} in data model {SiteModel.ID} from result cache");

        // Compute the matching filter mask that the full processing would have computed
        if (SubGridFilterMasks.ConstructSubGridCellFilterMask(ClientGrid, SiteModel, Filter, CellOverrideMask,
          HasOverrideSpatialCellRestriction, OverrideSpatialCellRestriction, ClientGrid.ProdDataMap, ClientGrid.FilterMap))
        {
          ModifyFilterMapBasedOnAdditionalSpatialFiltering();

          // Use that mask to copy the relevant cells from the cache to the client sub grid
          ClientGrid.AssignFromCachedPreProcessedClientSubgrid(cachedSubGrid, ClientGrid.FilterMap);

          return ServerRequestResult.NoError;
        }

        return ServerRequestResult.FailedToComputeDesignFilterPatch;
      }

      var result = retriever.RetrieveSubGrid(ClientGrid, CellOverrideMask);

      // If a sub grid was retrieved and this is a supported data type in the cache then add it to the cache
      if (result == ServerRequestResult.NoError && SubGridCacheContext != null)
      {
        // Don't add sub grids computed using a non-trivial WMS sieve to the general sub grid cache
        if (AreaControlSet.PixelXWorldSize == 0 && AreaControlSet.PixelYWorldSize == 0)
        {
          //Log.LogInformation($"Adding sub grid {ClientGrid.Moniker()} in data model {SiteModel.ID} to result cache");

          // Add the newly computed client sub grid to the cache by creating a clone of the client and adding it...
          var clientGrid2 = ClientLeafSubGridFactory.GetSubGrid(ClientGrid.GridDataType);
          clientGrid2.Assign(ClientGrid);
          clientGrid2.AssignFromCachedPreProcessedClientSubgrid(ClientGrid);

          if (!SubGridCache.Add(SubGridCacheContext, clientGrid2))
          {
            Log.LogWarning($"Failed to add sub grid {clientGrid2.Moniker()}, data model {SiteModel.ID} to sub grid result cache context [FingerPrint:{SubGridCacheContext.FingerPrint}], returning sub grid to factory as not added to cache");
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
      FilterAnnex.InitializeFilteringForCell(Filter.AttributeFilter, (byte) x, (byte) y);
      return FilterAnnex.FiltersElevation(z);
    }

    /// <summary>
    /// Annotates height information with elevations from surveyed surfaces
    /// </summary>
    private async Task<ServerRequestResult> PerformHeightAnnotation()
    {
      if ((FilteredSurveyedSurfaces?.Count ?? 0) == 0)
      {
        return ServerRequestResult.NoError;
      }

      var result = ServerRequestResult.NoError;

      // TODO: Add Debug_SwitchOffCompositeSurfaceGenerationFromSurveyedSurfaces to configuration
      // if <config>.Debug_SwitchOffCompositeSurfaceGenerationFromSurveyedSurfaces then Exit;

      ModifyFilterMapBasedOnAdditionalSpatialFiltering();

      if (!ClientGrid.UpdateProcessingMapForSurveyedSurfaces(ProcessingMap, FilteredSurveyedSurfaces as IList, ReturnEarliestFilteredCellPass))
      {
        return ServerRequestResult.NoError;
      }

      if (ProcessingMap.IsEmpty())
      {
        return result;
      }

      try
      {
        // Hand client grid details, a mask of cells we need surveyed surface elevations for, and a temp grid to the Design Profiler
        SurfaceElevationPatchArg.SetOTGBottomLeftLocation(ClientGrid.OriginX, ClientGrid.OriginY);

        if (!(await surfaceElevationPatchRequest.ExecuteAsync(SurfaceElevationPatchArg) is ClientHeightAndTimeLeafSubGrid surfaceElevations))
        {
          return result;
        }

        // Construct the elevation range filter lambda
        Func<int, int, float, bool> elevationRangeFilterLambda = null;
        if (Filter.AttributeFilter.HasElevationRangeFilter)
        {
          elevationRangeFilterLambda = ApplyElevationRangeFilter;
        }

        ClientGrid.PerformHeightAnnotation(ProcessingMap, FilteredSurveyedSurfaces as IList, ReturnEarliestFilteredCellPass, surfaceElevations,  elevationRangeFilterLambda);

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

      ProdDataRequested = prodDataRequested;
      SurveyedSurfaceDataRequested = surveyedSurfaceDataRequested;

      // if <config>.Debug_ExtremeLogSwitchB then Log.LogDebug("About to call RetrieveSubGrid()");

      result.clientGrid = ClientLeafSubGridFactory.GetSubGridEx(
        Utilities.IntermediaryICGridDataTypeForDataType(GridDataType, subGridAddress.SurveyedSurfaceDataRequested),
        SiteModel.CellSize, SubGridTreeConsts.SubGridTreeLevels,
        subGridAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask,
        subGridAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask);

      ClientGrid = result.clientGrid;

      if (ShouldInitialiseFilterContext() && !await InitialiseFilterContext())
      {
        result.requestResult = ServerRequestResult.FilterInitialisationFailure;
        return result;
      }

      if (ProdDataRequested)
      {
        if ((result.requestResult = PerformDataExtraction()) != ServerRequestResult.NoError)
          return result;
      }

      if (SurveyedSurfaceDataRequested)
      {
        // Construct the filter mask (e.g. spatial filtering) to be applied to the results of surveyed surface analysis
        result.requestResult = SubGridFilterMasks.ConstructSubGridCellFilterMask(ClientGrid, SiteModel, Filter, CellOverrideMask, HasOverrideSpatialCellRestriction, OverrideSpatialCellRestriction, ClientGrid.ProdDataMap, ClientGrid.FilterMap)
          ? await PerformHeightAnnotation()
          : ServerRequestResult.FailedToComputeDesignFilterPatch;
      }

      return result;
    }

    /// <summary>
    /// Checks whether filter context should be initialized.
    /// </summary>
    /// <returns></returns>
    private bool ShouldInitialiseFilterContext()
    {
      return (Filter != null) && (Filter.AttributeFilter.HasElevationRangeFilter || Filter.AttributeFilter.HasDesignFilter);
    }
  }
}

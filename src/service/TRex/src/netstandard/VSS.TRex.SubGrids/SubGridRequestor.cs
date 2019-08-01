using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
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

    private SubGridRetriever retriever;
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

    private IDesign ElevationRangeDesign;
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
                           ISurfaceElevationPatchArgument surfaceElevationPatchArgument)
    {
      SiteModel = siteModel;
      GridDataType = gridDataType;
      Filter = filter;

      HasOverrideSpatialCellRestriction = hasOverrideSpatialCellRestriction;
      OverrideSpatialCellRestriction = overrideSpatialCellRestriction;

      retriever = new SubGridRetriever(siteModel,
                                       gridDataType,
                                       storageProxy,
                                       Filter,
                                       FilterAnnex,
                                       hasOverrideSpatialCellRestriction,
                                       overrideSpatialCellRestriction,
                                       subGridCacheContext != null,
                                       maxNumberOfPassesToReturn,
                                       areaControlSet,
                                       populationControl,
                                       PDExistenceMap);

      ReturnEarliestFilteredCellPass = Filter.AttributeFilter.ReturnEarliestFilteredCellPass;
      AreaControlSet = areaControlSet;
      ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      SurfaceElevationPatchArg = surfaceElevationPatchArgument;
      this.surfaceElevationPatchRequest = surfaceElevationPatchRequest;

      SubGridCache = subGridCache;
      SubGridCacheContext = subGridCacheContext;

      FilteredSurveyedSurfaces = filteredSurveyedSurfaces;

      if (Filter.AttributeFilter.ElevationRangeDesign.DesignID != Guid.Empty)
        ElevationRangeDesign = SiteModel.Designs.Locate(Filter.AttributeFilter.ElevationRangeDesign.DesignID);

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

      (IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult profilerRequestResult) getDesignHeightsResult = (DesignElevations, DesignProfilerRequestResult.UnknownError);

      if (Filter.AttributeFilter.HasElevationRangeFilter)
      {
        FilterAnnex.ClearElevationRangeFilterInitialization();

        // If the elevation range filter uses a design then the design elevations
        // for the sub grid need to be calculated and supplied to the filter

        if (Filter.AttributeFilter.ElevationRangeDesign.DesignID != Guid.Empty)
        {
          // Query the design get the patch of elevations calculated
          getDesignHeightsResult = await ElevationRangeDesign.GetDesignHeights(SiteModel.ID, Filter.AttributeFilter.ElevationRangeDesign.Offset,
            ClientGrid.OriginAsCellAddress(), ClientGrid.CellSize);

          if ((getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.OK && getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
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
        getDesignHeightsResult = await SurfaceDesignMaskDesign.GetDesignHeights(SiteModel.ID, 0, ClientGrid.OriginAsCellAddress(), ClientGrid.CellSize);

        if ((getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.OK && getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
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
        ClientGrid.FilterMap.ForEachSetBit((X, Y) => ClientGrid.FilterMap.SetBitValue(X, Y, DesignElevations.CellHasValue((byte)X, (byte)Y)));

      if (SurfaceDesignMaskElevations != null)
        ClientGrid.FilterMap.ForEachSetBit((X, Y) => ClientGrid.FilterMap.SetBitValue(X, Y, SurfaceDesignMaskElevations.CellHasValue((byte)X, (byte)Y)));
    }

    /// <summary>
    /// // Note: There is an assumption you have already checked on a existence map that there is a sub grid for this address
    /// </summary>
    /// <returns></returns>
    private ServerRequestResult PerformDataExtraction(IOverrideParameters overrides)
    {
      // Determine if there is a suitable pre-calculated result present in the general sub grid result cache.
      // If there is, then apply the filter mask to the cached data and copy it to the client grid
      var CachedSubGrid = (IClientLeafSubGrid)SubGridCacheContext?.Get(ClientGrid.CacheOriginX, ClientGrid.CacheOriginY);
      //CachedSubGrid = null;
      // If there was a cached sub grid located, assign its contents according the client grid mask into the client grid and return it
      if (CachedSubGrid != null)
      {
        // Log.LogInformation($"Acquired sub grid {CachedSubGrid.Moniker()} for client sub grid {ClientGrid.Moniker()} in data model {SiteModel.ID} from result cache");

        // Compute the matching filter mask that the full processing would have computed
        if (SubGridFilterMasks.ConstructSubGridCellFilterMask(ClientGrid, SiteModel, Filter, CellOverrideMask,
          HasOverrideSpatialCellRestriction, OverrideSpatialCellRestriction, ClientGrid.ProdDataMap, ClientGrid.FilterMap))
        {
          ModifyFilterMapBasedOnAdditionalSpatialFiltering();

          // Use that mask to copy the relevant cells from the cache to the client sub grid
          ClientGrid.AssignFromCachedPreProcessedClientSubgrid(CachedSubGrid, ClientGrid.FilterMap);

          return ServerRequestResult.NoError;
        }

        return ServerRequestResult.FailedToComputeDesignFilterPatch;
      }

      ServerRequestResult Result = retriever.RetrieveSubGrid(/* LiftBuildSettings, */ overrides, ClientGrid, CellOverrideMask);

      // If a sub grid was retrieved and this is a supported data type in the cache then add it to the cache
      if (Result == ServerRequestResult.NoError && SubGridCacheContext != null)
      {
        // Don't add sub grids computed using a non-trivial WMS sieve to the general sub grid cache
        if (AreaControlSet.PixelXWorldSize == 0 && AreaControlSet.PixelYWorldSize == 0)
        {
          //Log.LogInformation($"Adding sub grid {ClientGrid.Moniker()} in data model {SiteModel.ID} to result cache");

          // Add the newly computed client sub grid to the cache by creating a clone of the client and adding it...
          IClientLeafSubGrid clientGrid2 = ClientLeafSubGridFactory.GetSubGrid(ClientGrid.GridDataType);
          clientGrid2.Assign(ClientGrid);
          clientGrid2.AssignFromCachedPreProcessedClientSubgrid(ClientGrid);

          if (!SubGridCache.Add(SubGridCacheContext, clientGrid2))
          {
            Log.LogWarning($"Failed to add sub grid {clientGrid2.Moniker()}, data model {SiteModel.ID} to sub grid result cache context [FingerPrint:{SubGridCacheContext.FingerPrint}], returning sub grid to factory as not added to cache");
            ClientLeafSubGridFactory.ReturnClientSubGrid(ref clientGrid2);
          }
        }
      }

      if (Result == ServerRequestResult.NoError)
        ModifyFilterMapBasedOnAdditionalSpatialFiltering();

      //if <config>.Debug_ExtremeLogSwitchB then  SIGLogMessage.PublishNoODS(Nil, 'Completed call to RetrieveSubGrid()');

      return Result;
    }

    /// <summary>
    /// Annotates height information with elevations from surveyed surfaces?
    /// </summary>
    private ServerRequestResult PerformHeightAnnotation()
    {
      if ((FilteredSurveyedSurfaces?.Count ?? 0) == 0)
        return ServerRequestResult.NoError;

      ClientHeightAndTimeLeafSubGrid ClientGridAsHeightAndTime = null;
      ClientHeightAndTimeLeafSubGrid SurfaceElevations;
      ServerRequestResult Result = ServerRequestResult.NoError;
      ClientCellProfileLeafSubgrid ClientGridAsCellProfile = null;

      // TODO: Add Debug_SwitchOffCompositeSurfaceGenerationFromSurveyedSurfaces to configuration
      // if <config>.Debug_SwitchOffCompositeSurfaceGenerationFromSurveyedSurfaces then Exit;

      ModifyFilterMapBasedOnAdditionalSpatialFiltering();

      bool ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime = ClientGrid is ClientHeightAndTimeLeafSubGrid;
      bool ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile = ClientGrid is ClientCellProfileLeafSubgrid;

      if (!(ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime || ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile))
        return ServerRequestResult.NoError;

      if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
      {
        ClientGridAsHeightAndTime = (ClientHeightAndTimeLeafSubGrid)ClientGrid;

        ProcessingMap.Assign(ClientGridAsHeightAndTime.FilterMap);

        // If we're interested in a particular cell, but we don't have any  surveyed surfaces later (or earlier)
        // than the cell production data pass time (depending on PassFilter.ReturnEarliestFilteredCellPass)
        // then there's no point in asking the Design Profiler service for an elevation
        long[,] Times = ClientGridAsHeightAndTime.Times;

        ProcessingMap.ForEachSetBit((x, y) =>
        {
          if (ClientGridAsHeightAndTime.Cells[x, y] != Consts.NullHeight &&
                      !(ReturnEarliestFilteredCellPass ? FilteredSurveyedSurfaces.HasSurfaceEarlierThan(Times[x, y]) : FilteredSurveyedSurfaces.HasSurfaceLaterThan(Times[x, y])))
            ProcessingMap.ClearBit(x, y);
        });
      }
      else // if (ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile)
      {
        ClientGridAsCellProfile = (ClientCellProfileLeafSubgrid)ClientGrid;
        ProcessingMap.Assign(ClientGridAsCellProfile.FilterMap);

        // If we're interested in a particular cell, but we don't have any
        // surveyed surfaces later (or earlier) than the cell production data
        // pass time (depending on PassFilter.ReturnEarliestFilteredCellPass)
        // then there's no point in asking the Design Profiler service for an elevation
        if (Result == ServerRequestResult.NoError)
        {
          ProcessingMap.ForEachSetBit((x, y) =>
          {
            if (ClientGridAsCellProfile.Cells[x, y].Height == Consts.NullHeight)
              return;

            if (Filter.AttributeFilter.ReturnEarliestFilteredCellPass)
            {
              if (!FilteredSurveyedSurfaces.HasSurfaceEarlierThan(ClientGridAsCellProfile.Cells[x, y].LastPassTime))
                ProcessingMap.ClearBit(x, y);
            }
            else
            {
              if (!FilteredSurveyedSurfaces.HasSurfaceLaterThan(ClientGridAsCellProfile.Cells[x, y].LastPassTime))
                ProcessingMap.ClearBit(x, y);
            }
          });
        }
      }

      // If we still have any cells to request surveyed surface elevations for...
      if (ProcessingMap.IsEmpty())
        return Result;

      try
      {
        // Hand client grid details, a mask of cells we need surveyed surface elevations for, and a temp grid to the Design Profiler
        SurfaceElevationPatchArg.SetOTGBottomLeftLocation(ClientGrid.OriginX, ClientGrid.OriginY);

        SurfaceElevations = surfaceElevationPatchRequest.Execute(SurfaceElevationPatchArg) as ClientHeightAndTimeLeafSubGrid;

        if (SurfaceElevations == null)
          return Result;

        // For all cells we wanted to request a surveyed surface elevation for,
        // update the cell elevation if a non null surveyed surface of appropriate time was computed
        // Note: The surveyed surface will return all cells in the requested sub grid, not just the ones indicated in the processing map
        // IE: It is unsafe to test for null top indicate not-filtered, use the processing map iterators to cover only those cells required
        ProcessingMap.ForEachSetBit((x, y) =>
        {
          float ProdHeight;
          long ProdTime;
          var SurveyedSurfaceCellHeight = SurfaceElevations.Cells[x, y];
          long SurveyedSurfaceCellTime = SurfaceElevations.Times[x, y];

                  // If we got back a surveyed surface elevation...
                  if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
          {
            ProdHeight = ClientGridAsHeightAndTime.Cells[x, y];
            ProdTime = ClientGridAsHeightAndTime.Times[x, y];
          }
          else // if (ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile)
                  {
            ProdHeight = ClientGridAsCellProfile.Cells[x, y].Height;
            ProdTime = ClientGridAsCellProfile.Cells[x, y].LastPassTime.Ticks;
          }

                  // Determine if the elevation from the surveyed surface data is required based on the production data elevation being null, and
                  // the relative age of the measured surveyed surface elevation compared with a non-null production data height
                  bool SurveyedSurfaceElevationWanted = SurveyedSurfaceCellHeight != Consts.NullHeight &&
                       (ProdHeight == Consts.NullHeight || ReturnEarliestFilteredCellPass ? SurveyedSurfaceCellTime < ProdTime : SurveyedSurfaceCellTime > ProdTime);

          if (!SurveyedSurfaceElevationWanted)
          {
                    // We didn't get a surveyed surface elevation, so clear the bit so that the renderer won't render it as a surveyed surface
                    ProcessingMap.ClearBit(x, y);
            return;
          }

                  // Check if there is an elevation range filter in effect and whether the surveyed surface elevation data matches it
                  if (Filter.AttributeFilter.HasElevationRangeFilter)
          {
            FilterAnnex.InitializeFilteringForCell(Filter.AttributeFilter, (byte)x, (byte)y);
            if (!FilterAnnex.FiltersElevation(SurveyedSurfaceCellHeight))
            {
                      // We didn't get a surveyed surface elevation, so clear the bit so that ASNode won't render it as a surveyed surface
                      ProcessingMap.ClearBit(x, y);
              return;
            }
          }

          if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
          {
            ClientGridAsHeightAndTime.Cells[x, y] = SurveyedSurfaceCellHeight;
            ClientGridAsHeightAndTime.Times[x, y] = SurveyedSurfaceCellTime;
          }
          else // if (ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile)
                    ClientGridAsCellProfile.Cells[x, y].Height = SurveyedSurfaceCellHeight;
        });

        if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
          ClientGridAsHeightAndTime.SurveyedSurfaceMap.Assign(ProcessingMap);

        Result = ServerRequestResult.NoError;
      }
      finally
      {
        // TODO: Use client sub grid pool...
        //    PSNodeImplInstance.RequestProcessor.RepatriateClientGrid(TICSubGridTreeLeafSubGridBase(SurfaceElevations));
      }

      return Result;
    }

    /// <summary>
    /// Responsible for coordinating the retrieval of production data for a sub grid from a site model and also annotating it with
    /// surveyed surface information for requests involving height data.
    /// </summary>
    /// <param name="subGridAddress"></param>
    /// <param name="overrides"></param>
    /// <param name="prodDataRequested"></param>
    /// <param name="surveyedSurfaceDataRequested"></param>
    /// <param name="clientGrid"></param>
    /// <returns></returns>
    public async Task<(ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)> RequestSubGridInternal(
      SubGridCellAddress subGridAddress,
      // LiftBuildSettings: TICLiftBuildSettings;
      IOverrideParameters overrides,
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
        if ((result.requestResult = PerformDataExtraction(overrides)) != ServerRequestResult.NoError)
          return result;
      }

      if (SurveyedSurfaceDataRequested)
      {
        // Construct the filter mask (e.g. spatial filtering) to be applied to the results of surveyed surface analysis
        result.requestResult = SubGridFilterMasks.ConstructSubGridCellFilterMask(ClientGrid, SiteModel, Filter, CellOverrideMask, HasOverrideSpatialCellRestriction, OverrideSpatialCellRestriction, ClientGrid.ProdDataMap, ClientGrid.FilterMap)
          ? PerformHeightAnnotation()
          : ServerRequestResult.FailedToComputeDesignFilterPatch;
      }

      return result;
    }

    /// <summary>
    /// Checks whether filter context should be initialised.
    /// </summary>
    /// <returns></returns>
    private bool ShouldInitialiseFilterContext()
    {
      return (Filter != null) && (Filter.AttributeFilter.HasElevationRangeFilter || Filter.AttributeFilter.HasDesignFilter);
    }
  }
}

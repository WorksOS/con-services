﻿using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Caching;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids
{
    public class SubGridRequestor
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Local reference to the client subgrid factory
        /// </summary>
        [NonSerialized]
        private IClientLeafSubgridFactory clientLeafSubGridFactory;

        private IClientLeafSubgridFactory ClientLeafSubGridFactory
           => clientLeafSubGridFactory ?? (clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubgridFactory>());

        private SubGridRetriever retriever;
        private ISiteModel SiteModel;
        private ICombinedFilter Filter;
        private SurfaceElevationPatchRequest surfaceElevationPatchRequest;
        private byte TreeLevel;
        private bool HasOverrideSpatialCellRestriction;
        private BoundingIntegerExtent2D OverrideSpatialCellRestriction;
        private int MaxNumberOfPassesToReturn;
        private bool ProdDataRequested;
        private bool SurveyedSurfaceDataRequested;
        private IClientLeafSubGrid ClientGrid;
        private SurfaceElevationPatchArgument SurfaceElevationPatchArg;
        private uint CellX;
        private uint CellY;
        public SubGridTreeBitmapSubGridBits CellOverrideMask = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);
        public AreaControlSet AreaControlSet;

        // For height requests, the ProcessingMap is ultimately used to indicate which elevations were provided from a surveyed surface (if any)
        private SubGridTreeBitmapSubGridBits ProcessingMap;

        private ISurveyedSurfaces FilteredSurveyedSurfaces;
        private bool ReturnEarliestFilteredCellPass;

        private ITRexSpatialMemoryCache SubGridCache;
        private ITRexSpatialMemoryCacheContext SubGridCacheContext;

        private IDesign ElevationRangeDesign;

        /// <summary>
        /// Constructor that accepts the common parameters around a set of subgrids the requester will be asked to process
        /// and initializes the requester state ready to start processing individual subgrid requests.
        /// </summary>
        public SubGridRequestor(ISiteModel sitemodel,
                                IStorageProxy storageProxy,
                                ICombinedFilter filter,
                                bool hasOverrideSpatialCellRestriction,
                                BoundingIntegerExtent2D overrideSpatialCellRestriction,
                                byte treeLevel,
                                int maxNumberOfPassesToReturn,
                                AreaControlSet areaControlSet,
                                IFilteredValuePopulationControl populationControl,
                                ISubGridTreeBitMask PDExistenceMap,
                                ITRexSpatialMemoryCache subGridCache,
                                ITRexSpatialMemoryCacheContext subGridCacheContext)
        {
            SiteModel = sitemodel;
            Filter = filter;
            TreeLevel = treeLevel;
            HasOverrideSpatialCellRestriction = hasOverrideSpatialCellRestriction;
            OverrideSpatialCellRestriction = overrideSpatialCellRestriction;
            MaxNumberOfPassesToReturn = maxNumberOfPassesToReturn;

            retriever = new SubGridRetriever(sitemodel,
                                             storageProxy,
                                             filter,
                                             hasOverrideSpatialCellRestriction,
                                             overrideSpatialCellRestriction,
                                             false, // todo Assigned(SubgridCache), //ClientGrid.SupportsAssignationFromCachedPreProcessedClientSubgrid
                                             treeLevel,
                                             maxNumberOfPassesToReturn,
                                             areaControlSet,
                                             populationControl,
                                             PDExistenceMap
                                             );

            ReturnEarliestFilteredCellPass = Filter.AttributeFilter.ReturnEarliestFilteredCellPass;

            surfaceElevationPatchRequest = new SurfaceElevationPatchRequest();

            AreaControlSet = areaControlSet;

            ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            // Construct the appropriate list of surveyed surfaces
            // Obtain local reference to surveyed surface list. If it is replaced while processing the
            // list then the local reference will still be valid allowing lock free read access to the list.
            ISurveyedSurfaces SurveyedSurfaceList = SiteModel.SurveyedSurfaces;
            FilteredSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

            if (SurveyedSurfaceList?.Count > 0)
            {
                // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
                SurveyedSurfaceList.FilterSurveyedSurfaceDetails(Filter.AttributeFilter.HasTimeFilter,
                                                                 Filter.AttributeFilter.StartTime, Filter.AttributeFilter.EndTime,
                                                                 Filter.AttributeFilter.ExcludeSurveyedSurfaces(), FilteredSurveyedSurfaces,
                                                                 Filter.AttributeFilter.SurveyedSurfaceExclusionList);

                // Ensure that the filtered surveyed surfaces are in a known ordered state
                FilteredSurveyedSurfaces.SortChronologically(ReturnEarliestFilteredCellPass);
            }

            // Instantiate a single instance of the argument object for the surface elevation patch requests and populate it with 
            // the common elements for this set of subgrids being requested. We always want to request all surface elevations to 
            // promote cacheability.
            SurfaceElevationPatchArg = new SurfaceElevationPatchArgument()
            {
                SiteModelID = SiteModel.ID,
                CellSize = SiteModel.Grid.CellSize,
                IncludedSurveyedSurfaces = FilteredSurveyedSurfaces,
                SurveyedSurfacePatchType = ReturnEarliestFilteredCellPass ? SurveyedSurfacePatchType.EarliestSingleElevation : SurveyedSurfacePatchType.LatestSingleElevation,
                ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled)
            };

          SubGridCache = subGridCache;
          SubGridCacheContext = subGridCacheContext;

          if (Filter.AttributeFilter.ElevationRangeDesignID != Guid.Empty)
            ElevationRangeDesign = SiteModel.Designs.Locate(Filter.AttributeFilter.ElevationRangeDesignID);
        }

        /// <summary>
        /// InitialiseFilterContext performs any required filter initialization and configuration
        /// that is external to the filter prior to engaging in cell by cell processing of
        /// this subgrid
        /// </summary>
        /// <returns></returns>
        private bool InitialiseFilterContext()
        {
            if (Filter == null)
                return true;

            if (Filter.AttributeFilter.HasElevationRangeFilter)
            {
                var ClonedFilter = new CombinedFilter(Filter.SpatialFilter);                
                ClonedFilter.AttributeFilter.Assign(Filter.AttributeFilter);
                ClonedFilter.AttributeFilter.ClearElevationRangeFilterInitialisation();

                // If the elevation range filter uses a design then the design elevations
                // for the subgrid need to be calculated and supplied to the filter

                if (Filter.AttributeFilter.ElevationRangeDesignID != Guid.Empty)
                {
                  // Query the design get the patch of elevations calculated
                  if (!ElevationRangeDesign.GetDesignHeights(SiteModel.ID, 
                        ClientGrid.OriginAsCellAddress(), ClientGrid.CellSize,
                        out IClientHeightLeafSubGrid DesignElevations, 
                        out DesignProfilerRequestResult ProfilerRequestResult))

                  if (ProfilerRequestResult != DesignProfilerRequestResult.OK || DesignElevations == null)
                    return false;

                  ClonedFilter.AttributeFilter.InitialiseElevationRangeFilter(DesignElevations);
                }
      }

            /*
    if CellFilter.HasDesignFilter then
      begin
//        SIGLogMessage.PublishNoODS(Nil, Format('#D# InitialiseFilterContext RequestDesignElevationPatch for Design %s',[CellFilter.DesignFilter.FileName]), slmcDebug);
        // Query the DesignProfiler service to get the patch of elevations calculated
        ErrorCode := PSNodeImplInstance.DesignProfilerService.RequestDesignElevationPatch
                     (Construct_CalculateDesignElevationPatch_Args(SiteModel.Grid.DataModelID,
                                                                   SubgridX, SubgridY,
                                                                   SiteModel.Grid.CellSize,
                                                                   CellFilter.DesignFilter,
                                                                   TSubGridTreeLeafBitmapSubGridBits.FullMask),
                     DesignFilterElevations);

        if (ErrorCode <> dppiOK) or not Assigned(DesignFilterElevations) then
          begin
            SIGLogMessage.PublishNoODS(Nil, Format('#D# InitialiseFilterContext RequestDesignElevationPatch for Design %s Failed',[CellFilter.DesignFilter.FileName]), slmcError);
            Result := False;
           Exit;
          end;
      end;
*/
            return true;
        }

        private ServerRequestResult PerformDataExtraction()
        {
            // note there is an assumption you have already checked on a existence map that there is a subgrid for this address

            // TICClientSubGridTreeLeaf_CellProfile ClientGridAsCellProfile = null
            // bool ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime;
            // bool ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile;
            IClientLeafSubGrid ClientGrid2;
            bool AddedSubgridToSubgridCache;

            ClientHeightLeafSubGrid DesignElevations = null;
            ServerRequestResult Result = ServerRequestResult.UnknownError;

            // Log.LogInformation("Entering RequestSubGridInternal");
           
            // Determine if there is a suitable pre-calculated result present
            // in the general subgrid result cache. If there is, then apply the
            // filter mask to the cached data and copy it to the client grid
            IClientLeafSubGrid CachedSubgrid = (IClientLeafSubGrid)SubGridCacheContext?.Get(ClientGrid.CacheOriginX, ClientGrid.CacheOriginY);

            // If there was a cached subgrid located, assign
            // it's contents according the client grid mask into the client grid and return it
            if (CachedSubgrid != null)
            {
              // Compute the matching filter mask that the full processing would have computed
              if (SubGridFilterMasks.ConstructSubgridCellFilterMask(ClientGrid, SiteModel, Filter,
                CellOverrideMask, HasOverrideSpatialCellRestriction, OverrideSpatialCellRestriction,
                ClientGrid.ProdDataMap, ClientGrid.FilterMap))
              {
                // If we have DesignElevations at this point, then a Lift filter is in operation and
                // we need to use it to constrain the returned client grid to the extents of the design elevations
                if (DesignElevations != null)
                {
                  IClientLeafSubGrid TempClientGrid = ClientGrid;
                  TempClientGrid.FilterMap.ForEachSetBit((X, Y) => TempClientGrid.FilterMap.SetBitValue(X, Y, DesignElevations.CellHasValue((byte)X, (byte)Y)));
                }

                // Use that mask to copy the relevant cells from the cache to the client subgrid
                ClientGrid.AssignFromCachedPreProcessedClientSubgrid(CachedSubgrid, ClientGrid.FilterMap);

                Result = ServerRequestResult.NoError;
              }
              else
                Result = ServerRequestResult.FailedToComputeDesignFilterPatch;
            }
            else
            {
                Result = retriever.RetrieveSubGrid(// DataStoreInstance.GridDataCache,
                                                   CellX, CellY,
                                                   // LiftBuildSettings,
                                                   ClientGrid,
                                                   CellOverrideMask,
                                                   DesignElevations);

                // If a subgrid was retrieved and this is a supported data type in the cache then add it to the cache
                if (Result == ServerRequestResult.NoError && SubGridCacheContext != null)
                {
                    // Don't add subgrids computed using a non-trivial WMS seive to the general subgrid cache
                    if (AreaControlSet.PixelXWorldSize == 0 && AreaControlSet.PixelYWorldSize == 0)
                    {
                        // Add the newly computed client subgrid to the cache
                        AddedSubgridToSubgridCache = SubGridCache.Add(SubGridCacheContext, ClientGrid);

                        try
                        {
                            if (!AddedSubgridToSubgridCache)
                            {
                                Log.LogWarning($"Failed to add subgrid {ClientGrid.Moniker()}, data model {SiteModel.ID} to subgrid result cache");
                            }

                            // Create a clone of the client grid that has the filter mask applied to
                            // returned the requested set of cell values back to the caller
                            ClientGrid2 = ClientLeafSubGridFactory.GetSubGrid(SubGridTrees.Client.Utilities.IntermediaryICGridDataTypeForDataType(ClientGrid.GridDataType, SurveyedSurfaceDataRequested));

                            // If we have DesignElevations at this point, then a Lift filter is in operation and
                            // we need to use it to constrain the returned client grid to the extents of the design elevations
                            if (DesignElevations != null)
                            {
                              IClientLeafSubGrid TempClientGrid = ClientGrid;
                              TempClientGrid.FilterMap.ForEachSetBit((X, Y) => TempClientGrid.FilterMap.SetBitValue(X, Y, DesignElevations.CellHasValue((byte)X, (byte)Y)));
                            }

                            ClientGrid2.Assign(ClientGrid);
                            ClientGrid2.AssignFromCachedPreProcessedClientSubgrid(ClientGrid, ClientGrid.FilterMap);
                        }
                        finally
                        {
                           // If not added to the cache, release it back to the pool
                           if (!AddedSubgridToSubgridCache)
                             ClientLeafSubGridFactory.ReturnClientSubGrid(ref ClientGrid);
                        }

                        ClientGrid = ClientGrid2;
                    }
                }
            }

            //if VLPDSvcLocations.Debug_ExtremeLogSwitchB then
            //  SIGLogMessage.PublishNoODS(Nil, 'Completed call to RetrieveSubGrid()', slmcDebug);

            return Result;
        }

        /// <summary>
        /// Annotates height information with elevations from surveyed surfaces?
        /// </summary>
        private ServerRequestResult PerformHeightAnnotation()
        {
            if (FilteredSurveyedSurfaces.Count == 0)
                return ServerRequestResult.NoError;

            ClientHeightAndTimeLeafSubGrid ClientGridAsHeightAndTime = null;
            ClientHeightAndTimeLeafSubGrid SurfaceElevations;
            bool SurveyedSurfaceElevationWanted;

            ServerRequestResult Result = ServerRequestResult.NoError;

            // if VLPDSvcLocations.Debug_SwitchOffCompositeSurfaceGenerationFromSurveyedSurfaces then
            // Exit;

            bool ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime = ClientGrid is ClientHeightAndTimeLeafSubGrid;

            //* TODO - subgrids containing cell profiles not yet supported
            // ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile = ClientGrid is ClientCellProfileLeafSubGrid; // TICClientSubGridTreeLeaf_CellProfile;

            if (!(ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime /* || ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile */))
                return ServerRequestResult.NoError;

            if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
            {
                ClientGridAsHeightAndTime = ClientGrid as ClientHeightAndTimeLeafSubGrid;

                ProcessingMap.Assign(ClientGridAsHeightAndTime.FilterMap);

                // If we're interested in a particular cell, but we don't have any
                // surveyed surfaces later (or earlier) than the cell production data
                // pass time (depending on PassFilter.ReturnEarliestFilteredCellPass)
                // then there's no point in asking the Design Profiler service for an elevation
                long[,] Times = ClientGridAsHeightAndTime.Times;

                ProcessingMap.ForEachSetBit((x, y) =>
                {
                    if (ClientGridAsHeightAndTime.Cells[x, y] != Consts.NullHeight &&
                        (!(ReturnEarliestFilteredCellPass ? FilteredSurveyedSurfaces.HasSurfaceEarlierThan(Times[x, y]) : FilteredSurveyedSurfaces.HasSurfaceLaterThan(Times[x, y]))))
                        ProcessingMap.ClearBit(x, y);
                });
            }
            /*
            else
            if (ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile)
            {
                ClientGridAsCellProfile = TICClientSubGridTreeLeaf_CellProfile(ClientGrid);
                ProcessingMap.Assign(ClientGridAsCellProfile .FilterMap);

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
                            if (!FilteredSurveyedSurfaces.HasSurfaceEarlierThan(ClientGridAsCellProfile.Cells[x, y].Time))
                                ProcessingMap.ClearBit(x, y);
                        }
                        else
                        {
                            if (!FilteredSurveyedSurfaces.HasSurfaceLaterThan(ClientGridAsCellProfile.Cells[x, y].Time))
                                ProcessingMap.ClearBit(x, y);
                        }
                    });
                }
            }
            */

            // If we still have any cells to request surveyed surface elevations for...
            if (ProcessingMap.IsEmpty())
                return Result;

            try
            {
                // Hand client grid details, a mask of cells we need surveyed surface elevations for, and a temp grid to the Design Profiler
                SurfaceElevationPatchArg.OTGCellBottomLeftX = ClientGrid.OriginX;
                SurfaceElevationPatchArg.OTGCellBottomLeftY = ClientGrid.OriginY;

                SurfaceElevations = surfaceElevationPatchRequest.Execute(SurfaceElevationPatchArg) as ClientHeightAndTimeLeafSubGrid;

                if (SurfaceElevations == null)
                    return Result;

                // For all cells we wanted to request a surveyed surface elevation for,
                // update the cell elevation if a non null surveyed surface of appropriate
                // time was computed
                float ProdHeight;
                long ProdTime;
                float SurveyedSurfaceCellHeight;
                long SurveyedSurfaceCellTime;

                // Note: The surveyed surface will return all cells in the requested subgrid, not just the ones indicated in the processing map
                // IE: It is unsafe to test for null top indicate not-filtered, use the processing map iterators to cover only those cells required
                ProcessingMap.ForEachSetBit((x, y) =>
                {
                    SurveyedSurfaceCellHeight = SurfaceElevations.Cells[x, y];
                    SurveyedSurfaceCellTime = SurfaceElevations.Times[x, y];

                    // If we got back a surveyed surface elevation...

                    if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
                    {
                        ProdHeight = ClientGridAsHeightAndTime.Cells[x, y];
                        ProdTime = ClientGridAsHeightAndTime.Times[x, y];
                    }
                    else
                    /*
                    if (ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile)
                    {
                        ProdHeight = ClientGridAsCellProfile.Cells[x, y].Height;
                        ProdTime = ClientGridAsCellProfile.Cells[x, y].LastPassTime;
                    }
                    else
                    */
                    {
                        ProdHeight = Consts.NullHeight; // should not get here
                        ProdTime = DateTime.MinValue.Ticks;
                    }

                    // Determine if the elevation from the surveyed surface data is required based on the nullness of the production data elevation, and
                    // the relative age of the measured surveyed surface elevation compared with a non-null production data height
                    SurveyedSurfaceElevationWanted = SurveyedSurfaceCellHeight != Consts.NullHeight &&
                                                     (ProdHeight == Consts.NullHeight ||
                                                      ReturnEarliestFilteredCellPass ? SurveyedSurfaceCellTime < ProdTime : SurveyedSurfaceCellTime > ProdTime);

                    if (SurveyedSurfaceElevationWanted)
                    {
                        // Check if there is an elevation range filter in effect and whether the
                        // surveyed surface elevation data matches it

                        bool ContinueProcessing = true;

                        if (Filter.AttributeFilter.HasElevationRangeFilter)
                        {
                            Filter.AttributeFilter.InitaliaseFilteringForCell((byte)x, (byte)y);

                            if (!Filter.AttributeFilter.FiltersElevation(SurveyedSurfaceCellHeight))
                            {
                                // We didn't get a surveyed surface elevation, so clear the bit so that ASNode won't render it as a surveyed surface
                                ProcessingMap.ClearBit(x, y);
                                ContinueProcessing = false;
                            }
                        }

                        if (ContinueProcessing)
                        {
                            if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
                            {
                                ClientGridAsHeightAndTime.Cells[x, y] = SurveyedSurfaceCellHeight;
                                ClientGridAsHeightAndTime.Times[x, y] = SurveyedSurfaceCellTime;
                            }
                            /*
                            else
                            {
                                if (ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile)
                                    ClientGridAsCellProfile.Cells[I, J] = SurveyedSurfaceCellHeight;
                            }
                            */
                        }
                    }
                    else
                    {
                        // We didn't get a surveyed surface elevation, so clear the bit so that the renderer won't render it as a surveyed surface
                        ProcessingMap.ClearBit(x, y);
                    }
                });

                if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
                    ClientGridAsHeightAndTime.SurveyedSurfaceMap.Assign(ProcessingMap);

                Result = ServerRequestResult.NoError;
            }
            finally
            {
                // TODO: Use client subgrid pool...
                //    PSNodeImplInstance.RequestProcessor.RepatriateClientGrid(TICSubGridTreeLeafSubGridBase(SurfaceElevations));
            }

            return Result;
        }

        /// <summary>
        /// Responsible for coordinating the retrieval of production data for a subgrid from a site model and also annotating it with
        /// surveyed surface information for requests involving height data.
        /// </summary>
        /// <param name="subGridAddress"></param>
        /// <param name="prodDataRequested"></param>
        /// <param name="surveyedSurfaceDataRequested"></param>
        /// <param name="clientGrid"></param>
        /// <returns></returns>
        public ServerRequestResult RequestSubGridInternal(SubGridCellAddress subGridAddress,
                                                          // LiftBuildSettings: TICLiftBuildSettings;
                                                          bool prodDataRequested,
                                                          bool surveyedSurfaceDataRequested,
                                                          IClientLeafSubGrid clientGrid
                                                          )
        {
            ProdDataRequested = prodDataRequested;
            SurveyedSurfaceDataRequested = surveyedSurfaceDataRequested;
            ClientGrid = clientGrid;

            if (!(ProdDataRequested || SurveyedSurfaceDataRequested))
                return ServerRequestResult.MissingInputParameters;

            ServerRequestResult Result = ServerRequestResult.UnknownError;

            // For now, it is safe to assume all subgrids containing on-the-ground cells have kSubGridTreeLevels levels
            CellX = subGridAddress.X << ((SubGridTreeConsts.SubGridTreeLevels - TreeLevel) * SubGridTreeConsts.SubGridIndexBitsPerLevel);
            CellY = subGridAddress.Y << ((SubGridTreeConsts.SubGridTreeLevels - TreeLevel) * SubGridTreeConsts.SubGridIndexBitsPerLevel);

            // if VLPDSvcLocations.Debug_ExtremeLogSwitchB then
            //    Log.LogDebug("About to call RetrieveSubGrid()");

            ClientGrid.SetAbsoluteOriginPosition((uint)(subGridAddress.X & ~SubGridTreeConsts.SubGridLocalKeyMask),
                                                 (uint)(subGridAddress.Y & ~SubGridTreeConsts.SubGridLocalKeyMask));
            ClientGrid.SetAbsoluteLevel(TreeLevel);
            ClientGrid.CellSize = SiteModel.Grid.CellSize;

            if (!InitialiseFilterContext())
              return ServerRequestResult.FilterInitialisationFailure;

            if (ProdDataRequested)
            {
                Result = PerformDataExtraction();

                if (Result != ServerRequestResult.NoError)
                    return Result;
            }

            if (SurveyedSurfaceDataRequested)
            {
                // Construct the filter mask (e.g. spatial filtering) to be applied to the results of surveyed surface analysis
                if (SubGridFilterMasks.ConstructSubgridCellFilterMask(ClientGrid, SiteModel, Filter,
                                                                      CellOverrideMask,
                                                                      HasOverrideSpatialCellRestriction,
                                                                      OverrideSpatialCellRestriction,
                                                                      ClientGrid.ProdDataMap,
                                                                      ClientGrid.FilterMap))
                    Result = PerformHeightAnnotation();
                else
                    Result = ServerRequestResult.FailedToComputeDesignFilterPatch;
            }

            return Result;
        }
    }
}

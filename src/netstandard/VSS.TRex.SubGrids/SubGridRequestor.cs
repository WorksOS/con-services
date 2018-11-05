using Microsoft.Extensions.Logging;
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
        private Guid[] FilteredSurveyedSurfacesAsArray;

        private bool ReturnEarliestFilteredCellPass;

        private ITRexSpatialMemoryCache SubGridCache;
        private ITRexSpatialMemoryCacheContext SubGridCacheContext;

        private IDesign ElevationRangeDesign;
        private IDesign SurfaceDesignMaskDesign;

        private IClientHeightLeafSubGrid DesignElevations;
        private IClientHeightLeafSubGrid SurfaceDesignMaskElevations;

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
                                ITRexSpatialMemoryCacheContext subGridCacheContext,                                
                                ISurveyedSurfaces filteredSurveyedSurfaces,
                                Guid[] filteredSurveyedSurfacesAsArray)
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

            AreaControlSet = areaControlSet;

            ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            // Instantiate a single instance of the argument object for the surface elevation patch requests and populate it with 
            // the common elements for this set of subgrids being requested. We always want to request all surface elevations to 
            // promote cacheability.
            SurfaceElevationPatchArg = new SurfaceElevationPatchArgument
            {
                SiteModelID = SiteModel.ID,
                CellSize = SiteModel.Grid.CellSize,
                IncludedSurveyedSurfaces = filteredSurveyedSurfacesAsArray,
                SurveyedSurfacePatchType = ReturnEarliestFilteredCellPass ? SurveyedSurfacePatchType.EarliestSingleElevation : SurveyedSurfacePatchType.LatestSingleElevation,
                ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled)
            };

            surfaceElevationPatchRequest = new SurfaceElevationPatchRequest(SurfaceElevationPatchArg.CacheFingerprint());
        
            SubGridCache = subGridCache;
            SubGridCacheContext = subGridCacheContext;
        
            FilteredSurveyedSurfaces = filteredSurveyedSurfaces;
            FilteredSurveyedSurfacesAsArray = filteredSurveyedSurfacesAsArray;
        
            if (Filter.AttributeFilter.ElevationRangeDesignID != Guid.Empty)
              ElevationRangeDesign = SiteModel.Designs.Locate(Filter.AttributeFilter.ElevationRangeDesignID);

            if (Filter.SpatialFilter.IsDesignMask)
              SurfaceDesignMaskDesign = SiteModel.Designs.Locate(Filter.SpatialFilter.SurfaceDesignMaskDesignUid);
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
                        out DesignElevations, 
                        out DesignProfilerRequestResult ProfilerRequestResult))

                  if (ProfilerRequestResult != DesignProfilerRequestResult.OK || DesignElevations == null)
                    return false;

                  ClonedFilter.AttributeFilter.InitialiseElevationRangeFilter(DesignElevations);
                }
            }

            if (Filter.AttributeFilter.HasDesignFilter)
            {
                // SIGLogMessage.PublishNoODS(Nil, Format('#D# InitialiseFilterContext RequestDesignElevationPatch for Design %s',[CellFilter.DesignFilter.FileName]), ...);
                // Query the DesignProfiler service to get the patch of elevations calculated
             
                if (!SurfaceDesignMaskDesign.GetDesignHeights(SiteModel.ID,
                  ClientGrid.OriginAsCellAddress(), ClientGrid.CellSize,
                  out SurfaceDesignMaskElevations,
                  out DesignProfilerRequestResult ProfilerRequestResult))
             
                if (ProfilerRequestResult != DesignProfilerRequestResult.OK || SurfaceDesignMaskElevations == null)
                {
                    Log.LogError($"#D# InitialiseFilterContext RequestDesignElevationPatch for Design {SurfaceDesignMaskDesign.Get_DesignDescriptor().FileName} failed");
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
            {
                ClientGrid.FilterMap.ForEachSetBit((X, Y) => ClientGrid.FilterMap.SetBitValue(X, Y, DesignElevations.CellHasValue((byte) X, (byte) Y)));
            }
        
            if (SurfaceDesignMaskElevations != null)
            {
                ClientGrid.FilterMap.ForEachSetBit((X, Y) => ClientGrid.FilterMap.SetBitValue(X, Y, SurfaceDesignMaskElevations.CellHasValue((byte) X, (byte) Y)));
            }
        }

        private ServerRequestResult PerformDataExtraction()
        {
            // note there is an assumption you have already checked on a existence map that there is a subgrid for this address

            // TICClientSubGridTreeLeaf_CellProfile ClientGridAsCellProfile = null
            // bool ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime;
            // bool ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile;

            ServerRequestResult Result; // = ServerRequestResult.UnknownError;

            // Log.LogInformation("Entering RequestSubGridInternal");
           
            // Determine if there is a suitable pre-calculated result present
            // in the general subgrid result cache. If there is, then apply the
            // filter mask to the cached data and copy it to the client grid
            var CachedSubgrid = (IClientLeafSubGrid)SubGridCacheContext?.Get(ClientGrid.CacheOriginX, ClientGrid.CacheOriginY);

            // If there was a cached subgrid located, assign
            // it's contents according the client grid mask into the client grid and return it
            if (CachedSubgrid != null)
            {
                Log.LogInformation($"Acquired subgrid {CachedSubgrid.Moniker()} for client subgrid {ClientGrid.Moniker()} in data model {SiteModel.ID} from result cache");
           
                // Compute the matching filter mask that the full processing would have computed
                if (SubGridFilterMasks.ConstructSubgridCellFilterMask(ClientGrid, SiteModel, Filter,
                    CellOverrideMask, HasOverrideSpatialCellRestriction, OverrideSpatialCellRestriction,
                    ClientGrid.ProdDataMap, ClientGrid.FilterMap))
                  {
                      ModifyFilterMapBasedOnAdditionalSpatialFiltering();
               
                      // Use that mask to copy the relevant cells from the cache to the client subgrid
                      ClientGrid.AssignFromCachedPreProcessedClientSubgrid(CachedSubgrid, ClientGrid.FilterMap);
               
                      Result = ServerRequestResult.NoError;
                  }
                  else
                      Result = ServerRequestResult.FailedToComputeDesignFilterPatch;
            }
            else
            {
                Result = retriever.RetrieveSubGrid(CellX, CellY,
                                                   // LiftBuildSettings,
                                                   ClientGrid,
                                                   CellOverrideMask,
                                                   DesignElevations);

                // If a subgrid was retrieved and this is a supported data type in the cache then add it to the cache
                if (Result == ServerRequestResult.NoError && SubGridCacheContext != null)
                {
                    // Don't add subgrids computed using a non-trivial WMS sieve to the general subgrid cache
                    if (AreaControlSet.PixelXWorldSize == 0 && AreaControlSet.PixelYWorldSize == 0)
                    {
                        // Log.LogInformation($"Adding subgrid {ClientGrid.Moniker()} in data model {SiteModel.ID} to result cache");

                        // Add the newly computed client subgrid to the cache by creating a clone of the client and adding it...
                        IClientLeafSubGrid ClientGrid2 = ClientLeafSubGridFactory.GetSubGrid(SubGridTrees.Client.Utilities.IntermediaryICGridDataTypeForDataType(ClientGrid.GridDataType, SurveyedSurfaceDataRequested));
                        ClientGrid2.Assign(ClientGrid);
                        ClientGrid2.AssignFromCachedPreProcessedClientSubgrid(ClientGrid);

                        if (!SubGridCache.Add(SubGridCacheContext, ClientGrid2))
                        {
                            Log.LogWarning($"Failed to add subgrid {ClientGrid2.Moniker()}, data model {SiteModel.ID} to subgrid result cache, returning subgrid to factory as not added to cache");
                            ClientLeafSubGridFactory.ReturnClientSubGrid(ref ClientGrid2);
                        }
                    }
                }

                if (Result == ServerRequestResult.NoError)
                    ModifyFilterMapBasedOnAdditionalSpatialFiltering();
            }

            //if <config>.Debug_ExtremeLogSwitchB then
            //  SIGLogMessage.PublishNoODS(Nil, 'Completed call to RetrieveSubGrid()');

            return Result;
        }

        /// <summary>
        /// Annotates height information with elevations from surveyed surfaces?
        /// </summary>
        private ServerRequestResult PerformHeightAnnotation()
        {
            if ((FilteredSurveyedSurfacesAsArray?.Length ?? 0) == 0)
                return ServerRequestResult.NoError;

            ClientHeightAndTimeLeafSubGrid ClientGridAsHeightAndTime = null;
            ClientHeightAndTimeLeafSubGrid SurfaceElevations;
            bool SurveyedSurfaceElevationWanted;

            ServerRequestResult Result = ServerRequestResult.NoError;

            // if <config>.Debug_SwitchOffCompositeSurfaceGenerationFromSurveyedSurfaces then
            // Exit;

            ModifyFilterMapBasedOnAdditionalSpatialFiltering();

            bool ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime = ClientGrid is ClientHeightAndTimeLeafSubGrid;

            //* TODO - subgrids containing cell profiles not yet supported
            // ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile = ClientGrid is ClientCellProfileLeafSubGrid; // TICClientSubGridTreeLeaf_CellProfile;

            if (!(ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime /* || ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile */))
                return ServerRequestResult.NoError;

            if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
            {
                ClientGridAsHeightAndTime = (ClientHeightAndTimeLeafSubGrid) ClientGrid;

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

                    // Determine if the elevation from the surveyed surface data is required based on the production data elevation beign null, and
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

            if (!(ProdDataRequested || SurveyedSurfaceDataRequested))
                return ServerRequestResult.MissingInputParameters;

            ServerRequestResult Result = ServerRequestResult.UnknownError;

            // For now, it is safe to assume all subgrids containing on-the-ground cells have kSubGridTreeLevels levels
            CellX = subGridAddress.X << ((SubGridTreeConsts.SubGridTreeLevels - TreeLevel) * SubGridTreeConsts.SubGridIndexBitsPerLevel);
            CellY = subGridAddress.Y << ((SubGridTreeConsts.SubGridTreeLevels - TreeLevel) * SubGridTreeConsts.SubGridIndexBitsPerLevel);

            // if <config>.Debug_ExtremeLogSwitchB then
            //    Log.LogDebug("About to call RetrieveSubGrid()");

            ClientGrid = clientGrid;
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
                {
                    Result = PerformHeightAnnotation();
                }
                else
                    Result = ServerRequestResult.FailedToComputeDesignFilterPatch;
            }

            return Result;
        }
    }
}

using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Services.Surfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Surfaces;
using VSS.VisionLink.Raptor.Surfaces.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Surfaces.GridFabric.Requests;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    public static class SubGridRequestor
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // InitialiseFilterContext performs any required filter initialisation and configuration
        // that is external to the filter prior to engaging in cell by cell processing of
        // this subgrid
        private static bool InitialiseFilterContext(CombinedFilter Filter)
        {
            if (Filter == null)
            {
                return true;
            }

            /* TODO Not yet supported
            ErrorCode: TDesignProfilerRequestResult;

            if (Filter.AttributeFilter.HasElevationRangeFilter)
            {
                ClonedFilter = TICGridDataPassFilter.Create(null);
                ClonedFilter.Assign(PassFilter);

                PassFilter = ClonedFilter;
                PassFilter.ClearElevationRangeFilterInitialisation;

                // If the elevation range filter uses a design then the design elevations
                // for the subgrid need to be calculated and supplied to the filter

                if (!Filter.AttributeFilter.ElevationRangeDesign.IsNull)
                {
                    // Query the DesignProfiler service to get the patch of elevations calculated
                    ErrorCode = PSNodeImplInstance.DesignProfilerService.RequestDesignElevationPatch
                                 (Construct_CalculateDesignElevationPatch_Args(SiteModel.Grid.DataModelID,
                                                                               SubgridX, SubgridY,
                                                                               SiteModel.Grid.CellSize,
                                                                               PassFilter.ElevationRangeDesign,
                                                                               TSubGridTreeLeafBitmapSubGridBits.FullMask),
                                 DesignElevations);

                    if (ErrorCode != dppiOK || DesignElevations == null)
                    {
                        return false;
                    }
                }

                PassFilter.InitialiseElevationRangeFilter(DesignElevations);
            }
            */
            return true;
        }

        public static ServerRequestResult RequestSubGridInternal(
                                              // SubgridCache : TDataModelContextSubgridResultCache;
                                              CombinedFilter Filter,
                                              int AMaxNumberOfPassesToReturn,
                                              bool AHasOverrideSpatialCellRestriction,
                                              BoundingIntegerExtent2D AOverrideSpatialCellRestriction,
                                              SiteModel SiteModel,
                                              SubGridCellAddress subGridAddress,
                                              byte Level,
                                              // LiftBuildSettings: TICLiftBuildSettings;
                                              // ASubgridLockToken : Integer;
                                              bool AProdDataRequested,
                                              bool ASurveyedSurfaceDataRequested,
                                              IClientLeafSubGrid ClientGrid,
                                              SubGridTreeBitmapSubGridBits CellOverrideMask,
                                              ref AreaControlSet AAreaControlSet)
        {
            uint CellX, CellY;

            // For height requests, the ProcessingMap is ultimately used to indicate which elevations were provided from a surveyed surface (if any)
            SubGridTreeBitmapSubGridBits ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridTreeBitmapSubGridBits.SubGridBitsCreationOptions.Unfilled);

            SurveyedSurfaces FilteredSurveyedSurfaces;
            ClientHeightAndTimeLeafSubGrid ClientGridAsHeightAndTime = null;
            ClientHeightAndTimeLeafSubGrid SurfaceElevations = null;
            SubGridCellHeightAndTime SurveyedSurfaceCell;
            bool SurveyedSurfaceElevationWanted;
            // TICClientSubGridTreeLeaf_CellProfile ClientGridAsCellProfile = null
            float ProdHeight;
            DateTime ProdTime;
            bool ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime;
            //bool ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile;
            // ISubGrid CachedSubgrid = null;
            // ISubGrid ClientGrid2;
            // bool AddedSubgridToSubgridCache;

            // ILeafSubGrid TempClientGrid;

            // CombinedFilter ClonedFilter = null;

            ClientHeightLeafSubGrid DesignElevations = null;

            // bool SubgridAlreadyPresentInCache = false;

            ServerRequestResult Result = ServerRequestResult.UnknownError;

//            Log.Info("Entering RequestSubGridInternal");

            if (AProdDataRequested || ASurveyedSurfaceDataRequested)
            {
                if (!InitialiseFilterContext(Filter))
                {
                    return ServerRequestResult.FilterInitialisationFailure;
                }
            }

            // For now, it is safe to assume all subgrids containing on-the-ground cells have kSubGridTreeLevels levels
            CellX = subGridAddress.X << ((SubGridTree.SubGridTreeLevels - Level) * SubGridTree.SubGridIndexBitsPerLevel);
            CellY = subGridAddress.Y << ((SubGridTree.SubGridTreeLevels - Level) * SubGridTree.SubGridIndexBitsPerLevel);

            //    if VLPDSvcLocations.Debug_ExtremeLogSwitchB then
            //      SIGLogMessage.PublishNoODS(Nil, 'About to call RetrieveSubGrid()', slmcDebug);

            ClientGrid.SetAbsoluteOriginPosition((uint)(subGridAddress.X & ~SubGridTree.SubGridLocalKeyMask),
                                                 (uint)(subGridAddress.Y & ~SubGridTree.SubGridLocalKeyMask));
            ClientGrid.SetAbsoluteLevel(Level);

            if (AProdDataRequested) // note there is an assumption you have already checked on a existenance map that there is a subgrid for this address
            {
                /* TODO - subgrid general result cache not supported yet
                // Determine if there is a suitable pre-calculated result present
                // in the general subgrid result cache. If there is, then apply the
                // filter mask to the cached data and copy it to the client grid
                if (SubgridCache != null)
                {
                    CachedSubgrid = SubgridCache.Lookup(ClientGrid.OriginAsCellAddress);
                }
                else
                {
                    CachedSubgrid = null;
                }

                // If there was a cached subgrid located, assign
                // it's contents according the client grid mask into the client grid and return it
                if (CachedSubgrid != null)
                {
                    try
                    {
                        // Compute the matching filter mask that the full processing would have computed
                        if ConstructSubgridCellFilterMask(ClientGrid, SiteModel, CellFilter,
                                                          CellOverrideMask, AHasOverrideSpatialCellRestriction, AOverrideSpatialCellRestriction,
                                                          ClientGrid.ProdDataMap, ClientGrid.FilterMap)
                              {
                            // If we have DesignElevations at this point, then a Lift filter is in operation and
                            // we need to use it to constrain the returned client grid to the extents of the design elevations
                            if (DesignElevations != null)
                            {
                                TempClientGrid = ClientGrid;
                                TempClientGrid.FilterMap.IterateOverSetBits(procedure(const X, Y: Integer) begin TempClientGrid.FilterMap.SetBitValue(X, Y, DesignElevations.CellHasValue(X, Y)); end);
                            }

                            // Use that mask to copy the relevant cells from the cache to the client subgrid
                            ClientGrid.AssignFromCachedPreProcessedClientSubgrid(CachedSubgrid, ClientGrid.FilterMap);

                            Result = ServerRequestResult.NoError;
                        }
                        else
                        {
                            Result = ServerRequestResult.FailedToComputeDesignFilterPatch;
                        }
                    }
                    finally
                    {
                        // The lookup of the cached subgrid performs a reference of the subgrid.
                        // The reference needs to be offset by a DeReference to indicate this
                        // interest in the subgrid is no longer required.
                        CachedSubgrid.DeReference;
                    }
                }
                */

                if (false)
                {
                    // TODO placeholder for cache implementation above
                }
                else
                {
                    SubGridRetriever retriever = new SubGridRetriever();
                    Result = retriever.RetrieveSubGrid(Filter,
                                                       AMaxNumberOfPassesToReturn,
                                                       AHasOverrideSpatialCellRestriction,
                                                       AOverrideSpatialCellRestriction,
                                                       SiteModel,
                                                       // DataStoreInstance.GridDataCache,
                                                       Level,
                                                       CellX, CellY,
                                                       // LiftBuildSettings,
                                                       ClientGrid,
                                                       false, // Assigned(SubgridCache), //ClientGrid.SupportsAssignationFromCachedPreProcessedClientSubgrid,
                                                       CellOverrideMask,
                                                       // ASubgridLockToken,
                                                       ref AAreaControlSet,
                                                       DesignElevations);

                    /* TODO: General subgrid result caching not yet supported
                    // If a subgrid was retrieved and this is a supported data type in the cache
                    // then add it to the cache
                    if (Result = ServerRequestResult.NoError && (SubgridCache != null))
                    {
                        // Don't add subgrids computed using a non-trivial WMS seive to the general subgrid cache
                        if (AAreaControlSet.PixelXWorldSize == 0 && AAreaControlSet.PixelYWorldSize == 0)
                        {
                            // Add the newly computed client subgrid to the cache
                            AddedSubgridToSubgridCache = SubgridCache.Add(ClientGrid, SubgridAlreadyPresentInCache);

                            try
                            {
                                if (!AddedSubgridToSubgridCache && !SubgridAlreadyPresentInCache)
                                {
                                    // TODO Add when logging available
                                    // SIGLogMessage.PublishNoODS(Nil, Format('Failed to add subgrid %s, data model %d to subgrid result cache', [ClientGrid.Moniker, SiteModel.ID]), slmcWarning);
                                }

                                // Create a clone of the client grid that has the filter mask applied to
                                // returned the requested set of cell values back to the caller
                                ClientGrid2 = PSNodeImplInstance.RequestProcessor.RequestClientGrid(ClientGrid.GridDataType,
                                                                                                    ClientGrid.CellSize,
                                                                                                    ClientGrid.IndexOriginOffset);

                                // If we have DesignElevations at this point, then a Lift filter is in operation and
                                // we need to use it to constrain the returned client grid to the extents of the design elevations
                                if (DesignElevations != null)
                                {
                                    TempClientGrid = ClientGrid;
                                    TempClientGrid.FilterMap.IterateOverSetBits((x, y) => { TempClientGrid.FilterMap.SetBitValue(X, Y, DesignElevations.CellHasValue(X, Y)); });
                                }

                                ClientGrid2.Assign(ClientGrid);
                                ClientGrid2.AssignFromCachedPreProcessedClientSubgrid(ClientGrid, ClientGrid.FilterMap);
                            }
                            finally
                            {
                                // Remove interest in the cached client grid if it was previously added to the cache
                                if (AddedSubgridToSubgridCache)
                                {
                                    ClientGrid.DeReference;
                                }
                                else // If not added to the cache, release it back to the pool
                                {
                                    PSNodeImplInstance.RequestProcessor.RepatriateClientGrid(ClientGrid);
                                }
                            }

                            ClientGrid = ClientGrid2;
                        }
                    }
                    */
                }
            }
            else
            {
                // Perform e.g. spatial filtering
                if (SubGridFilterMasks.ConstructSubgridCellFilterMask(ClientGrid, SiteModel, Filter,
                                                                      CellOverrideMask, 
                                                                      AHasOverrideSpatialCellRestriction, 
                                                                      AOverrideSpatialCellRestriction,
                                                                      ref (ClientGrid as ClientLeafSubGrid).ProdDataMap,
                                                                      ref (ClientGrid as ClientLeafSubGrid).FilterMap))
                {
                    Result = ServerRequestResult.NoError;
                }
                else
                {
                    Result = ServerRequestResult.FailedToComputeDesignFilterPatch;
                }
            }

            //if VLPDSvcLocations.Debug_ExtremeLogSwitchB then
            //  SIGLogMessage.PublishNoODS(Nil, 'Completed call to RetrieveSubGrid()', slmcDebug);

            // if VLPDSvcLocations.Debug_SwitchOffCompositeSurfaceGenerationFromSurveyedSurfaces then
            // Exit;

            return Result;

            if (!ASurveyedSurfaceDataRequested || Result != ServerRequestResult.NoError)
            {
                return Result;
            }

            ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime = ClientGrid is ClientHeightAndTimeLeafSubGrid;

            ///* TODO - cell profiles not yet supported
            // ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile = ClientGrid is ClientCellProfileLeafSubGrid; // TICClientSubGridTreeLeaf_CellProfile;

            // If we're requesting heights, we need to determine if any surveyed surfaces should be used to provide more up to date elevations
            if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime /* || ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile */)
            {
                FilteredSurveyedSurfaces = null;

                // Obtain local reference to surveyed surface list. If it is replaced while processing the
                // list then the local reference will still be valid allowing lock free read access to the list.
                SurveyedSurfaces SurveyedSurfaceList = SiteModel.SurveyedSurfaces;
                if (SurveyedSurfaceList.Count > 0)
                {
                    FilteredSurveyedSurfaces = new SurveyedSurfaces();

                    // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
                    SurveyedSurfaceList.FilterSurveyedSurfaceDetails(Filter.AttributeFilter.HasTimeFilter,
                         Filter.AttributeFilter.StartTime, Filter.AttributeFilter.EndTime,
                         Filter.AttributeFilter.ExcludeSurveyedSurfaces(), FilteredSurveyedSurfaces,
                         Filter.AttributeFilter.SurveyedSurfaceExclusionList);
                }

                if (FilteredSurveyedSurfaces != null && FilteredSurveyedSurfaces.Count > 0)
                {
                    // Ensure that the filtered ground surfaces are in a known ordered state
                    FilteredSurveyedSurfaces.SortChronologically();

                    if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
                    {
                        ClientGridAsHeightAndTime = ClientGrid as ClientHeightAndTimeLeafSubGrid;
                        ProcessingMap.Assign(ClientGridAsHeightAndTime.FilterMap);

                        // If we're interested in a particular cell, but we don't have any
                        // surveyed surfaces later (or earlier) than the cell production data
                        // pass time (depending on PassFilter.ReturnEarliestFilteredCellPass)
                        // then there's no point in asking the Design Profiler service for an elevation
                        if (Result == ServerRequestResult.NoError)
                        {
                            ProcessingMap.ForEachSetBit((I, J) =>
                            {
                                if (ClientGridAsHeightAndTime.Cells[I, J].Height == Consts.NullHeight)
                                {
                                    return;
                                }

                                if (Filter.AttributeFilter.ReturnEarliestFilteredCellPass)
                                {
                                    if (!FilteredSurveyedSurfaces.HasSurfaceEarlierThan(ClientGridAsHeightAndTime.Cells[I, J].Time))
                                    {
                                        ProcessingMap.ClearBit(I, J);
                                    }
                                }
                                else
                                {
                                    if (!FilteredSurveyedSurfaces.HasSurfaceLaterThan(ClientGridAsHeightAndTime.Cells[I, J].Time))
                                    {
                                        ProcessingMap.ClearBit(I, J);
                                    }
                                }
                            });
                        }
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
                            ProcessingMap.ForEachSetBit((I, J) =>
                            {
                                if (ClientGridAsCellProfile.Cells[I, J].Height == Consts.NullHeight)
                                {
                                    return;
                                }

                                if (Filter.AttributeFilter.ReturnEarliestFilteredCellPass)
                                {
                                    if (!FilteredSurveyedSurfaces.HasSurfaceEarlierThan(ClientGridAsCellProfile.Cells[I, J].Time))
                                    {
                                        ProcessingMap.ClearBit(I, J);
                                    }
                                }
                                else
                                {
                                    if (!FilteredSurveyedSurfaces.HasSurfaceLaterThan(ClientGridAsCellProfile.Cells[I, J].Time))
                                    {
                                        ProcessingMap.ClearBit(I, J);
                                    }
                                }
                            });
                        }
                    }
                    */ 

                    // If we still have any cells to request surveyed surface elevations for...
                    if (ProcessingMap.IsEmpty())
                    {
                        return Result;
                    }

                // Create the client grid instance to contain the result. Needed in the POC code??????
                    // SurfaceElevations = PSNodeImplInstance.RequestProcessor.RequestClientGrid(icdtHeightAndTime, ClientGrid.CellSize, ClientGrid.IndexOriginOffset) as ClientHeightAndTimeLeafSubGrid;

                    try
                    {
                        SurfaceElevationPatchRequest request = new SurfaceElevationPatchRequest();
                        SurfaceElevationPatchArgument arg = new SurfaceElevationPatchArgument
                        {
                            SiteModelID = SiteModel.ID,
                            CellSize = ClientGrid.CellSize,
                            OTGCellBottomLeftX = ClientGrid.OriginX,
                            OTGCellBottomLeftY = ClientGrid.OriginY,
                            EarliestSurface = Filter.AttributeFilter.ReturnEarliestFilteredCellPass,
                            ProcessingMap = ProcessingMap,
                            IncludedSurveyedSurfaces = FilteredSurveyedSurfaces
                        };

                        SurfaceElevations = request.Execute(arg);

                        if (SurfaceElevations == null)
                        {
                            return Result;
                        }
                        // Hand client grid details, a mask of cells we need surveyed surface elevations for, and a temp grid to the Design Profiler
/*
                          if (PSNodeImplInstance.DesignProfilerService.RequestSurfaceElevationPatch
                            (Construct_CalculateSurveyedSurfaceElevationPatch_Args(SiteModel.ID, 
                                                                                   ClientGrid.OriginX, ClientGrid.OriginY,
                                                                                   Filter.AttributeFilter.ReturnEarliestFilteredCellPass, 
                                                                                   ClientGrid.CellSize, ProcessingMap),
                             FilteredGroundSurfaces, SurfaceElevations) != dppiOK)
                        {
                            return Result;
                        }
*/

                        // For all cells we wanted to request a surveyed surface elevation for,
                        // update the cell elevation if a non null surveyed surface of appropriate
                        // time was computed
                        ProcessingMap.ForEachSetBit((I, J) =>
                        {
                            SurveyedSurfaceCell = SurfaceElevations.Cells[I, J];

                            // If we got back a surveyed surface elevation...ComputeFuncs

                            if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
                            {
                                ProdHeight = ClientGridAsHeightAndTime.Cells[I, J].Height;
                                ProdTime = ClientGridAsHeightAndTime.Cells[I, J].Time;
                            }
                            else
                            /*
                            if (ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile)
                            {
                                ProdHeight = ClientGridAsCellProfile.Cells[I, J].Height;
                                ProdTime = ClientGridAsCellProfile.Cells[I, J].LastPassTime;
                            }
                            else
                            */
                            {
                                ProdHeight = Consts.NullHeight; // should not get here
                                ProdTime = DateTime.MinValue;
                            }

                            if (Filter.AttributeFilter.ReturnEarliestFilteredCellPass)
                            {
                                SurveyedSurfaceElevationWanted = (SurveyedSurfaceCell.Height != Consts.NullHeight) &&
                                                                 ((ProdHeight == Consts.NullHeight) || (SurveyedSurfaceCell.Time < ProdTime));
                            }
                            else
                            {
                                SurveyedSurfaceElevationWanted = (SurveyedSurfaceCell.Height != Consts.NullHeight) &&
                                                                 ((ProdHeight == Consts.NullHeight) || (SurveyedSurfaceCell.Time > ProdTime));
                            }

                            if (SurveyedSurfaceElevationWanted)
                            {
                                // Check if there is an elevation range filter in effect and whether the
                                // surveyed surface elevation data matches it

                                bool ContinueProcessing = true;

                                if (Filter.AttributeFilter.HasElevationRangeFilter)
                                {
                                    Filter.AttributeFilter.InitaliaseFilteringForCell((byte)I, (byte)J);

                                    if (!Filter.AttributeFilter.FiltersElevation(SurveyedSurfaceCell.Height))
                                    {
                                        // We didn't get a surveyed surface elevation, so clear the bit so that ASNode won't render it as a surveyed surface
                                        ProcessingMap.ClearBit(I, J);
                                        ContinueProcessing = false;
                                    }
                                }

                                if (ContinueProcessing)
                                {
                                    if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
                                    {
                                        ClientGridAsHeightAndTime.Cells[I, J] = SurveyedSurfaceCell;
                                    }
                                    /*
                                    else
                                    {
                                        if (ClientGrid_is_TICClientSubGridTreeLeaf_CellProfile)
                                        {
                                            ClientGridAsCellProfile.Cells[I, J].Height = SurveyedSurfaceCell.Height;
                                        }
                                    }
                                    */
                                }
                            }
                            else
                            {
                                // We didn't get a surveyed surface elevation, so clear the bit so that the renderer won't render it as a surveyed surface
                                ProcessingMap.ClearBit(I, J);
                            }
                        });

                        if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
                        {
                            ClientGridAsHeightAndTime.SurveyedSurfaceMap.Assign(ProcessingMap);
                        }

                        Result = ServerRequestResult.NoError;
                    }
                    finally
                    {
                    // TODO: Use client subgrid pool...
                    //    PSNodeImplInstance.RequestProcessor.RepatriateClientGrid(TICSubGridTreeLeafSubGridBase(SurfaceElevations));
                    }
                }
            }

//            Log.Info("Exiting RequestSubGridInternal");

            return Result;
        }
    }
}

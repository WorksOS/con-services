using System;
using System.Diagnostics;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Pipelines.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.Pipelines
{
    /// <summary>
    /// RequestAnalyzer examines the set of parameters defining a request and determines the full set of subgrids
    /// the need to be requested, and the production data/surveyed surface aspects of those requests.
    /// Its implementation was modelled on the activities of the Legacy TSVOICSubGridSubmissionThread class.
    /// </summary>
    public class RequestAnalyser
    {
        /// <summary>
        /// The pipeline that has initiated this request analysis
        /// </summary>
        private ISubGridPipelineBase Owner;

        /// <summary>
        /// The resulting bitmap subgrid tree mask of all subgrids containing production data that need to be requested
        /// </summary>
        public SubGridTreeSubGridExistenceBitMask ProdDataMask;

        /// <summary>
        /// The resulting bitmap subgrid tree mask of all subgrids containing production data that need to be requested
        /// </summary>
        public SubGridTreeSubGridExistenceBitMask SurveydSurfaceOnlyMask;

        /// <summary>
        /// A cell coordinate level (rather than world coordinate) boundary that acts as an optional final override of the spatial area
        /// within which subgrids are being requested
        /// </summary>
        public BoundingIntegerExtent2D OverrideSpatialCellRestriction = BoundingIntegerExtent2D.Inverted();

        public BoundingWorldExtent3D WorldExtents = BoundingWorldExtent3D.Inverted();

        public long TotalNumberOfSubgridsAnalysed;
        public long TotalNumberOfCandidateSubgrids;
        protected bool ScanningFullWorldExtent;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public RequestAnalyser() 
        {
        }

        /// <summary>
        /// Constructor accepting the pipeline (analyser client) and the bounding world coordinate extents within which subgrids
        /// are being requested
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="worldExtents"></param>
        public RequestAnalyser(ISubGridPipelineBase owner,
            BoundingWorldExtent3D worldExtents) : this()
        {
            Owner = owner;
            ProdDataMask = new SubGridTreeSubGridExistenceBitMask();
            SurveydSurfaceOnlyMask = new SubGridTreeSubGridExistenceBitMask();
            WorldExtents = worldExtents;
        }

        /// <summary>
        /// Performs the donkey work of the request analysis
        /// </summary>
        protected void PerformScanning()
        {
            BoundingWorldExtent3D FilterRestriction = new BoundingWorldExtent3D();

            // Compute a filter spatial restriction on the world extents of the request
            if (WorldExtents.IsValidPlanExtent)
            {
                FilterRestriction.Assign(WorldExtents);
            }
            else
            {
                FilterRestriction.SetMaximalCoverage();
            }

            foreach (CombinedFilter filter in Owner.FilterSet.Filters)
            {
                if (filter != null)
                {
                    FilterRestriction = filter.SpatialFilter.CalculateIntersectionWithExtents(FilterRestriction);
                }
            }

            // TODO: Complete when design filter existance map is available
            /*
            //  for each filter and mask in filter to FOwner.OverallExistenceMap
            foreach (CombinedFilter filter in Owner.FilterSet.Filters)
            {
                if (filter != null && filter.AttributeFilter.HasDesignFilter && filter.DesignFilterSubgridOverlayMap)
                {
                    //    SIGLogMessage.PublishNoODS(Self, Format('#D# %s Has Design %s Anding with OverallExistMap',[Self.ClassName,FOwner.FilterSet.Filters[I].DesignFilter.FileName]), slmcDebug);
                    Owner.OverallExistenceMap.SetOp_AND(filter.DesignFilterSubgridOverlayMap);
                }
            }
            */

            ScanningFullWorldExtent = !WorldExtents.IsValidPlanExtent || WorldExtents.IsMaximalPlanConverage;

            if (ScanningFullWorldExtent)
            {
                Owner.OverallExistenceMap.ScanSubGrids(Owner.OverallExistenceMap.FullCellExtent(), SubGridEvent);
            }
            else
            {
                Owner.OverallExistenceMap.ScanSubGrids(FilterRestriction, SubGridEvent);
            }
        }

        /// <summary>
        /// The executor method for the analyser
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            try
            {
                PerformScanning();

                return true;  
            }
            catch //(Exception E)
            {
                // TODO Readd when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('Exception: ''%s''', [E.Message]), slmcException);
                return false;
            }
        }

        /// <summary>
        /// Performs scanning operations across subgrids, determining if they should be included in the request
        /// </summary>
        /// <param name="SubGrid"></param>
        /// <returns></returns>
        protected bool SubGridEvent(ISubGrid SubGrid)
        {
            // The given subgrid is a leaf sudgrid containing a bit mask recording subgrid inclusion in the overall subgrid map 
            // being iterated over. This map includes, production data only subgrids, surveyed surface data only subgrids and
            // subgrids that will have both types of data retrived for them. The analyser needs to seaprate out the two in terms
            // of the masks of subgrids that needs to be queried, one for production data (and optionally surveyed surface data) and 
            // one for surveyed surface data only. 
            // Get the matching subgrid from the production data only bit mask subgrid tree and use this subgrid to be able to sepearat 
            // the two sets of subgrids

            SubGridTreeLeafBitmapSubGrid ProdDataSubGrid = Owner.ProdDataExistenceMap.LocateSubGridContaining(SubGrid.OriginX, SubGrid.OriginY) as SubGridTreeLeafBitmapSubGrid;

            byte ScanMinXb, ScanMinYb, ScanMaxXb, ScanMaxYb;
            bool Result = true; // Set to false if the scanning process needs to be aborted.
            double OTGCellSize = SubGrid.Owner.CellSize / SubGridTree.SubGridTreeDimension;
            SubGridTreeLeafBitmapSubGrid CastSubGrid = SubGrid as SubGridTreeLeafBitmapSubGrid;

            if (ScanningFullWorldExtent)
            {
                ScanMinXb = 0;
                ScanMinYb = 0;
                ScanMaxXb = SubGridTree.SubGridTreeDimensionMinus1;
                ScanMaxYb = SubGridTree.SubGridTreeDimensionMinus1;
            }
            else
            {
                // Calculate the range of cells in this subgrid we need to scan and request. The steps below
                // calculate the on-the-ground cell indices of the world coordinate bounding extents, then
                // determine the subgrid indices of the cell within this subgrid that contains those
                // cells, then determines the subgrid extents in this subgrid to scan over
                // Remember, each on-the-ground element (bit) in the existance map represents an
                // entire on-the-ground subgrid (32x32 OTG cells) in the matching sub grid tree.

                // Expand the number of cells scanned to create the rendered tile by a single cell width
                // on all sides to ensure the boundaries of tiles are rendered right to the edge of the tile.

                SubGrid.Owner.CalculateIndexOfCellContainingPosition(WorldExtents.MinX - OTGCellSize,
                                                                     WorldExtents.MinY - OTGCellSize,
                                                                     out uint ScanMinX, out uint ScanMinY);
                SubGrid.Owner.CalculateIndexOfCellContainingPosition(WorldExtents.MaxX + OTGCellSize,
                                                                     WorldExtents.MaxY + OTGCellSize,
                                                                     out uint ScanMaxX, out uint ScanMaxY);

                ScanMinX = Math.Max(CastSubGrid.OriginX, ScanMinX);
                ScanMinY = Math.Max(CastSubGrid.OriginY, ScanMinY);
                ScanMaxX = Math.Min(ScanMaxX, CastSubGrid.OriginX + SubGridTree.SubGridTreeDimensionMinus1);
                ScanMaxY = Math.Min(ScanMaxY, CastSubGrid.OriginY + SubGridTree.SubGridTreeDimensionMinus1);

                SubGrid.GetSubGridCellIndex(ScanMinX, ScanMinY, out ScanMinXb, out ScanMinYb);
                SubGrid.GetSubGridCellIndex(ScanMaxX, ScanMaxY, out ScanMaxXb, out ScanMaxYb);
            }

            // Iterate over the subrange of cells (bits) in this subgrid and request the matching subgrids

            for (byte I = ScanMinXb; I <= ScanMaxXb; I++)
            {
                for (byte J = ScanMinYb; J <= ScanMaxYb; J++)
                {
                    if (CastSubGrid.Bits.BitSet(I, J))
                    {
                        TotalNumberOfCandidateSubgrids++;

                        // If there is a design subgrid overlay map supplied to the renderer then
                        // check to see if this subgrid is in the map, and if so then continue. If it is
                        // not in the map then it does not need to be considered. Design subgrid overlay
                        // indices contain a single bit for each on the ground subgrid (32x32 cells),
                        // which means they are only 5 levels deep. This means the (OriginX + I, OriginY + J)
                        // origin coordinates correctly identify the single bits that denote the subgrids.

                        if (Owner.DesignSubgridOverlayMap != null)
                        {
                            if (!Owner.DesignSubgridOverlayMap.GetCell(SubGrid.OriginX + I, SubGrid.OriginY + J))
                            {
                                continue;
                            }
                        }

                        Debug.Assert(Owner.ProdDataExistenceMap != null, "Production Data Existance Map should have been specified");

                        // If there is a spatial filter in play then determine if the subgrid about to be requested intersects the spatial filter extent

                        bool SubgridSatisfiesFilter = true;
                        foreach (CombinedFilter filter in Owner.FilterSet.Filters)
                        {
                            if (filter == null)
                            {
                                continue;
                            }

                            CellSpatialFilter spatialFilter = filter.SpatialFilter;

                            if (spatialFilter.IsSpatial && spatialFilter.Fence != null && spatialFilter.Fence.NumVertices > 0)
                            {
                                SubgridSatisfiesFilter = spatialFilter.Fence.IntersectsExtent(CastSubGrid.Owner.GetCellExtents(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J));
                            }
                            else
                            {
                                if (spatialFilter.IsPositional)
                                {
                                    CastSubGrid.Owner.GetCellCenterPosition(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J, out double centerX, out double centerY);

                                    SubgridSatisfiesFilter = Math.Sqrt(Math.Pow(spatialFilter.PositionX - centerX, 2) + Math.Pow(spatialFilter.PositionY - centerY, 2)) < (spatialFilter.PositionRadius + (Math.Sqrt(2) * CastSubGrid.Owner.CellSize) / 2);
                                }
                            }

                            if (!SubgridSatisfiesFilter)
                            {
                                break;
                            }
                        }

                        if (SubgridSatisfiesFilter)
                        {
                            TotalNumberOfSubgridsAnalysed++;

                            // Add the leaf subgrid identitied by the address below, along with the production data and surveyed surface
                            // flags to the subgrid tree being used to aggregate all the subgrids that need to be queried for the request
                            // SubGridCellAddress NewSubGridAddress =
                            //      new SubGridCellAddress((CastSubGrid.OriginX + I) << SubGridTree.SubGridIndexBitsPerLevel,
                            //                             (CastSubGrid.OriginY + J) << SubGridTree.SubGridIndexBitsPerLevel,
                            //                             Owner.ProdDataExistenceMap.GetCell(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J),
                            //                             Owner.IncludeSurveyedSurfaceInformation);

                            // Set the ProdDataMask for the production data
                            if (ProdDataSubGrid != null && ProdDataSubGrid.Bits.BitSet(I, J))
                            {
                                ProdDataMask.SetCell(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J, true);
                            }
                            else
                            {
                                // Note: This is ONLY recording the subgrids that have surveyed surface data required, but not production data 
                                // as a delta to the production data requests
                                SurveydSurfaceOnlyMask.SetCell(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J, true);
                            }
                        }
                    }
                }
            }

            return Result;
        }
    }
} 
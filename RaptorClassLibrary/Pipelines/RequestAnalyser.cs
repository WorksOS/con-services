using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
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
        private SubGridPipelineBase Owner = null;

        public SubGridTreeBitMask Mask = null;

        public BoundingIntegerExtent2D OverrideSpatialCellRestriction = BoundingIntegerExtent2D.Inverted();

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

            for (int I = 0; I < Owner.FilterSet.Filters.Length; I++)
            {
                if (Owner.FilterSet.Filters[I] != null)
                {
                    FilterRestriction = Owner.FilterSet.Filters[I].SpatialFilter.CalculateIntersectionWithExtents(FilterRestriction);
                }
            }

            ScanningFullWorldExtent = !WorldExtents.IsValidPlanExtent || WorldExtents.IsMaximalPlanConverage;

            if (ScanningFullWorldExtent)
            {
                Owner.OverallExistenceMap.ScanSubGrids(Owner.OverallExistenceMap.FullCellExtent(), SubGridEvent);
            }
            else
            {
                Owner.OverallExistenceMap.ScanSubGrids(FilterRestriction, SubGridEvent);
            };
        }

        public BoundingWorldExtent3D WorldExtents = BoundingWorldExtent3D.Inverted();

        public long TotalNumberOfSubgridsAnalysed = 0;
        public long TotalNumberOfCandidateSubgrids = 0;
        protected bool ScanningFullWorldExtent = false;

        public RequestAnalyser() : base()
        {
        }

        public RequestAnalyser(SubGridPipelineBase owner, BoundingWorldExtent3D worldExtents) : this()
        {
            Owner = owner;
            Mask = new SubGridTreeBitMask();
            WorldExtents = worldExtents;
        }

        public bool Execute()
        {
            try
            {
                PerformScanning();

                return true;  
            }
            catch // (Exception E)
            {
                // TODO Readd when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('Exception: ''%s''', [E.Message]), slmcException);
                return false;
            }
        }

        protected bool SubGridEvent(ISubGrid SubGrid)
        {
            byte ScanMinXb, ScanMinYb, ScanMaxXb, ScanMaxYb;
            bool SubgridSatisfiesFilter;
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

            for (byte I = ScanMinXb; I < ScanMaxXb + 1; I++)
            {
                for (byte J = ScanMinYb; J < ScanMaxYb + 1; J++)
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
                            if (!Owner.DesignSubgridOverlayMap.GetCell((uint)(SubGrid.OriginX + I), (uint)(SubGrid.OriginY + J)))
                            {
                                continue;
                            }
                        }

                        Debug.Assert(Owner.ProdDataExistenceMap != null, "Production Data Existance Map should have been specified");

                        // If there is a spatial filter in play then determine if the subgrid about to be requested intersects the spatial filter extent

                        SubgridSatisfiesFilter = true;
                        for (int FilterIdx = 0; FilterIdx < Owner.FilterSet.Filters.Length; FilterIdx++)
                        {
                            if (Owner.FilterSet.Filters[FilterIdx] != null)
                            {
                                CellSpatialFilter spatialFilter = Owner.FilterSet.Filters[FilterIdx].SpatialFilter;

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
                        }

                        if (SubgridSatisfiesFilter)
                        {
                            TotalNumberOfSubgridsAnalysed++;

                            // Add the leaf subgrid identitied by the address below, along with the production data and surveyed surface
                            // flags to the subgrid tree being used to aggregate all the subgrids that need to be queried for the request
//                            SubGridCellAddress NewSubGridAddress =
//                                 new SubGridCellAddress((CastSubGrid.OriginX + I) << SubGridTree.SubGridIndexBitsPerLevel,
//                                                        (CastSubGrid.OriginY + J) << SubGridTree.SubGridIndexBitsPerLevel,
//                                                        Owner.ProdDataExistenceMap.GetCell(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J),
//                                                        Owner.IncludeSurveyedSurfaceInformation);

                            /// Set the mask for the production data
                            Mask.SetCell(CastSubGrid.OriginX + I, CastSubGrid.OriginY + J, true);

                            /* TODO - Do the same for surveyed surface information */
                        }
                    }
                }
            }

            return Result;
        }
    }
}
 
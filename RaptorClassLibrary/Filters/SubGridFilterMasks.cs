using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Server;

namespace VSS.VisionLink.Raptor.Filters
{
    /// <summary>
    /// Methods that support the calculation of subgrid filter masks controlling which cells in a subgrid
    /// are valid for processing
    /// </summary>
    public static class SubGridFilterMasks
    {
        public static void ConstructSubgridSpatialAndPositionalMask(ILeafSubGrid SubGridAsLeaf,
                                                   SiteModel SiteModel,
                                                   CombinedFilter Filter,
                                                   bool AHasOverrideSpatialCellRestriction,
                                                   BoundingIntegerExtent2D AOverrideSpatialCellRestriction,
                                                   ref SubGridTreeBitmapSubGridBits PDMask,
                                                   ref SubGridTreeBitmapSubGridBits FilterMask)
        {
            double CX, CY;
            uint OriginXPlusI;
            bool SubGridAsLeaf_is_TICServerSubGridTreeLeaf;

            if ((Filter == null) || !Filter.SpatialFilter.HasSpatialOrPostionalFilters)
            {
                PDMask.Fill();
                FilterMask.Fill();
                return;
            }

            FilterMask.Clear();

            // Construct the filter mask based on the spatial and location (square/circle/polygonal) filtering
            for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
            {
                OriginXPlusI = SubGridAsLeaf.OriginX + I;

                for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                {
                    if (AHasOverrideSpatialCellRestriction && !AOverrideSpatialCellRestriction.Includes((int)OriginXPlusI, (int)SubGridAsLeaf.OriginY + J))
                    {
                        continue;
                    }

                    SiteModel.Grid.GetCellCenterPosition(OriginXPlusI, SubGridAsLeaf.OriginY + J, out CX, out CY);
                    if (!Filter.SpatialFilter.IsCellInSelection(CX, CY))
                    {
                        continue;
                    }

                    FilterMask.SetBit(I, J);
                }
            }

            // Handle the case when the passed in subgrid is a server leaf subgrid. In this case, construct the PDMask so that
            // it denotes the production data cells (only) that were selected by the spatial filter.
            SubGridAsLeaf_is_TICServerSubGridTreeLeaf = SubGridAsLeaf is ServerSubGridTreeLeaf;
            if (SubGridAsLeaf_is_TICServerSubGridTreeLeaf)
            {
                PDMask = FilterMask & ((ServerSubGridTreeLeaf)SubGridAsLeaf).Directory.GlobalLatestCells.PassDataExistanceMap;
            }
            else
            {
                PDMask.Clear();
            }
        }


        public static bool ConstructSubgridCellFilterMask(ILeafSubGrid SubGridAsLeaf,
                                        SiteModel SiteModel,
                                        CombinedFilter Filter,
                                        SubGridTreeBitmapSubGridBits CellOverrideMask,
                                        bool AHasOverrideSpatialCellRestriction,
                                        BoundingIntegerExtent2D AOverrideSpatialCellRestriction,
                                        ref SubGridTreeBitmapSubGridBits PDMask,
                                        ref SubGridTreeBitmapSubGridBits FilterMask)
        {
            // TODO: No design alignment mask support... RequestResult: TDesignProfilerRequestResult;
            // SubGridTreeBitmapSubGridBits AlignMask;
            // Fence DesignBoundary = null;

            bool Result = true;

            ConstructSubgridSpatialAndPositionalMask(SubGridAsLeaf, SiteModel, Filter,
                                                     AHasOverrideSpatialCellRestriction, AOverrideSpatialCellRestriction,
                                                     ref PDMask, ref FilterMask);

            // Apply any override mask supplied by the caller. If all bits are required in the override,
            // then a filled mask should be supplied...
            PDMask = PDMask & CellOverrideMask;

            /* TODO - Design/alignment masks not yet supported
            // If the filter contains a design mask filter then compute this and AND it with the
            // mask calculated in the step above to derive the final required filter mask

            if (Filter != null && Filter.SpatialFilter.HasAlignmentDesignMask())
            {
                if (Filter.SpatialFilter.AlignmentFence.IsNull()) // Should have been done in ASNode but if not
                {
                    RemoveDesignBoundaryFence = true;
                    RequestResult = DesignProfilerLayerLoadBalancer.LoadBalancedDesignProfilerService.RequestDesignFilterBoundary
                        (Construct_CalculateDesignFilterBoundary_Args
                        (SiteModel.ID,
                         Filter.SpatialFilter.ReferenceDesign,
                         Filter.SpatialFilter.StartStation, Filter.SpatialFilter.EndStation,
                         Filter.SpatialFilter.LeftOffset, Filter.SpatialFilter.RightOffset,
                         dfbrtList),
                        DesignBoundary);
                }
                else
                {
                    RequestResult = dppiOK; // boundy fence lookup work has been done in ASNode
                }
                DesignBoundary = Filter.SpatialFilter.AlignmentFence;
            }

            if (RequestResult == dppiOK)
            {
                AlignMask = FilterMask;

                // Go over setbits and determine if they are in Design fence boundary
                //with SubGridAsLeaf, SiteModel.Grid do
                AlignMask.ForEachSetBit((X, Y) =>
                {
                    double CX, CY;
                    SiteModel.Grid.GetCellCenterPosition((uint)(SubGridAsLeaf.OriginX + X), (uint)(SubGridAsLeaf.OriginY + Y), out CX, out CY);
                    if (!DesignBoundary.IncludesPoint(CX, CY))
                    {
                        AlignMask.ClearBit(X, Y); // remove interest as its not in design boundry
                    }
                });

                FilterMask = AlignMask; // update filtermask after design boundary filter applied
                PDMask = PDMask & FilterMask;
            }
            else
            {
                Result = false;
                // TODO Readd when logging available
                //SIGLogMessage.PublishNoODS(Nil, Format('Call to RequestDesignFilterBoundary in ICRetrieveSubgrid.ConstructSubgridCellFilterMask returned error result %s for %s.',
                //                                [DesignProfilerErrorStatusName(RequestResult), CellFilter.ReferenceDesign.ToString]), slmcError);
            }
            */

            return Result;
        }
    }
}

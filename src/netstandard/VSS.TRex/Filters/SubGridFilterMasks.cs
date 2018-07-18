using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.Filters
{
    /// <summary>
    /// Methods that support the calculation of subgrid filter masks controlling which cells in a subgrid
    /// are valid for processing
    /// </summary>
    public static class SubGridFilterMasks
    {
        public static void ConstructSubgridSpatialAndPositionalMask(ILeafSubGrid SubGridAsLeaf,
                                                                    ISiteModel SiteModel,
                                                                    CombinedFilter Filter,
                                                                    bool AHasOverrideSpatialCellRestriction,
                                                                    BoundingIntegerExtent2D AOverrideSpatialCellRestriction,
                                                                    ref SubGridTreeBitmapSubGridBits PDMask,
                                                                    ref SubGridTreeBitmapSubGridBits FilterMask)
        {
            if ((Filter == null) || !Filter.SpatialFilter.HasSpatialOrPostionalFilters)
            {
                PDMask.Fill();
                FilterMask.Fill();
                return;
            }

            uint originX = SubGridAsLeaf.OriginX;
            uint originY = SubGridAsLeaf.OriginY;

            double cellSize = SiteModel.Grid.CellSize;

            // Get the world location of the origin position
            SiteModel.Grid.GetCellCenterPosition(originX, originY, out double OX, out double OY);

            CellSpatialFilter SpatialFilter = Filter.SpatialFilter;

            // Attempt to satisfy the calculation below on the basis of the subgrid wholly resising in the overide and filter spatial restrictions
            if (SpatialFilter.Fence.IncludesExtent(new BoundingWorldExtent3D(OX, OY,
                                                                             OX + cellSize * SubGridTree.SubGridTreeDimension,
                                                                             OY + cellSize * SubGridTree.SubGridTreeDimension)))
            {
                // The extent of the subgrid is wholly contained in the filter, therefore there is no need to iterate though all the cells
                // individually...

                FilterMask.Fill();

                // ... unless there is an override spatial cell restriction that does not enclose the extent of the subgrid
                if (AHasOverrideSpatialCellRestriction &&
                    !AOverrideSpatialCellRestriction.Encloses(new BoundingIntegerExtent2D((int)originX, (int)originY,
                                                                                          (int)originX + SubGridTree.SubGridTreeDimension,
                                                                                          (int)originY + SubGridTree.SubGridTreeDimension)))
                {
                    for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
                    {
                        for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                        {
                            if (!AOverrideSpatialCellRestriction.Includes(originX + I, originY + J))
                                FilterMask.ClearBit(I, J);
                        }
                    }
                }
            }
            else
            {
                // Perform the calculation the long hand way
                // ... Idea: Invert row and column order of calculation below to get and set bits based on an entire column of bits

                FilterMask.Clear();

                // Construct the filter mask based on the spatial and location (square/circle/polygonal) filtering
                double CX = OX;

                for (byte I = 0; I < SubGridTree.SubGridTreeDimension; I++)
                {
                    uint OriginXPlusI = originX + I;
                    double CY = OY; // Set to the first row in the column about to be processed

                    for (byte J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                    {
                        if (AHasOverrideSpatialCellRestriction && !AOverrideSpatialCellRestriction.Includes((int)OriginXPlusI, (int)(originY + J)))
                        {
                            // Do nothing
                        }
                        else
                        {
                            // SiteModel.Grid.GetCellCenterPosition(OriginXPlusI, originY + J, out CX, out CY);
                            if (SpatialFilter.IsCellInSelection(CX, CY))
                            {
                                FilterMask.SetBit(I, J);
                            }
                        }

                        CY += cellSize; // Move to next row
                    }

                    CX += cellSize; // Move to bext column
                }
            }

            // Handle the case when the passed in subgrid is a server leaf subgrid. In this case, construct the PDMask so that
            // it denotes the production data cells (only) that were selected by the spatial filter.
            bool SubGridAsLeaf_is_TICServerSubGridTreeLeaf = SubGridAsLeaf is IServerLeafSubGrid;
            if (SubGridAsLeaf_is_TICServerSubGridTreeLeaf)
            {
                PDMask.SetAndOf(FilterMask, ((IServerLeafSubGrid)SubGridAsLeaf).Directory.GlobalLatestCells.PassDataExistanceMap);
            }
            else
            {
                PDMask.Clear();
            }
        }


        public static bool ConstructSubgridCellFilterMask(ILeafSubGrid SubGridAsLeaf,
                                        ISiteModel SiteModel,
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
            PDMask.AndWith(CellOverrideMask);

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
                PDMask.AndWith(FilterMask);
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

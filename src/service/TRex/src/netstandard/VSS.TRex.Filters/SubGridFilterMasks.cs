using System;
using VSS.TRex.Filters.Interfaces;
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
                                                                    ICombinedFilter Filter,
                                                                    bool AHasOverrideSpatialCellRestriction,
                                                                    BoundingIntegerExtent2D AOverrideSpatialCellRestriction,
                                                                    SubGridTreeBitmapSubGridBits PDMask,
                                                                    SubGridTreeBitmapSubGridBits FilterMask)
        {
            if (Filter == null || !Filter.SpatialFilter.HasSpatialOrPositionalFilters)
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

            ICellSpatialFilter SpatialFilter = Filter.SpatialFilter;

            // Attempt to satisfy the calculation below on the basis of the subgrid wholly resizing in the override and filter spatial restrictions
            if (SpatialFilter.Fence.IncludesExtent(new BoundingWorldExtent3D(OX, OY,
                                                                             OX + cellSize * SubGridTreeConsts.SubGridTreeDimension,
                                                                             OY + cellSize * SubGridTreeConsts.SubGridTreeDimension)))
            {
                // The extent of the subgrid is wholly contained in the filter, therefore there is no need to iterate though all the cells
                // individually...

                FilterMask.Fill();

                // ... unless there is an override spatial cell restriction that does not enclose the extent of the subgrid
                if (AHasOverrideSpatialCellRestriction &&
                    !AOverrideSpatialCellRestriction.Encloses(new BoundingIntegerExtent2D((int)originX, (int)originY,
                                                                                          (int)originX + SubGridTreeConsts.SubGridTreeDimension,
                                                                                          (int)originY + SubGridTreeConsts.SubGridTreeDimension)))
                {
                    for (byte I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
                    {
                        for (byte J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
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

                for (byte I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
                {
                    uint OriginXPlusI = originX + I;
                    double CY = OY; // Set to the first row in the column about to be processed

                    for (byte J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
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

                    CX += cellSize; // Move to next column
                }
            }

            // Handle the case when the passed in subgrid is a server leaf subgrid. In this case, construct the PDMask so that
            // it denotes the production data cells (only) that were selected by the spatial filter.
            bool SubGridAsLeaf_is_TICServerSubGridTreeLeaf = SubGridAsLeaf is IServerLeafSubGrid;
            if (SubGridAsLeaf_is_TICServerSubGridTreeLeaf)
            {
                PDMask.SetAndOf(FilterMask, ((IServerLeafSubGrid)SubGridAsLeaf).Directory.GlobalLatestCells.PassDataExistenceMap);
            }
            else
            {
                PDMask.Clear();
            }
        }


        public static bool ConstructSubgridCellFilterMask(ILeafSubGrid SubGridAsLeaf,
                                        ISiteModel SiteModel,
                                        ICombinedFilter Filter,
                                        SubGridTreeBitmapSubGridBits CellOverrideMask,
                                        bool AHasOverrideSpatialCellRestriction,
                                        BoundingIntegerExtent2D AOverrideSpatialCellRestriction,
                                        SubGridTreeBitmapSubGridBits PDMask,
                                        SubGridTreeBitmapSubGridBits FilterMask)
        {
            bool Result = true;

            ConstructSubgridSpatialAndPositionalMask(SubGridAsLeaf, SiteModel, Filter,
                                                     AHasOverrideSpatialCellRestriction, AOverrideSpatialCellRestriction,
                                                     PDMask, FilterMask);

            // Apply any override mask supplied by the caller. If all bits are required in the override,
            // then a filled mask should be supplied...
            PDMask.AndWith(CellOverrideMask);

            // If the filter contains a design mask filter then compute this and AND it with the
            // mask calculated in the step above to derive the final required filter mask
            if (Filter != null && Filter.SpatialFilter.HasAlignmentDesignMask())
            {
                if (Filter.SpatialFilter.AlignmentFence.IsNull()) // Should have been done in ASNode but if not
                    throw new ArgumentException($"Spatial filter does not contained pre-prepared alignment fence for design {Filter.SpatialFilter.AlignmentDesignMaskDesignUID}");
              
                // Go over set bits and determine if they are in Design fence boundary
                FilterMask.ForEachSetBit((X, Y) =>
                {
                    SiteModel.Grid.GetCellCenterPosition((uint) (SubGridAsLeaf.OriginX + X), (uint) (SubGridAsLeaf.OriginY + Y), out var CX, out var CY);
                    if (!Filter.SpatialFilter.AlignmentFence.IncludesPoint(CX, CY))
                    {
                        FilterMask.ClearBit(X, Y); // remove interest as its not in design boundary
                    }
                });
            }

            PDMask.AndWith(FilterMask);           

            return Result;
        }
    }
}

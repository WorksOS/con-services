using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.Velociraptor.Designs.TTM;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.Velociraptor.DesignProfiling
{
    public class TTMDesign : DesignBase
    {
        private TrimbleTINModel FData;
        private double FMinHeight;
        private double FMaxHeight;
        //      private FIndex : TDQMTTMQuadTree;
        private double FCellSize;
        private SubGridTreeBitMask FSubgridIndex;

        private void SwapVertices(ref TriVertex A, ref TriVertex B) => MinMax.Swap(ref A, ref B);


        public TrimbleTINModel Data { get { return FData; } }

        // No spatial index for now...
        // property Index : TDQMTTMQuadTree read FIndex;

        public override object CreateAccessContext() => null; /* TDQMTTMQuadTree.Clone(FIndex, False); */

        private void AddTrianglePieceToElevationPatch(TriVertex H1, TriVertex H2, TriVertex V,
                                                      Triangle Tri,
                                                      bool SingleRowOnly,
                                                      double OriginX, double OriginY,
                                                      double CellSize,
                                                      float[,] Patch,
                                                      double OffSet,
                                                      ref int ValueCount)
        {
            double Y, Z;
            double YRange;
            int YStep;
            double H1X, H1Y, H2X, H2Y;
            int ProcessingCellYIndex;
            double HalfCellSize;
            double HalfMinorCellSize;
            double H1Slope, H2Slope;
            double H1SlopeTimesCellSize;
            double H2SlopeTimesCellSize;
            double PatchSize;
            double VX, VY;
            double TopEdge, RightEdge;
            double OriginXPlusHalfCell, OriginYPlusHalfCell;
            double TopEdgeLessHalfCell, RightEdgeLessHalfCell;
            int PatchOriginCellIndexX, PatchOriginCellIndexY;
            int PatchCellLimitIndexX;
            int LeftCellIndexX, RightCellIndexX;
            int HCellIndexY, VCellIndexY;
            int NumCellRowsToProcess;
            int VCellPatchIndex, HCellPatchIndex;
            int Delta;
            double AbsH1SlopeTimesCellSize, AbsH2SlopeTimesCellSize;

            try
            {
                // H1 and H2 describe the horizontal portion of the triangle piece
                // V describes the vertex above, or below the horizontal line

                // Ensure H1 is left of H2 and take local copies of the vertex ordinates
                if (H1.X > H2.X)
                {
                    SwapVertices(ref H1, ref H2);
                }

                H1X = H1.X;
                H1Y = H1.Y;
                H2X = H2.X;
                H2Y = H2.Y;
                VX = V.X;
                VY = V.Y;

                // HalfMinorCellSize is half of the cell size of the on-the-ground cells that
                // will be compared against the TIN design surface during cut fill operations.
                // As the sample point for a cell is the center point of the cell then there is
                // no need to include a half cell width outer boundary of each cell in the subgrid
                // index. A small epsilon value is deducted from the half cell size value to prevent
                // numeric imprecision
                HalfCellSize = CellSize / 2;
                HalfMinorCellSize = HalfCellSize - 0.001;

                PatchSize = SubGridTree.SubGridTreeDimension * CellSize;
                TopEdge = OriginY + PatchSize;
                RightEdge = OriginX + PatchSize;

                OriginXPlusHalfCell = OriginX + HalfMinorCellSize;
                OriginYPlusHalfCell = OriginY + HalfMinorCellSize;

                TopEdgeLessHalfCell = TopEdge - HalfMinorCellSize;
                RightEdgeLessHalfCell = RightEdge - HalfMinorCellSize;

                // Check to see if the triangle piece being considered could possibly intersect
                // the extent of the patch (or any of the cell center positions at which the
                // spot elevation are calculated).
                if (((H1X > RightEdgeLessHalfCell) && (VX > RightEdgeLessHalfCell)) ||
                    ((H2X < OriginXPlusHalfCell) && (VX < OriginXPlusHalfCell)) ||
                    ((H1Y > TopEdgeLessHalfCell) && (H2Y > TopEdgeLessHalfCell) && (VY > TopEdgeLessHalfCell)) ||
                    ((H1Y < OriginYPlusHalfCell) && (H2Y < OriginYPlusHalfCell) && (VY < OriginYPlusHalfCell)))
                {
                    // The triangle piece cannot intersect the patch
                    return;
                }

                PatchOriginCellIndexX = (int)Math.Floor((OriginX + HalfMinorCellSize) / CellSize);
                PatchOriginCellIndexY = (int)Math.Floor((OriginY + HalfMinorCellSize) / CellSize);
                PatchCellLimitIndexX = PatchOriginCellIndexX + SubGridTree.SubGridTreeDimension - 1;

                // Work out 'Y' range and step direction of the triangle piece.
                YRange = VY - H1Y;
                YStep = Math.Sign(YRange);

                try
                {
                    if (SingleRowOnly)
                    {
                        H1Slope = 0;
                        H2Slope = 0;
                    }
                    else
                    {
                        H1Slope = (VX - H1X) / Math.Abs(YRange);
                        H2Slope = (H2X - VX) / Math.Abs(YRange);
                    }
                }
                catch
                {
                    H1Slope = 0;
                    H2Slope = 0;
                }

                H1SlopeTimesCellSize = H1Slope * CellSize;
                H2SlopeTimesCellSize = H2Slope * CellSize;

                AbsH1SlopeTimesCellSize = Math.Abs(H1SlopeTimesCellSize) + 0.001;
                AbsH2SlopeTimesCellSize = Math.Abs(H2SlopeTimesCellSize) + 0.001;

                // ProcessingCellYIndex is used to ensure that each 'row' of cells is adjacent to the
                // previous row to ensure a row of cells is not skipped in the event that
                // H1 and H2 vertices lie on the boundary of two cells which may cause numeric
                // imprecision when the H1 and H2 vertices are updated after scanning across the
                // cells in the row.

                VCellIndexY = (int)Math.Floor(VY / CellSize);
                HCellIndexY = (int)Math.Floor(H1Y / CellSize);

                VCellPatchIndex = VCellIndexY - PatchOriginCellIndexY;
                HCellPatchIndex = HCellIndexY - PatchOriginCellIndexY;

                NumCellRowsToProcess = Math.Abs(VCellPatchIndex - HCellPatchIndex) + 1;

                ProcessingCellYIndex = HCellPatchIndex;

                // Determine how many rows of cells there are between ProcessingCellYIndex and
                // the extent covered by the subgrid. Shift the H1X/H2X/etc values appropriately,
                // also clamping the starting cell row index to the patch
                if (HCellPatchIndex < 0)
                {
                    if (YStep == -1) // There's nothing more to be done here
                    {
                        return;
                    }

                    Delta = -HCellPatchIndex;
                    H1X = H1X + Delta * H1SlopeTimesCellSize;
                    H2X = H2X - Delta * H2SlopeTimesCellSize;

                    NumCellRowsToProcess -= Delta;
                    ProcessingCellYIndex = 0;
                }
                else
                {
                    if (HCellPatchIndex >= SubGridTree.SubGridTreeDimension)
                    {
                        if (YStep == 1) // There's nothing more to be done here
                        {
                            return;
                        }

                        Delta = (HCellPatchIndex - SubGridTree.SubGridTreeDimension) + 1;
                        H1X = H1X + Delta * H1SlopeTimesCellSize;
                        H2X = H2X - Delta * H2SlopeTimesCellSize;

                        NumCellRowsToProcess -= Delta;
                        ProcessingCellYIndex = SubGridTree.SubGridTreeDimension - 1;
                    }
                }

                // Clamp the ending cell row to be processed to the patch
                if (VCellPatchIndex < 0)
                {
                    if (YStep == 1)
                    {
                        return; // Nothing more to do here
                    }

                    NumCellRowsToProcess -= -VCellPatchIndex;
                }
                else if (VCellPatchIndex >= SubGridTree.SubGridTreeDimension)
                {
                    if (YStep == -1)
                    {
                        return; // Nothing more to do here
                    }

                    NumCellRowsToProcess -= ((VCellPatchIndex - SubGridTree.SubGridTreeDimension) + 1);
                }

                if (NumCellRowsToProcess == 0)
                {
                    return;
                }

                // Widen the H1/H2 spread to adequately cover the cells in the interval
                // as iterating across just this interval will leave cells on the extreme
                // edges missed out from the spot elevation calcs

                H1X = H1X - AbsH1SlopeTimesCellSize;
                H2X = H2X + AbsH2SlopeTimesCellSize;

                // Note: H1X & H2X are modified in the loop after this location

                // Repeatedly scan over rows of cells that cover the triangle piece checking
                // if they cover the body of the triangle
                do //repeat
                {
                    // Calculate the positions of the left and right cell indices in the coordinate space of the
                    // triangle piece
                    LeftCellIndexX = (int)Math.Floor(H1X / CellSize);
                    RightCellIndexX = (int)Math.Floor(H2X / CellSize) + 1;

                    // Clip the calculated cell indices against the coordinate space of the patch
                    if (LeftCellIndexX < PatchOriginCellIndexX)
                        LeftCellIndexX = PatchOriginCellIndexX;
                    if (RightCellIndexX > PatchCellLimitIndexX)
                        RightCellIndexX = PatchCellLimitIndexX;

                    if (LeftCellIndexX <= RightCellIndexX)
                    {
                        Y = ((PatchOriginCellIndexY + ProcessingCellYIndex) * CellSize) + HalfCellSize;

                        for (int I = LeftCellIndexX; I < RightCellIndexX; I++)
                        {
                            Z = Tri.GetHeight((I * CellSize) + HalfCellSize, Y);

                            if (Z != Consts.kTTMNullReal)
                            {
                                if (Patch[I - PatchOriginCellIndexX, ProcessingCellYIndex] == Consts.NullHeight)
                                {
                                    ValueCount++;
                                    Patch[I - PatchOriginCellIndexX, ProcessingCellYIndex] = (float)(Z + OffSet);
                                }
                            }
                        }
                    }

                    // Recalculate the left and right cell indexors for the next row of cells to be scanned across the triangle.
                    H1X = H1X + H1SlopeTimesCellSize;
                    H2X = H2X - H2SlopeTimesCellSize;

                    NumCellRowsToProcess--;
                    ProcessingCellYIndex += YStep;

                    //      if (NumCellRowsToProcess > 0) and not InRange(ProcessingCellYIndex, 0, kSubGridTreeDimension - 1) then
                    //        SIGLogMessage.PublishNoODS(Self, Format('ProcessingCellYIndex (%d) out of range', [ProcessingCellYIndex]), slmcException);
                }
                while ((NumCellRowsToProcess > 0) && !SingleRowOnly); // or not InRange(ProcessingCellYIndex, 0, kSubGridTreeDimension - 1);
            }
            catch // (Exception E)
            {
                //SIGLogMessage.PublishNoODS(Self, Format('Exception ''%s'' raised in TTTMDesign.AddTrianglePieceToSubgridIndex', [E.Message]), slmcException);
                throw;
            }
        }

        private void AddTrianglePieceToSubgridIndex(TriVertex H1, TriVertex H2, TriVertex V, bool SingleRowOnly)
        {
            double YRange;
            int YStep;
            uint LeftSubGridX, RightSubGridX, SubGridY;
            uint PrevLeftSubGridX, PrevRightSubGridX;
            BoundingWorldExtent3D Extents;
            double H1X, H1Y, H2X, H2Y;
            uint OverrideSubGridY;
            double HalfMinorCellSize;
            double H1Slope, H2Slope;
            double YStepTimesCellSize;
            double H1SlopeTimesCellSize;
            double H2SlopeTimesCellSize;
            bool FirstRow;
            bool LastRow;
            double SubgridFraction;
            double T;

            try
            {
                // H1 and H2 describe the horizontal portion of the triangle piece
                // V describes the vertex above, or below the horizontal line

                // Ensure H1 is left of H2 and take local copies of the vertex ordinates

                if (H1.X > H2.X)
                {
                    SwapVertices(ref H1, ref H2);
                }

                H1X = H1.X;
                H1Y = H1.Y;
                H2X = H2.X;
                H2Y = H2.Y;

                // Work out 'Y' range and step direction of the triangle piece.
                YRange = V.Y - H1.Y;
                YStep = Math.Sign(YRange);
                YStepTimesCellSize = YStep * FSubgridIndex.CellSize;

                try
                {
                    if (SingleRowOnly)
                    {
                        H1Slope = 0;
                        H2Slope = 0;
                    }
                    else
                    {
                        H1Slope = (V.X - H1.X) / Math.Abs(YRange);
                        H2Slope = (H2.X - V.X) / Math.Abs(YRange);
                    }
                }
                catch
                {
                    H1Slope = 0;
                    H2Slope = 0;
                }

                H1SlopeTimesCellSize = H1Slope * FSubgridIndex.CellSize;
                H2SlopeTimesCellSize = H2Slope * FSubgridIndex.CellSize;

                OverrideSubGridY = uint.MaxValue;

                // HalfMinorCellSize is half of the cell size of the on-the-ground cells that
                // will be compared against the TIN design surface during cut fill operations.
                // As the sample point for a cell is the center point of the cell then there is
                // no need to include a half cell width outer boundary of each cell in the subgrid
                // index. A small epsilon value is deducted from the half cell size value to prevent
                // numeric imprecision
                HalfMinorCellSize = ((FSubgridIndex.CellSize / SubGridTree.SubGridTreeDimension) / 2) - 0.0001;

                // Calculate a fraction of the horizontal movement distances required to move the
                // H1X and H2X positions to the leading 'edge' of the first row of subgrids
                // that they will cross as the subgrids are scanned. This is an adjusment made after
                // the first row of subgrids is analysed to ensure that the left and right subgrid
                // indices of the next subgrid row are correctly determined due to the lines between
                // H1->V and H2->V crossing the horizontal subgrid boundaries, especially
                // when the slope is shallow.

                // Compute the fractional part of the following calculation...
                T = H1.Y / FSubgridIndex.CellSize;
                T = T - Math.Truncate(T); // T = Math.Frac(H1.Y / FSubgridIndex.CellSize);

                if (V.Y < H1.Y)  // if V is below H1
                {
                    SubgridFraction = (T < 0) ? 1 + T : T;
                }
                else  // V is top
                {
                    SubgridFraction = (T < 0) ? 1 - Math.Abs(T) : 1 - T;
                }

                // Modify the subgrid fraction so the test position is just inside the next subgrid row that
                // H[1|2].Y  in, rather than lying exactly on the boundary between the two rows.
                SubgridFraction = SubgridFraction * 1.001;

                FirstRow = true;
                PrevLeftSubGridX = uint.MaxValue;
                PrevRightSubGridX = uint.MinValue;

                // Repeatedly scan over rows of subgrids that cover the triangle piece checking
                // if they cover the body of the triangle
                do
                {
                    FSubgridIndex.CalculateIndexOfCellContainingPosition(H1X, H1Y, out LeftSubGridX, out SubGridY);
                    FSubgridIndex.CalculateIndexOfCellContainingPosition(H2X, H2Y, out RightSubGridX, out SubGridY);

                    // Bracket the calculate left and right subgrid indices with the previous left and
                    // right subgrid indices to ensure subgrids included via shallow grazing
                    // of near horizontal triangle edges are taken into consideration as each
                    // subsequent row of subgrids is scanned.
                    if (PrevLeftSubGridX < LeftSubGridX)
                        LeftSubGridX = PrevLeftSubGridX;
                    if (PrevRightSubGridX > RightSubGridX)
                        RightSubGridX = PrevRightSubGridX;

                    // OverrideSubGridY is used to ensure that each 'row' of subgrids is adjacent to the
                    // previous row to ensure a row of subgrids is not skipped in the event that
                    // H1 and H2 vertices lie on the boundary of two subgrids which may cause numeric
                    // imprecision when the H1 and H2 vertices are updated after scanning across the
                    // subgrids in the row.

                    if (OverrideSubGridY == uint.MaxValue)
                    {
                        OverrideSubGridY = SubGridY;
                    }

                    for (uint I = LeftSubGridX - 1; I <= RightSubGridX + 1; I++) // Go +- one for some additional safety
                    {
                        if (!FSubgridIndex[I, SubGridY])
                        {
                            Extents = FSubgridIndex.GetCellExtents(I, OverrideSubGridY);
                            Extents.Shrink(HalfMinorCellSize, HalfMinorCellSize);

                            // Does extents of subgrid intersect over triangle
                            if (SubGridIntersectsTriangle(Extents, H1, H2, V))
                            {
                                FSubgridIndex[I, SubGridY] = true; // tag subgrid as true
                            }
                        };
                    }

                    if (FirstRow)
                    {
                        // Move the starting position of the H1X  H2X positions to be at the leading 'edge' of
                        // the first row of subgrids that they will cross as the subgrids are scanned. This is a single
                        // adjustment only so that the per row adjustment that is made below will advance the H1X and
                        // H2X positions correctly into the new row.

                        H1X = H1X + (H1SlopeTimesCellSize * SubgridFraction);
                        H1Y = H1Y + (YStepTimesCellSize * SubgridFraction);
                        H2X = H2X - (H2SlopeTimesCellSize * SubgridFraction);
                        H2Y = H2Y + (YStepTimesCellSize * SubgridFraction);

                        FirstRow = false;
                        PrevLeftSubGridX = LeftSubGridX;
                        PrevRightSubGridX = RightSubGridX;
                    }
                    else
                    {
                        // Recalculate the left and right subgrid indexors for the next row
                        // of subgrids to be scanned across the triangle.
                        H1X = H1X + H1SlopeTimesCellSize;
                        H1Y = H1Y + YStepTimesCellSize;

                        H2X = H2X - H2SlopeTimesCellSize;
                        H2Y = H2Y + YStepTimesCellSize;
                    };

                    OverrideSubGridY = (uint)(OverrideSubGridY + YStep);

                    LastRow = H2X <= H1X;
                }
                while (LeftSubGridX <= RightSubGridX && !LastRow && !SingleRowOnly);
            }
            catch // (Exception E)
            {
                throw;
                //SIGLogMessage.PublishNoODS(Self, Format('Exception ''%s'' raised in TTTMDesign.AddTrianglePieceToSubgridIndex', [E.Message]), slmcException);
            }
        }

        private void ScanCellsOverTriangle(Triangle Tri,
                            Action<TriVertex, TriVertex, TriVertex, bool> ProcessTrianglePiece,
                                                   TriVertex IntersectionVertex)
        {

            TriVertex TopVertex, CentralVertex, BottomVertex;
            TriVertex LeftMostVertex, RightMostVertex;
            TriVertex[] SortVertices = new TriVertex[3];
            bool TopPieceOnly, BottomPieceOnly;
            double IntersectX, IntersectY;
            bool LinesAreCoLinear;

            // Split triangle into two pieces, a 'top' piece and a 'bottom' piece to simplify
            // scanning across the triangle. Split is always with a horizontal line

            for (int I = 0; I < 3; I++)
            {
                SortVertices[I] = Tri.Vertices[I];
            }

            // Make sure triangle has top vertex in last item vertex array
            for (int I = 0; I < 2; I++)
            {
                for (int J = I + 1; J < 3; J++)
                {
                    if (SortVertices[I].Y > SortVertices[J].Y)
                        SwapVertices(ref SortVertices[I], ref SortVertices[J]);
                }
            }

            TopVertex = SortVertices[2];
            CentralVertex = SortVertices[1];
            BottomVertex = SortVertices[0];

            // now make sure leftmost vertex in in first array item
            for (int I = 0; I < 2; I++)
            {
                for (int J = I + 1; J < 3; J++)
                {
                    if (SortVertices[I].X > SortVertices[J].X)
                        SwapVertices(ref SortVertices[I], ref SortVertices[J]);
                }
            }

            LeftMostVertex = SortVertices[0];
            RightMostVertex = SortVertices[2];

            // Are top or bottom vertices coincident with the middle vertex
            BottomPieceOnly = Math.Abs(TopVertex.Y - CentralVertex.Y) < 0.0001;
            TopPieceOnly = Math.Abs(BottomVertex.Y - CentralVertex.Y) < 0.0001;

            if (TopPieceOnly && BottomPieceOnly) // It's a thin horizontal triangle
            {
                ProcessTrianglePiece(LeftMostVertex, RightMostVertex, CentralVertex, true);
            }
            else
            {
                if (!(TopPieceOnly || BottomPieceOnly))
                {
                    // Divide triangle in two with a hort line
                    // Find intersection point of triangle edge between top most and bottom most vertice
                    if (LineIntersection.LinesIntersect(LeftMostVertex.X - 1, CentralVertex.Y,
                                      RightMostVertex.X + 1, CentralVertex.Y,
                                      TopVertex.X, TopVertex.Y,
                                      BottomVertex.X, BottomVertex.Y,
                                      out IntersectX, out IntersectY, true, out LinesAreCoLinear))
                    {
                        IntersectionVertex.XYZ = new XYZ(IntersectX, IntersectY, 0);
                        ProcessTrianglePiece(CentralVertex, IntersectionVertex, TopVertex, false);
                        ProcessTrianglePiece(CentralVertex, IntersectionVertex, BottomVertex, false);
                    }
                    else
                    {
                        // TODO: Readd when logging available
                        // SIGLogMessage.PublishNoODS(Self, Format('Triangle %d failed to have intersection line calculated for it', [Tri.Tag]), slmcWarning);
                        return;
                    }
                }
                else
                {
                    if (TopPieceOnly)
                        ProcessTrianglePiece(BottomVertex, CentralVertex, TopVertex, false);
                    else // BottomPieceOnly
                        ProcessTrianglePiece(TopVertex, CentralVertex, BottomVertex, false);
                }
            }
        }

        public override bool ComputeFilterPatch(object DesignSearchContext,
                                  double StartStn, double EndStn, double LeftOffset, double RightOffset,
                                  SubGridTreeBitmapSubGridBits Mask,
                                  ref SubGridTreeBitmapSubGridBits Patch,
                                  double OriginX, double OriginY,
                                  double CellSize,
                                  DesignDescriptor DesignDescriptor)
        {
            float[,] Heights = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

            if (InterpolateHeights(DesignSearchContext, Heights, OriginX, OriginY, CellSize, DesignDescriptor.Offset))
            {
                // Iterate over the cell bitmask in Mask (ie: the cell this function is instructed to care about and remove cell fromn
                // that mask where there is no non-null elevation in the heights calculated by InterpolateHeights. Return the result
                // back as Patch. Use TempMask as local var capturable by the anonymous function...
                SubGridTreeBitmapSubGridBits TempMask = Mask;

                TempMask.ForEachSetBit((x, y) => { if (Heights[x, y] == Consts.NullHeight) TempMask.ClearBit(x, y); });
                Patch = TempMask;

                //{$IFDEF DEBUG}
                //SIGLogMessage.PublishNoODS(Self, Format('Filter patch construction successful with %d bits', [Patch.CountBits]), slmcDebug);
                //{$ENDIF}

                return true;
            }
            else
            {
                //{$IFDEF DEBUG}
                //SIGLogMessage.PublishNoODS(Self, Format('Filter patch construction failed...', []), slmcDebug);
                //{$ENDIF}

                return false;
            }
        }

        protected bool ConstructSubgridIndex(string SubgridIndexFileName)
        {
            // Read through all the triangles in the model and, for each triangle,
            // determine which subgrids intersect it and set the appropriate bits in the
            // subgrid index.
            try
            {
                // TODO Readd when logging available
                //SIGLogMessage.PublishNoODS(Self, Format('In: Constructing subgrid index for design %s containing %d triangles', [SubgridIndexFileName, FData.Triangles.Count]), slmcMessage);
                TriVertex IntersectionVertex = new TriVertex(0, 0, 0);

                try
                {
                    foreach (Triangle tri in FData.Triangles)
                    {
                        ScanCellsOverTriangle(tri,
                            (H1, H2, V, SingleRowOnly) => AddTrianglePieceToSubgridIndex(H1, H2, V, SingleRowOnly),
                            IntersectionVertex);
                    }
                }
                finally
                {
                    //SIGLogMessage.PublishNoODS(Self, Format('Out: Constructing subgrid index for design %s containing %d triangles', [SubgridIndexFileName, FData.Triangles.Count]), slmcMessage);
                }

                return true;
            }
            catch // (Exception E)
            {
                // TODO readd when logging available
                //  SIGLogMessage.PublishNoODS(Self, Format('Exception in TTTMDesign.ConstructSubgridIndex: %s', [E.Message]), slmcException);
                return false;
            }
        }

        public TTMDesign(double ACellSize) : base()
        {
            FData = new TrimbleTINModel();
            // FIndex = TDQMTTMQuadTree.Create;
            FCellSize = ACellSize;

            // Create a subgrid tree bit mask index that holds one bit per on-the-ground
            // subgrid that intersects at least one triangle in the TTM.
            FSubgridIndex = new SubGridTreeBitMask(SubGridTree.SubGridTreeLevels - 1, SubGridTree.SubGridTreeDimension * ACellSize);
        }

        public override void GetExtents(out double x1, out double y1, out double x2, out double y2)
        {
            x1 = FData.Header.MinimumEasting;
            y1 = FData.Header.MinimumNorthing;
            x2 = FData.Header.MaximumEasting;
            y2 = FData.Header.MaximumNorthing;
        }

        public override void GetHeightRange(out double z1, out double z2)
        {
            if (FMinHeight == Consts.NullReal || FMaxHeight == Consts.NullReal) // better calculate them
            {
                FMinHeight = 1E100;
                FMaxHeight = -1E100;

                foreach (var vertex in FData.Vertices)
                {
                    if (vertex.Z < FMinHeight)
                        FMinHeight = vertex.Z;
                    if (vertex.Z > FMaxHeight)
                        FMaxHeight = vertex.Z;
                }
            }

            z1 = FMinHeight;
            z2 = FMaxHeight;
        }

        /*
            function TTTMDesign.GetMemorySizeInKB: Integer;
            begin
              Result = -1;
            try
    Result = TTrimbleTINModel.MemorySizeInKB(FileName) + (BA_AllocationSize(FIndex.BAtree) div 1024);
            // debug LogWarnToRaptorLog(Format('Full Est%d',[Result]));

            except
              on E: Exception do
                SIGLogMessage.PublishNoODS(Nil, Format('Exception ''%s'' for %s in %s.GetMemorySizeInKB', [E.Message, FileName, Self.ClassName]), slmcException);              
           end;
            end;
*/

        public override bool HasElevationDataForSubGridPatch(double X, double Y)
        {
            FSubgridIndex.CalculateIndexOfCellContainingPosition(X, Y, out uint SubgridX, out uint SubgridY);
            return FSubgridIndex[SubgridX, SubgridY];
        }

        public override bool HasElevationDataForSubGridPatch(uint SubGridX, uint SubGridY) => FSubgridIndex[SubGridX, SubGridY];

        public override bool HasFiltrationDataForSubGridPatch(double X, double Y) => false;

        public override bool HasFiltrationDataForSubGridPatch(uint SubGridX, uint SubgridY) => false;

        public override bool InterpolateHeight(object DesignSearchContext,
                                   ref object Hint,
                                   double X, double Y,
                                   double Offset,
                                   out double Z)
        {
            if (Hint != null)
            {
                Z = (Hint as Triangle).GetHeight(X, Y);
                if (Z != Consts.kTTMNullReal)
                {
                    if (Offset != 0)
                    {
                        Z += Offset;
                    }
                    return true;
                }
            }

            // Get the interpolated height for the triangle (very slow without an index)
            // A spatial index will need to be established...
            Hint = FData.GetTriangleAtPoint(X, Y, out Z);

            return Hint != null && Z != Consts.NullReal;

            /* Legacy implementation utilizing a quadtree index
             SearchState : Search_state_rec;
                        what: entity_type;
                        edata: TDQMEntityBase;
                        eindex: longint;
                        IterationCount: Integer;
                        IterationMax: Integer;

                        begin
                            if Assigned(Hint) then
                begin
                                Z = TTriangle(Hint).GetHeight(X, Y);
                        if Z <> kTTM_NullReal then
                          begin
                                    if OffSet <> 0 then
                        Z = Z + OffSet;
                        Result = True;
                        Exit;
                        end;
                        end;

                        IterationMax = VLPDSvcLocations.DesignProfiler_MaxTrianglesToScanInSpatialSearchForHeightInterpolation;
                        IterationCount = 0;

                        with DesignSearchContext as TDQMTTMQuadTree do
                            begin
                              MapView = Nil;


                            start_search(x - 0.1, y - 0.1, x + 0.1, y + 0.1,
                                         search_triangles, 0, True, SearchState);
                        while next_entity(SearchState, what, eindex, edata) do
                                begin
                                  Hint = FData.Triangles[TDQMTriangleEty(edata).application_info];


                            Inc(IterationCount);

                        if IterationCount >= IterationMax then
                          begin
                                        if IterationCount = IterationMax then
                                         if VLPDSvcLocations.Debug_ExtremeLogSwitchK then
                                            SIGLogMessage.PublishNoODS(Self, Format('Elevation interpolation at %.3f, %.3f in model %s has failed to find triangle after %d candidates examined',
                                                                    [x, y, FData.ModelName, IterationMax]), slmcDebug)
                                        else
                            begin
                                         if VLPDSvcLocations.Debug_ExtremeLogSwitchK then
                                            SIGLogMessage.PublishNoODS(Self, Format('Elevation interpolation now @ triangle index %d, centroid = %s',
                                                                      [TDQMTriangleEty(edata).application_info, TTriangle(Hint).Centroid.ToString]), slmcDebug);

                        if IterationCount > IterationMax + 10 then
                          begin
                                                if VLPDSvcLocations.Debug_ExtremeLogSwitchK then
                                                   SIGLogMessage.PublishNoODS(Self, Format('Elevation interpolation at %.3f, %.3f in model %s being abandoned as no clear solution',
                                                                          [x, y, FData.ModelName]), slmcDebug);
                        Result = False;
                        Z = NullReal;
                        Exit;
                        end;
                        end;
                        end;

                        Z = TTriangle(Hint).GetHeight(X, Y);
                        if Z <> kTTM_NullReal then
                          begin
                                        if OffSet <> 0 then
                            Z = Z + OffSet;
                        Result = True;
                        Exit;
                        end;
                        end;
                        end;

                        Result = False;
                        Z = NullReal;
                        end;
            */
        }

        public override bool InterpolateHeights(object DesignSearchContext,
                                   float[,] Patch, // [TICSubGridCellPassData_HeightPtr] The receiver of the patch of elevations
                                   double OriginX, double OriginY,
                                   double CellSize,
                                   double Offset)
        {
            int ValueCount = 0;
            object Hint = null;
            double HalfCellSize = CellSize / 2;
            double OriginXPlusHalfCellSize = OriginX + HalfCellSize;
            double OriginYPlusHalfCellSize = OriginY + HalfCellSize;

            try
            {
                for (int I = 0; I < SubGridTree.SubGridTreeDimension; I++)
                {
                    double CellSizeTimesI = CellSize * I;

                    for (int J = 0; J < SubGridTree.SubGridTreeDimension; J++)
                    {
                        if (InterpolateHeight(DesignSearchContext, ref Hint,
                                             OriginXPlusHalfCellSize + CellSizeTimesI,
                                             OriginYPlusHalfCellSize + (CellSize * J),
                                             0, out double Z))
                        {
                            Patch[I, J] = (float)(Z + Offset);
                            ValueCount++;
                        }
                        else
                        {
                            Patch[I, J] = Consts.NullHeight;
                        }
                    }
                }

                return ValueCount > 0;
            }
            catch // (Exception E)
            {
                // TODO readd when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('Exception "%s" occurred in TTTMDesign.InterpolateHeights', [E.Message]), slmcException);

                return false;
            }
            /* Old implementation
                        var
                          SearchState : Search_state_rec;
                        what: entity_type;
                        edata: TDQMEntityBase;
                        eindex: longint;
                        Triangle: TTriangle;
                        P: TProcessTrianglePiece;
                        IntersectionVertex: TTriVertex;
                        ValueCount: Integer;

                        Hint: TObject;
                        I, J: Integer;
                        CellSizeTimesI: Double;
                        HalfCellSize: Double;
                        OriginXPlusHalfCellSize: Double;
                        OriginYPlusHalfCellSize: Double;
                        Z: Double;
                        begin
                          Result = False;

                        try
                ValueCount = 0;

                        // Iterate through the triangles scanning the cells positions that overlay each one
                        // and compute the spot elevations into Patch2
                        IntersectionVertex = TTriVertex.Create(0, 0, 0);
                        try
                  with DesignSearchContext as TDQMTTMQuadTree do
                            begin
                                  if VLPDSvcLocations.UsePerCellHeightInterpolationForSubgridElevationPatchCalculation then
                        begin
                                      Hint = Nil;
                        HalfCellSize = CellSize / 2;
                        OriginXPlusHalfCellSize = OriginX + HalfCellSize;
                        OriginYPlusHalfCellSize = OriginY + HalfCellSize;

                        for I = 0 to kSubGridTreeDimension - 1 do
                                begin
                                  CellSizeTimesI = CellSize * I;
                              for J = 0 to kSubGridTreeDimension - 1 do
                                if InterpolateHeight(DesignSearchContext, Hint,
                                                     OriginXPlusHalfCellSize + CellSizeTimesI,
                                                     OriginYPlusHalfCellSize + (CellSize * J),
                                                     0, z) then
                                  begin
                                    Patch[I, J] = Z + Offset;
                        Inc(ValueCount);
                        end
                                            else
                                  Patch[I, J] = kICNullHeight;
                        end
                    end

                      else
                        begin
                          MapView = Nil;
                        start_search(OriginX, OriginY,
                                     OriginX + kSubGridTreeDimension * CellSize, OriginY + kSubGridTreeDimension * CellSize,
                                     search_triangles, 0, True, SearchState);
                        while next_entity(SearchState, what, eindex, edata) do
                                begin
                                  P = Procedure(H1, H2, V: TTriVertex; SingleRowOnly : Boolean)
                                   begin
                                                 AddTrianglePieceToElevationPatch(H1, H2, V, Triangle, SingleRowOnly, OriginX, OriginY, CellSize, Patch, OffSet, ValueCount);
                        end;

                        Triangle = FData.Triangles[TDQMTriangleEty(edata).application_info];
                        ScanCellsOverTriangle(Triangle, P, IntersectionVertex);
                        end;
                        end;

                        end;
                finally
                  IntersectionVertex.Free;
                        end;

                        Result = ValueCount > 0;
                        except
                          on E: Exception do
                            SIGLogMessage.PublishNoODS(Self, Format('Exception "%s" occurred in TTTMDesign.InterpolateHeights', [E.Message]), slmcException);


                       end;
                        end;
            */
        }

        public override DesignLoadResult LoadFromFile(string FileName)
        {
            DesignLoadResult Result = DesignLoadResult.UnknownFailure;

            try
            {
                FData.LoadFromFile(FileName);

                FMinHeight = Consts.NullReal;
                FMaxHeight = Consts.NullReal;

                // FIndex.Initialise(FData, False);

                if (LoadSubgridIndexFile(FileName + Consts.kDesignSubgridIndexFileExt))
                {
                    Result = DesignLoadResult.Success;
                }

                /*
                if (Result == DesignLoadResult.Success)
                    with FData.Header do
                    SIGLogMessage.PublishNoODS(Self, Format('Loaded TTM file %s containing %d triangles and %d vertices. Area: (%.3f, %.3f) -> (%.3f, %.3f): [%.3f x %.3f]',
                                                            [AFileName, NumberOfTriangles, NumberOfVertices,
                                                             MinimumEasting, MinimumNorthing, MaximumEasting, MaximumNorthing,
                                                             MaximumEasting - MinimumEasting, MaximumNorthing - MinimumNorthing]), slmcMessage);
                */
            }
            catch // (Exception E)
            {
                // Readd when logging avbailable
                // SIGLogMessage.PublishNoODS(Self, Format('Exception ''%s'' in %s.LoadFromFile', [E.Message, Self.ClassName]), slmcException);
                Result = DesignLoadResult.UnknownFailure;
            }

            return Result;
        }


        protected bool LoadSubgridIndex(string SubgridIndexFileName)
        {
            try
            {
                if (File.Exists(SubgridIndexFileName))
                {
                    using (FileStream fs = new FileStream(SubgridIndexFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader reader = new BinaryReader(fs))
                        {
                            return SubGridTreePersistor.Read(FSubgridIndex, reader);
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch // (Exception E)
            {
                // Readd when loggin available
                // SIGLogMessage.PublishNoODS(Self, Format('Exception ''%s'' in %s.LoadSubgridIndex', [E.Message, Self.ClassName]), slmcException);

                return false;
            }
        }

        protected bool LoadSubgridIndexFile(string SubgridIndexFileName)
        {
            // TODO Readd when logging available
            // SIGLogMessage.PublishNoODS(Self, Format('Loading subgrid index file %s', [SubgridIndexFileName]), slmcMessage);

            bool Result = LoadSubgridIndex(SubgridIndexFileName);

            if (!Result)
            {
                Result = ConstructSubgridIndex(SubgridIndexFileName);

                if (Result)
                {
                    if (SaveSubgridIndex(SubgridIndexFileName))
                    {
                        // TODO Readd when logging available
                        //SIGLogMessage.PublishNoODS(Self, Format('Saved constructed subgrid index file %s', [SubgridIndexFileName]), slmcMessage)
                    }
                    else
                    {
                        // TODO Readd when logging available
                        //SIGLogMessage.PublishNoODS(Self, Format('Unable to save subgrid index file %s - continuing with unsaved index', [SubgridIndexFileName]), slmcError)
                    }
                }
                else
                {
                    // TODO Readd when logging available
                    // SIGLogMessage.PublishNoODS(Self, Format('Unable to create and save subgrid index file %s', [SubgridIndexFileName]), slmcError);
                }
            }

            return Result;
        }

        protected bool SaveSubgridIndex(string SubgridIndexFileName)
        {
            bool Result = false;

            try
            {
                // Write the index out to a file
                using (FileStream fs = new FileStream(SubgridIndexFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        SubGridTreePersistor.Write(FSubgridIndex, writer);
                    }
                }

                if (!File.Exists(SubgridIndexFileName))
                {
                    Thread.Sleep(500); // Seems to be a Windows update problem hence introduce delay b4 checking again
                }

                /*
                if (!File.Exists(SubgridIndexFileName))
                {
                    try
                    {
                        FSubgridIndexStream.SaveToFile(SubgridIndexFileName);
                    }
                    catch (Exception E)
                    {
                        // When this occurs the file does appear to be saved hence warning only

                        // Readd when logging available
                        //SIGLogMessage.PublishNoODS(Self, 'Note: The following exception is reported as informational and may not signal an operational issue', slmcMessage);
                        //SIGLogMessage.PublishNoODS(Self, Format('Exception ''%s'' in %s.SaveSubgridIndex', [E.Message, Self.ClassName]), slmcWarning);
                    }
                }
                */

                Result = true;
            }
            catch // (Exception E)
            {
                // TODO Readd when logging available
                //SIGLogMessage.PublishNoODS(Self, Format('Exception ''%s'' in %s.SaveSubgridIndex', [E.Message, Self.ClassName]), slmcException);
            }

            return Result;
        }

        private bool SubGridIntersectsTriangle(BoundingWorldExtent3D Extents,
        TriVertex H1, TriVertex H2, TriVertex V)
        {
            bool LinesAreCoincident;
            double IntX, IntY;

            // If any of the triangle vertices are in the cell extents then 'yes'
            if (Extents.Includes(H1.X, H1.Y) || Extents.Includes(H2.X, H2.Y) || Extents.Includes(V.X, V.Y))
            {
                return true;
            }

            // If any of the subgrid corners sit in the triangle then 'yes'
            {
                XYZ H1_XYZ = H1.XYZ;
                XYZ H2_XYZ = H2.XYZ;
                XYZ V_XYZ = V.XYZ;

                if (XYZ.PointInTriangle(H1_XYZ, H2_XYZ, V_XYZ, Extents.MinX, Extents.MinY) ||
                    XYZ.PointInTriangle(H1_XYZ, H2_XYZ, V_XYZ, Extents.MinX, Extents.MaxY) ||
                    XYZ.PointInTriangle(H1_XYZ, H2_XYZ, V_XYZ, Extents.MaxX, Extents.MaxY) ||
                    XYZ.PointInTriangle(H1_XYZ, H2_XYZ, V_XYZ, Extents.MaxX, Extents.MinY))
                {
                    return true;
                }
            }

            // If any of the extent and triangle lines intersect then also 'yes'
            //    with Extents do
            if ((LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MaxX, Extents.MinY, H1.X, H1.Y, H2.X, H2.Y, out IntX, out IntY, false, out LinesAreCoincident) ||
                 LineIntersection.LinesIntersect(Extents.MaxX, Extents.MinY, Extents.MaxX, Extents.MaxY, H1.X, H1.Y, H2.X, H2.Y, out IntX, out IntY, false, out LinesAreCoincident) ||
                 LineIntersection.LinesIntersect(Extents.MinX, Extents.MaxY, Extents.MaxX, Extents.MaxY, H1.X, H1.Y, H2.X, H2.Y, out IntX, out IntY, false, out LinesAreCoincident) ||
                 LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MinX, Extents.MaxY, H1.X, H1.Y, H2.X, H2.Y, out IntX, out IntY, false, out LinesAreCoincident)) ||
                (LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MaxX, Extents.MinY, H1.X, H1.Y, V.X, V.Y, out IntX, out IntY, false, out LinesAreCoincident) ||
                 LineIntersection.LinesIntersect(Extents.MaxX, Extents.MinY, Extents.MaxX, Extents.MaxY, H1.X, H1.Y, V.X, V.Y, out IntX, out IntY, false, out LinesAreCoincident) ||
                 LineIntersection.LinesIntersect(Extents.MinX, Extents.MaxY, Extents.MaxX, Extents.MaxY, H1.X, H1.Y, V.X, V.Y, out IntX, out IntY, false, out LinesAreCoincident) ||
                 LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MinX, Extents.MaxY, H1.X, H1.Y, V.X, V.Y, out IntX, out IntY, false, out LinesAreCoincident)) ||
                (LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MaxX, Extents.MinY, V.X, V.Y, H2.X, H2.Y, out IntX, out IntY, false, out LinesAreCoincident) ||
                 LineIntersection.LinesIntersect(Extents.MaxX, Extents.MinY, Extents.MaxX, Extents.MaxY, V.X, V.Y, H2.X, H2.Y, out IntX, out IntY, false, out LinesAreCoincident) ||
                 LineIntersection.LinesIntersect(Extents.MinX, Extents.MaxY, Extents.MaxX, Extents.MaxY, V.X, V.Y, H2.X, H2.Y, out IntX, out IntY, false, out LinesAreCoincident) ||
                 LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MinX, Extents.MaxY, V.X, V.Y, H2.X, H2.Y, out IntX, out IntY, false, out LinesAreCoincident)))
            {
                return true;
            };

            // Otherwise 'no'
            return false;
        }

        public override SubGridTreeBitMask SubgridOverlayIndex() => FSubgridIndex;
    }
}

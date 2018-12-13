using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.TTM.Optimised;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Designs
{
  /// <summary>
  /// Implements a scanner that iterates over cells that intersect with the geometry of a given triangle.
  /// </summary>
  public class TriangleCellScanner
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<TriangleCellScanner>();

    private readonly Triangle[] TriangleItems;
    private readonly XYZ[] VertexItems;

    public TriangleCellScanner(TrimbleTINModel ttm)
    {
      TriangleItems = ttm.Triangles.Items;
      VertexItems = ttm.Vertices.Items;
    }

    public void ScanCellsOverTriangle(ISubGridTree tree,
         int triIndex,
         Func<ISubGridTree, uint, uint, bool> leafSatisfied,
         Action<ISubGridTree, uint, uint, int> includeTriangleInLeaf,
         Action<ISubGridTree,
           int, // sourceTriangle
           Func<ISubGridTree, uint, uint, bool>, // leafSatisfied
           Action<ISubGridTree, uint, uint, int>, // includeTriangleInLeaf
           XYZ, XYZ, XYZ, bool> ProcessTrianglePiece)
    {
      Triangle Tri = TriangleItems[triIndex];

      // Split triangle into two pieces, a 'top' piece and a 'bottom' piece to simplify
      // scanning across the triangle. Split is always with a horizontal line

      XYZ[] SortVertices = new XYZ[]
      {
        VertexItems[Tri.Vertex0],
        VertexItems[Tri.Vertex1],
        VertexItems[Tri.Vertex2]
      };

      if (SortVertices[0].Y > SortVertices[1].Y) DesignGeometry.SwapVertices(ref SortVertices[0], ref SortVertices[1]);
      if (SortVertices[1].Y > SortVertices[2].Y) DesignGeometry.SwapVertices(ref SortVertices[1], ref SortVertices[2]);
      if (SortVertices[0].Y > SortVertices[1].Y) DesignGeometry.SwapVertices(ref SortVertices[0], ref SortVertices[1]);

      XYZ TopVertex = SortVertices[2];
      XYZ CentralVertex = SortVertices[1];
      XYZ BottomVertex = SortVertices[0];

      // now make sure leftmost vertex in in first array item
      if (SortVertices[0].X > SortVertices[1].X) DesignGeometry.SwapVertices(ref SortVertices[0], ref SortVertices[1]);
      if (SortVertices[1].X > SortVertices[2].X) DesignGeometry.SwapVertices(ref SortVertices[1], ref SortVertices[2]);
      if (SortVertices[0].X > SortVertices[1].X) DesignGeometry.SwapVertices(ref SortVertices[0], ref SortVertices[1]);

      XYZ LeftMostVertex = SortVertices[0];
      XYZ RightMostVertex = SortVertices[2];

      // Are top or bottom vertices coincident with the middle vertex
      bool BottomPieceOnly = Math.Abs(TopVertex.Y - CentralVertex.Y) < 0.0001;
      bool TopPieceOnly = Math.Abs(BottomVertex.Y - CentralVertex.Y) < 0.0001;

      if (TopPieceOnly && BottomPieceOnly) // It's a thin horizontal triangle
      {
        ProcessTrianglePiece(tree, triIndex, leafSatisfied, includeTriangleInLeaf, LeftMostVertex, RightMostVertex, CentralVertex, true);
      }
      else
      {
        if (!(TopPieceOnly || BottomPieceOnly))
        {
          // Divide triangle in two with a horizontal line
          // Find intersection point of triangle edge between top most and bottom most vertices
          if (LineIntersection.LinesIntersect(LeftMostVertex.X - 1, CentralVertex.Y,
            RightMostVertex.X + 1, CentralVertex.Y,
            TopVertex.X, TopVertex.Y,
            BottomVertex.X, BottomVertex.Y,
            out double IntersectX, out double IntersectY, true, out _))
          {
            XYZ IntersectionVertex = new XYZ(IntersectX, IntersectY, 0);
            ProcessTrianglePiece(tree, triIndex, leafSatisfied, includeTriangleInLeaf, CentralVertex, IntersectionVertex, TopVertex, false);
            ProcessTrianglePiece(tree, triIndex, leafSatisfied, includeTriangleInLeaf, CentralVertex, IntersectionVertex, BottomVertex, false);
          }
          else
          {
            Log.LogWarning($"Triangle {Tri} failed to have intersection line calculated for it");
          }
        }
        else
        {
          if (TopPieceOnly)
          {
            ProcessTrianglePiece(tree, triIndex, leafSatisfied, includeTriangleInLeaf, BottomVertex, CentralVertex, TopVertex, false);
          }
          else // BottomPieceOnly
          {
            ProcessTrianglePiece(tree, triIndex, leafSatisfied, includeTriangleInLeaf, TopVertex, CentralVertex, BottomVertex, false);
          }
        }
      }
    }

    /// <summary>
    /// AddTrianglePieceToSubgridIndex_Extents is used to prevent very large numbers of bounding extent record allocations in the
    /// AddTrianglePieceToSubgridIndex method. Note: This code is expected to be single threaded. Note: Do not use this
    /// member in any other context
    /// </summary>
    private BoundingWorldExtent3D AddTrianglePieceToSubgridIndex_Extents = new BoundingWorldExtent3D();

    public void AddTrianglePieceToSubgridIndex(ISubGridTree index,
      int sourceTriangle,
      Func<ISubGridTree, uint, uint, bool> leafSatisfied,
      Action<ISubGridTree, uint, uint, int> includeTriangleInLeaf,
      XYZ H1, XYZ H2, XYZ V, bool SingleRowOnly)
    {
      double H1Slope, H2Slope;
      bool LastRow = false;
      bool WasLastRow; // = false;

      // H1 and H2 describe the horizontal portion of the triangle piece
      // V describes the vertex above, or below the horizontal line

      // Ensure H1 is left of H2 and take local copies of the vertex ordinates
      if (H1.X > H2.X)
        DesignGeometry.SwapVertices(ref H1, ref H2);

      double H1X = H1.X;
      double H1Y = H1.Y;
      double H2X = H2.X;
      double H2Y = H2.Y;

      // Work out 'Y' range and step direction of the triangle piece.
      double YRange = V.Y - H1Y;
      int YStep = Math.Sign(YRange);
      double YStepTimesCellSize = YStep * index.CellSize;

      try
      {
        if (SingleRowOnly)
        {
          H1Slope = 0;
          H2Slope = 0;
        }
        else
        {
          H1Slope = (V.X - H1X) / Math.Abs(YRange);
          H2Slope = (H2X - V.X) / Math.Abs(YRange);
        }
      }
      catch
      {
        H1Slope = 0;
        H2Slope = 0;
      }

      double H1SlopeTimesCellSize = H1Slope * index.CellSize;
      double H2SlopeTimesCellSize = H2Slope * index.CellSize;

      bool FirstRow = true;
      uint PrevLeftSubGridX = uint.MaxValue;
      uint PrevRightSubGridX = uint.MinValue;

      // Determine the start and end rows
      index.CalculateIndexOfCellContainingPosition(H1X, H1Y, out _, out uint SubgridStartY);
      index.CalculateIndexOfCellContainingPosition(V.X, V.Y, out _, out uint SubgridEndY);

      if (SubgridStartY == SubgridEndY)
      {
        SingleRowOnly = true;
      }

      // OverrideSubGridY is used to ensure that each 'row' of subgrids is adjacent to the
      // previous row to ensure a row of subgrids is not skipped in the event that
      // H1 and H2 vertices lie on the boundary of two subgrids which may cause numeric
      // imprecision when the H1 and H2 vertices are updated after scanning across the
      // subgrids in the row.
      uint OverrideSubGridY = SubgridStartY;

      // Repeatedly scan over rows of subgrids that cover the triangle piece checking
      // if they cover the body of the triangle
      do
      {
        index.CalculateIndexOfCellContainingPosition(H1X, H1Y, out uint LeftSubGridX, out _);
        index.CalculateIndexOfCellContainingPosition(H2X, H2Y, out uint RightSubGridX, out _);

        if (LeftSubGridX > RightSubGridX)
          MinMax.Swap(ref LeftSubGridX, ref RightSubGridX);

        // Bracket the calculate left and right subgrid indices with the previous left and
        // right subgrid indices to ensure subgrids included via shallow grazing
        // of near horizontal triangle edges are taken into consideration as each
        // subsequent row of subgrids is scanned.

        uint TestLeftSubGridX = FirstRow ? LeftSubGridX : (PrevLeftSubGridX < LeftSubGridX) ? PrevLeftSubGridX : LeftSubGridX;
        uint TestRightSubGridX = FirstRow ? RightSubGridX : (PrevRightSubGridX > RightSubGridX) ? PrevRightSubGridX : RightSubGridX;

        // Scan 'central' portion of subgrids between the two end points
        for (uint I = TestLeftSubGridX; I <= TestRightSubGridX; I++)
        {
          if (!leafSatisfied(index, I, OverrideSubGridY))
          {
            index.GetCellExtents(I, OverrideSubGridY, ref AddTrianglePieceToSubgridIndex_Extents);

            if (DesignGeometry.SubGridIntersectsTriangle(AddTrianglePieceToSubgridIndex_Extents, H1, H2, V))
              includeTriangleInLeaf(index, I, OverrideSubGridY, sourceTriangle);
          }
        }

        // Scan to the left from the left most point until subgrids no longer intersect the triangle
        uint SubGridX = TestLeftSubGridX;
        do
        {
          SubGridX--;

          index.GetCellExtents(SubGridX, OverrideSubGridY, ref AddTrianglePieceToSubgridIndex_Extents);

          if (!DesignGeometry.SubGridIntersectsTriangle(AddTrianglePieceToSubgridIndex_Extents, H1, H2, V))
            break;

          if (!leafSatisfied(index, SubGridX, OverrideSubGridY))
            includeTriangleInLeaf(index, SubGridX, OverrideSubGridY, sourceTriangle);
        } while (true);

        // Scan to the right from the right most point until subgrids no longer intersect the triangle
        SubGridX = TestRightSubGridX;
        do
        {
          SubGridX++;

          index.GetCellExtents(SubGridX, OverrideSubGridY, ref AddTrianglePieceToSubgridIndex_Extents);

          if (!DesignGeometry.SubGridIntersectsTriangle(AddTrianglePieceToSubgridIndex_Extents, H1, H2, V))
            break;

          if (!leafSatisfied(index, SubGridX, OverrideSubGridY))
            includeTriangleInLeaf(index, SubGridX, OverrideSubGridY, sourceTriangle);
        } while (true);

        FirstRow = false;

        H1X += H1SlopeTimesCellSize;
        H1Y += YStepTimesCellSize;

        H2X -= H2SlopeTimesCellSize;
        H2Y += YStepTimesCellSize;

        PrevLeftSubGridX = LeftSubGridX;
        PrevRightSubGridX = RightSubGridX;

        OverrideSubGridY = (uint)(OverrideSubGridY + YStep);

        WasLastRow = LastRow;
        LastRow = OverrideSubGridY == SubgridEndY; // H2X < H1X;
      } while (!WasLastRow && !SingleRowOnly);
    }

  }
}

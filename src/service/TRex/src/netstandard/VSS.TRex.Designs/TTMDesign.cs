using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.TTM.Optimised;
using VSS.TRex.Designs.TTM.Optimised.Profiling.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using Consts = VSS.TRex.Designs.TTM.Optimised.Consts;
using SubGridUtilities = VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities;

namespace VSS.TRex.Designs
{
  /// <summary>
  /// A design comprised of a Triangulated Irregular Network TIN surface, consumed from a Trimble TIN Model file
  /// </summary>
  public class TTMDesign : DesignBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    private double FMinHeight;
    private double FMaxHeight;
    private readonly double FCellSize;
    private readonly ISubGridTreeBitMask FSubgridIndex;

    public TrimbleTINModel Data { get; }

    private Triangle[] TriangleItems;
    private XYZ[] VertexItems;

    public long NumTINProbeLookups = 0;
    public long NumTINHeightRequests = 0;
    public long NumNonNullProbeResults = 0;

    private struct TriangleSubGridCellExtents
    {
      public byte MinX, MinY, MaxX, MaxY;
    }

    private int[] SpatialIndexOptimisedTriangles;

    public OptimisedSpatialIndexSubGridTree SpatialIndexOptimised { get; private set; }

    private void AddTrianglePieceToElevationPatch(XYZ H1, XYZ H2, XYZ V,
      Triangle Tri,
      bool SingleRowOnly,
      double OriginX, double OriginY,
      double CellSize,
      float[,] Patch,
      double OffSet,
      ref int ValueCount)
    {
      double H1Slope, H2Slope;
      int Delta;

      // H1 and H2 describe the horizontal portion of the triangle piece
      // V describes the vertex above, or below the horizontal line

      // Ensure H1 is left of H2 and take local copies of the vertex ordinates
      if (H1.X > H2.X)
        DesignGeometry.SwapVertices(ref H1, ref H2);

      double H1X = H1.X;
      double H1Y = H1.Y;
      double H2X = H2.X;
      double H2Y = H2.Y;
      double VX = V.X;
      double VY = V.Y;

      // HalfMinorCellSize is half of the cell size of the on-the-ground cells that
      // will be compared against the TIN design surface during cut fill operations.
      // As the sample point for a cell is the center point of the cell then there is
      // no need to include a half cell width outer boundary of each cell in the subgrid
      // index. A small epsilon value is deducted from the half cell size value to prevent
      // numeric imprecision
      double HalfCellSize = CellSize / 2;
      double HalfMinorCellSize = HalfCellSize - 0.001;

      double PatchSize = SubGridTreeConsts.SubGridTreeDimension * CellSize;
      double TopEdge = OriginY + PatchSize;
      double RightEdge = OriginX + PatchSize;

      double OriginXPlusHalfCell = OriginX + HalfMinorCellSize;
      double OriginYPlusHalfCell = OriginY + HalfMinorCellSize;

      double TopEdgeLessHalfCell = TopEdge - HalfMinorCellSize;
      double RightEdgeLessHalfCell = RightEdge - HalfMinorCellSize;

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

      int PatchOriginCellIndexX = (int) Math.Floor(OriginXPlusHalfCell / CellSize);
      int PatchOriginCellIndexY = (int) Math.Floor(OriginYPlusHalfCell / CellSize);
      int PatchCellLimitIndexX = PatchOriginCellIndexX + SubGridTreeConsts.SubGridTreeDimension - 1;

      // Work out 'Y' range and step direction of the triangle piece.
      double YRange = VY - H1Y;
      int YStep = Math.Sign(YRange);

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

      double H1SlopeTimesCellSize = H1Slope * CellSize;
      double H2SlopeTimesCellSize = H2Slope * CellSize;

      double AbsH1SlopeTimesCellSize = Math.Abs(H1SlopeTimesCellSize) + 0.001;
      double AbsH2SlopeTimesCellSize = Math.Abs(H2SlopeTimesCellSize) + 0.001;

      // ProcessingCellYIndex is used to ensure that each 'row' of cells is adjacent to the
      // previous row to ensure a row of cells is not skipped in the event that
      // H1 and H2 vertices lie on the boundary of two cells which may cause numeric
      // imprecision when the H1 and H2 vertices are updated after scanning across the
      // cells in the row.

      int VCellIndexY = (int) Math.Floor(VY / CellSize);
      int HCellIndexY = (int) Math.Floor(H1Y / CellSize);

      int VCellPatchIndex = VCellIndexY - PatchOriginCellIndexY;
      int HCellPatchIndex = HCellIndexY - PatchOriginCellIndexY;

      int NumCellRowsToProcess = Math.Abs(VCellPatchIndex - HCellPatchIndex) + 1;

      int ProcessingCellYIndex = HCellPatchIndex;

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
        if (HCellPatchIndex >= SubGridTreeConsts.SubGridTreeDimension)
        {
          if (YStep == 1) // There's nothing more to be done here
          {
            return;
          }

          Delta = (HCellPatchIndex - SubGridTreeConsts.SubGridTreeDimension) + 1;
          H1X = H1X + Delta * H1SlopeTimesCellSize;
          H2X = H2X - Delta * H2SlopeTimesCellSize;

          NumCellRowsToProcess -= Delta;
          ProcessingCellYIndex = SubGridTreeConsts.SubGridTreeDimension - 1;
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
      else if (VCellPatchIndex >= SubGridTreeConsts.SubGridTreeDimension)
      {
        if (YStep == -1)
        {
          return; // Nothing more to do here
        }

        NumCellRowsToProcess -= ((VCellPatchIndex - SubGridTreeConsts.SubGridTreeDimension) + 1);
      }

      if (NumCellRowsToProcess == 0)
      {
        return;
      }

      // Widen the H1/H2 spread to adequately cover the cells in the interval
      // as iterating across just this interval will leave cells on the extreme
      // edges missed out from the spot elevation calculations

      H1X -= AbsH1SlopeTimesCellSize;
      H2X += AbsH2SlopeTimesCellSize;

      // Note: H1X & H2X are modified in the loop after this location

      // Repeatedly scan over rows of cells that cover the triangle piece checking
      // if they cover the body of the triangle
      do //repeat
      {
        // Calculate the positions of the left and right cell indices in the coordinate space of the
        // triangle piece
        int LeftCellIndexX = (int) Math.Floor(H1X / CellSize);
        int RightCellIndexX = (int) Math.Floor(H2X / CellSize) + 1;

        // Clip the calculated cell indices against the coordinate space of the patch
        if (LeftCellIndexX < PatchOriginCellIndexX)
          LeftCellIndexX = PatchOriginCellIndexX;
        if (RightCellIndexX > PatchCellLimitIndexX)
          RightCellIndexX = PatchCellLimitIndexX;

        if (LeftCellIndexX <= RightCellIndexX)
        {
          double Y = ((PatchOriginCellIndexY + ProcessingCellYIndex) * CellSize) + HalfCellSize;

          for (int I = LeftCellIndexX; I < RightCellIndexX; I++)
          {
            double Z = GetHeight(Tri, I * CellSize + HalfCellSize, Y);

            if (Z != Common.Consts.NullReal)
            {
              if (Patch[I - PatchOriginCellIndexX, ProcessingCellYIndex] == Common.Consts.NullHeight)
              {
                ValueCount++;
                Patch[I - PatchOriginCellIndexX, ProcessingCellYIndex] = (float) (Z + OffSet);
              }
            }
          }
        }

        // Recalculate the left and right cell indexers for the next row of cells to be scanned across the triangle.
        H1X += H1SlopeTimesCellSize;
        H2X -= H2SlopeTimesCellSize;

        NumCellRowsToProcess--;
        ProcessingCellYIndex += YStep;

        // if (NumCellRowsToProcess > 0) and not InRange(ProcessingCellYIndex, 0, kSubGridTreeDimension - 1) then
        //   SIGLogMessage.PublishNoODS(Self, Format('ProcessingCellYIndex (%d) out of range', [ProcessingCellYIndex]), ...);
      } while ((NumCellRowsToProcess > 0) && !SingleRowOnly); // or not InRange(ProcessingCellYIndex, 0, kSubGridTreeDimension - 1);
    }
  
    public override bool ComputeFilterPatch(double StartStn, double EndStn, double LeftOffset, double RightOffset,
      SubGridTreeBitmapSubGridBits Mask,
      SubGridTreeBitmapSubGridBits Patch,
      double OriginX, double OriginY,
      double CellSize,
      double Offset)
    {
      float[,] Heights = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

      if (InterpolateHeights(Heights, OriginX, OriginY, CellSize, Offset))
      {
        Mask.ForEachSetBit((x, y) =>
        {
          if (Heights[x, y] == Common.Consts.NullHeight) Mask.ClearBit(x, y);
        });
        Patch.Assign(Mask);

        //SIGLogMessage.PublishNoODS(Self, Format('Filter patch construction successful with %d bits', [Patch.CountBits]), ...);

        return true;
      }

      //SIGLogMessage.PublishNoODS(Self, Format('Filter patch construction failed...', []), ...);
      return false;
    }

    /// <summary>
    /// Constructs the Subgrid existence map for the design
    /// </summary>
    /// <returns></returns>
    private bool ConstructSubgridIndex()
    {
      // Read through all the triangles in the model and, for each triangle,
      // determine which subgrids intersect it and set the appropriate bits in the
      // subgrid index.
      try
      {
        Log.LogInformation($"In: Constructing subgrid index for design containing {Data.Triangles.Items.Length} triangles");

        try
        {
          var cellScanner = new TriangleCellScanner(Data);

          int triangleCount = Data.Triangles.Items.Length;
          for (int triIndex = 0; triIndex < triangleCount; triIndex++)
          {
            cellScanner.ScanCellsOverTriangle(FSubgridIndex,
              triIndex,
              (tree, x, y) => ((ISubGridTreeBitMask)tree)[x, y],
              (tree, x, y, t) => ((ISubGridTreeBitMask)tree)[x, y] = true,
              cellScanner.AddTrianglePieceToSubgridIndex
            );
          }
        }
        finally
        {
          Log.LogInformation($"Out: Constructing subgrid index for design containing {Data.Triangles.Items.Length} triangles");
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError($"Exception in TTMDesign.ConstructSubgridIndex: {e}");
        return false;
      }
    }

    /// <summary>
    /// Constructor for a TTMDesign that takes the underlying cell size for the site model that will be used when interpolating heights from the design surface
    /// </summary>
    /// <param name="ACellSize"></param>
    public TTMDesign(double ACellSize)
    {
      Data = new TrimbleTINModel();
      TriangleItems = Data.Triangles.Items;
      VertexItems = Data.Vertices.Items;

      FCellSize = ACellSize;

      // Create a subgrid tree bit mask index that holds one bit per on-the-ground
      // subgrid that intersects at least one triangle in the TTM.
      FSubgridIndex = new SubGridTreeSubGridExistenceBitMask
      {
        CellSize = SubGridTreeConsts.SubGridTreeDimension * ACellSize
      };

      // Create the optimized subgrid tree spatial index that minimizes the number of allocations in the final result.
      SpatialIndexOptimised = new OptimisedSpatialIndexSubGridTree(SubGridTreeConsts.SubGridTreeLevels - 1, SubGridTreeConsts.SubGridTreeDimension * ACellSize);
    }

    /// <summary>
    /// Retrieves the ground extents of the TTM design 
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    public override void GetExtents(out double x1, out double y1, out double x2, out double y2)
    {
      x1 = Data.Header.MinimumEasting;
      y1 = Data.Header.MinimumNorthing;
      x2 = Data.Header.MaximumEasting;
      y2 = Data.Header.MaximumNorthing;
    }

    /// <summary>
    /// Retrieves the elevation range of the vertices in the TTm design surface
    /// </summary>
    /// <param name="z1"></param>
    /// <param name="z2"></param>
    public override void GetHeightRange(out double z1, out double z2)
    {
      if (FMinHeight == Common.Consts.NullReal || FMaxHeight == Common.Consts.NullReal) // better calculate them
      {
        FMinHeight = 1E100;
        FMaxHeight = -1E100;

        foreach (var vertex in VertexItems)
        {
          if (vertex.Z < FMinHeight) FMinHeight = vertex.Z;
          if (vertex.Z > FMaxHeight) FMaxHeight = vertex.Z;
        }
      }

      z1 = FMinHeight;
      z2 = FMaxHeight;
    }

    public override bool HasElevationDataForSubGridPatch(double X, double Y)
    {
      FSubgridIndex.CalculateIndexOfCellContainingPosition(X, Y, out uint SubgridX, out uint SubgridY);
      return FSubgridIndex[SubgridX, SubgridY];
    }

    public override bool HasElevationDataForSubGridPatch(uint SubGridX, uint SubGridY) => FSubgridIndex[SubGridX, SubGridY];

    public override bool HasFiltrationDataForSubGridPatch(double X, double Y) => false;

    public override bool HasFiltrationDataForSubGridPatch(uint SubGridX, uint SubgridY) => false;

    private double GetHeight(Triangle tri, double X, double Y)
    {
      return XYZ.GetTriangleHeight(VertexItems[tri.Vertex0], VertexItems[tri.Vertex1], VertexItems[tri.Vertex2], X, Y);
    }

    private double GetHeight2(ref Triangle tri, double X, double Y)
    {
      return XYZ.GetTriangleHeightEx(ref VertexItems[tri.Vertex0], ref VertexItems[tri.Vertex1], ref VertexItems[tri.Vertex2], X, Y);
    }

    /// <summary>
    /// Interpolates a single spot height from the design, using the optimized spatial index
    /// </summary>
    /// <param name="Hint"></param>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="Offset"></param>
    /// <param name="Z"></param>
    /// <returns></returns>
    public override bool InterpolateHeight(ref int Hint,
      double X, double Y,
      double Offset,
      out double Z)
    {
      if (Hint != -1)
      {
        Z = GetHeight(TriangleItems[Hint], X, Y);
        if (Z != Common.Consts.NullDouble)
        {
          Z += Offset;
          return true;
        }

        Hint = -1;
      }

      // Search in the subgrid triangle list for this subgrid from the spatial index

      SpatialIndexOptimised.CalculateIndexOfCellContainingPosition(X, Y, out uint CellX, out uint CellY);

      TriangleArrayReference arrayReference = SpatialIndexOptimised[CellX, CellY];

      if (arrayReference.Count == 0)
      {
        // There are no triangles that can satisfy the query
        Z = Common.Consts.NullReal;
        return false;
      }

      // Search the triangles in the leaf to locate the one to interpolate height from
      int limit = arrayReference.TriangleArrayIndex + arrayReference.Count;
      for (int i = arrayReference.TriangleArrayIndex; i < limit; i++)
      {
        int triIndex = SpatialIndexOptimisedTriangles[i];
        Z = GetHeight(TriangleItems[triIndex], X, Y);

        if (Z != Common.Consts.NullReal)
        {
          Hint = triIndex;
          Z += Offset;
          return true;
        }
      }

      Z = Common.Consts.NullReal;
      return false;
    }

    private static readonly float[,] kNullPatch = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

    static TTMDesign()
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) => kNullPatch[x, y] = Common.Consts.NullHeight);
    }

    /// <summary>
    /// Interpolates heights from the design for all the cells in a subgrid
    /// </summary>
    /// <param name="Patch"></param>
    /// <param name="OriginX"></param>
    /// <param name="OriginY"></param>
    /// <param name="CellSize"></param>
    /// <param name="Offset"></param>
    /// <returns></returns>
    public override bool InterpolateHeights(float[,] Patch, double OriginX, double OriginY, double CellSize, double Offset)
    {
      bool hasValues = false;
      TriangleSubGridCellExtents triangleCellExtent = new TriangleSubGridCellExtents();

      try
      {
        double HalfCellSize = CellSize / 2;
        double halfCellSizeMinusEpsilon = HalfCellSize - 0.0001;
        double OriginXPlusHalfCellSize = OriginX + HalfCellSize;
        double OriginYPlusHalfCellSize = OriginY + HalfCellSize;

        // Search in the subgrid triangle list for this subgrid from the spatial index
        // All cells in this subgrid will be contained in the same triangle list from the spatial index
        SpatialIndexOptimised.CalculateIndexOfCellContainingPosition(OriginXPlusHalfCellSize, OriginYPlusHalfCellSize, out uint CellX, out uint CellY);
        TriangleArrayReference arrayReference = SpatialIndexOptimised[CellX, CellY];
        int triangleCount = arrayReference.Count;

        if (triangleCount == 0) // There are no triangles that can satisfy the query (leaf cell is empty)
          return false;

        double leafCellSize = SpatialIndexOptimised.CellSize / SubGridTreeConsts.SubGridTreeDimension;
        BoundingWorldExtent3D cellWorldExtent = SpatialIndexOptimised.GetCellExtents(CellX, CellY);

        // Create the array of triangle cell extents in the subgrid
        TriangleSubGridCellExtents[] triangleCellExtents = new TriangleSubGridCellExtents[triangleCount];

        // Compute the bounding structs for the triangles in this subgrid
        for (int i = 0; i < triangleCount; i++)
        {
          // Get the triangle...
          Triangle tri = TriangleItems[SpatialIndexOptimisedTriangles[arrayReference.TriangleArrayIndex + i]];

          // Get the real world bounding box for the triangle
          // Note: As sampling occurs at cell centers shrink the effective bounding box for each triangle used
          // for calculating the cell bounding box by half a cell size (less a small Epsilon) so the cell bounding box
          // captures cell centers falling in the triangle world coordinate bounding box

          XYZ TriVertex0 = VertexItems[tri.Vertex0];
          XYZ TriVertex1 = VertexItems[tri.Vertex1];
          XYZ TriVertex2 = VertexItems[tri.Vertex2];

          double TriangleWorldExtent_MinX = Math.Min(TriVertex0.X, Math.Min(TriVertex1.X, TriVertex2.X)) + halfCellSizeMinusEpsilon;
          double TriangleWorldExtent_MinY = Math.Min(TriVertex0.Y, Math.Min(TriVertex1.Y, TriVertex2.Y)) + halfCellSizeMinusEpsilon;
          double TriangleWorldExtent_MaxX = Math.Max(TriVertex0.X, Math.Max(TriVertex1.X, TriVertex2.X)) - halfCellSizeMinusEpsilon;
          double TriangleWorldExtent_MaxY = Math.Max(TriVertex0.Y, Math.Max(TriVertex1.Y, TriVertex2.Y)) - halfCellSizeMinusEpsilon;

          int minCellX = (int) Math.Floor((TriangleWorldExtent_MinX - cellWorldExtent.MinX) / leafCellSize);
          int minCellY = (int) Math.Floor((TriangleWorldExtent_MinY - cellWorldExtent.MinY) / leafCellSize);
          int maxCellX = (int) Math.Floor((TriangleWorldExtent_MaxX - cellWorldExtent.MinX) / leafCellSize);
          int maxCellY = (int) Math.Floor((TriangleWorldExtent_MaxY - cellWorldExtent.MinY) / leafCellSize);

          triangleCellExtent.MinX = (byte) (minCellX <= 0 ? 0 : minCellX >= SubGridTreeConsts.SubGridTreeDimensionMinus1 ? SubGridTreeConsts.SubGridTreeDimensionMinus1 : minCellX);
          triangleCellExtent.MinY = (byte) (minCellY <= 0 ? 0 : minCellY >= SubGridTreeConsts.SubGridTreeDimensionMinus1 ? SubGridTreeConsts.SubGridTreeDimensionMinus1 : minCellY);
          triangleCellExtent.MaxX = (byte) (maxCellX <= 0 ? 0 : maxCellX >= SubGridTreeConsts.SubGridTreeDimensionMinus1 ? SubGridTreeConsts.SubGridTreeDimensionMinus1 : maxCellX);
          triangleCellExtent.MaxY = (byte) (maxCellY <= 0 ? 0 : maxCellY >= SubGridTreeConsts.SubGridTreeDimensionMinus1 ? SubGridTreeConsts.SubGridTreeDimensionMinus1 : maxCellY);

          triangleCellExtents[i] = triangleCellExtent;
        }

        // Initialise Patch to null height values
        Array.Copy(kNullPatch, 0, Patch, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid);

        // Iterate over all the cells in the grid using the triangle subgrid cell extents to filter
        // triangles in the leaf that will be considered for point-in-triangle & elevation checks.

        double X = OriginXPlusHalfCellSize;
        for (int x = 0; x < SubGridTreeConsts.SubGridTreeDimension; x++)
        {
          double Y = OriginYPlusHalfCellSize;
          for (int y = 0; y < SubGridTreeConsts.SubGridTreeDimension; y++)
          {
            // Search the triangles in the leaf to locate the one to interpolate height from
            for (int i = 0; i < triangleCount; i++)
            {
              //NumTINProbeLookups++;

              if (x < triangleCellExtents[i].MinX || x > triangleCellExtents[i].MaxX || y < triangleCellExtents[i].MinY || y > triangleCellExtents[i].MaxY)
                continue; // No intersection, move to next triangle

              //NumTINHeightRequests++;

              double Z = GetHeight2(ref TriangleItems[SpatialIndexOptimisedTriangles[arrayReference.TriangleArrayIndex + i]], X, Y);

              if (Z != Common.Consts.NullReal)
              {
                //NumNonNullProbeResults++;

                hasValues = true;
                Patch[x, y] = (float) (Z + Offset);

                break; // No more triangles need to be examined for this cell
              }
            }

            Y += CellSize;
          }

          X += CellSize;
        }

        return hasValues;
      }
      catch (Exception e)
      {
        Log.LogError($"Exception {e} occurred in TTMDesign.InterpolateHeights");
        return false;
      }
    }

    /// <summary>
    /// Loads the TTM design file/s, from storage
    /// Includes design file and 2 index files (if they exist)
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="fileName"></param>
    /// <param name="localPath"></param>
    /// <param name="loadIndices"></param>
    /// <returns></returns>
    public override DesignLoadResult LoadFromStorage(Guid siteModelUid, string fileName, string localPath, bool loadIndices = false)
    {
      var isDownloaded = S3FileTransfer.ReadFile(siteModelUid, fileName, localPath).Result;
      if (!isDownloaded)
      {
        return DesignLoadResult.UnknownFailure;
      }

      if (loadIndices)
      {
        isDownloaded = S3FileTransfer.ReadFile(siteModelUid, (fileName + Consts.kDesignSubgridIndexFileExt), TRexServerConfig.PersistentCacheStoreLocation).Result;
        if (!isDownloaded)
        {
          return DesignLoadResult.UnableToLoadSubgridIndex;
        }

        isDownloaded = S3FileTransfer.ReadFile(siteModelUid, (fileName + Consts.kDesignSpatialIndexFileExt), TRexServerConfig.PersistentCacheStoreLocation).Result;
        if (!isDownloaded)
        {
          return DesignLoadResult.UnableToLoadSpatialIndex;
        }
      }

      return DesignLoadResult.Success;
    }

    /// <summary>
    /// Loads the TTM design from a TTM file, along with the subgrid existence map file if it exists (created otherwise)
    /// </summary>
    /// <param name="localPathAndFileName"></param>
    /// <returns></returns>
    public override DesignLoadResult LoadFromFile(string localPathAndFileName)
    {
      try
      {
        Data.LoadFromFile(localPathAndFileName);
        TriangleItems = Data.Triangles.Items;
        VertexItems = Data.Vertices.Items;

        Log.LogInformation($"Loaded TTM file {localPathAndFileName} containing {Data.Header.NumberOfTriangles} triangles and {Data.Header.NumberOfVertices} vertices.");

        FMinHeight = Common.Consts.NullReal;
        FMaxHeight = Common.Consts.NullReal;

        if (!LoadSubgridIndexFile(localPathAndFileName + Consts.kDesignSubgridIndexFileExt))
          return DesignLoadResult.UnableToLoadSubgridIndex;

        if (!LoadSpatialIndexFile(localPathAndFileName + Consts.kDesignSpatialIndexFileExt))
          return DesignLoadResult.UnableToLoadSubgridIndex;

        Log.LogInformation(
          $"Area: ({Data.Header.MinimumEasting}, {Data.Header.MinimumNorthing}) -> ({Data.Header.MaximumEasting}, {Data.Header.MaximumNorthing}): [{Data.Header.MaximumEasting - Data.Header.MinimumEasting} x {Data.Header.MaximumNorthing - Data.Header.MinimumNorthing}]");

        return DesignLoadResult.Success;
      }
      catch (Exception E)
      {
        Log.LogError($"Exception {E} in LoadFromFile");
        return DesignLoadResult.UnknownFailure;
      }
    }

    /// <summary>
    /// Loads the subgrid existence map from a file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private bool LoadSubgridIndex(string fileName)
    {
      try
      {
        if (!File.Exists(fileName))
          return false;

        using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(fileName)))
        {
          FSubgridIndex.FromStream(ms);
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError($"Exception {e} in LoadSubgridIndex");

        return false;
      }
    }
    
    /// <summary>
    /// Loads the subgrid existence map from a file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private bool LoadSpatialIndex(string fileName)
    {
      try
      {
        if (!File.Exists(fileName))
          return false;

        byte[] bytes = File.ReadAllBytes(fileName);

        using (MemoryStream ms = new MemoryStream(bytes))
        {
          using (BinaryReader reader = new BinaryReader(ms))
          {
            byte majorVer = reader.ReadByte();
            byte minorVer = reader.ReadByte();

            if (majorVer != 1 || minorVer != 0)
              return false;

            // Load the array of triangle references
            long numTriangles = reader.ReadInt64();
            SpatialIndexOptimisedTriangles = new int[numTriangles];
            int bufPos = (int)ms.Position;
            for (int i = 0; i < numTriangles; i++)
            {
              // Binary reader version, replaced by faster version below
              // SpatialIndexOptimizedTriangles[i] = reader.ReadInt32();

              // The much faster direct version
              SpatialIndexOptimisedTriangles[i] = bytes[bufPos] | bytes[bufPos + 1] << 8 | bytes[bufPos + 2] << 16 | bytes[bufPos + 3] << 24;
              bufPos += 4;
            }

            // Reset stream position to start of serialized sub grid tree.
            ms.Position = bufPos;

            // Load the tree of references into the optimized triangle reference list
            SpatialIndexOptimised.FromStream(ms);

            return true;
          }
        }
      }
      catch (Exception e)
      {
        Log.LogError($"Exception {e} in LoadSubgridIndex");

        return false;
      }
    }

    /// <summary>
    /// Loads a subgrid existence map for the design from a file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private bool LoadSubgridIndexFile(string fileName)
    {
      Log.LogInformation($"Loading subgrid index file {fileName}");

      bool Result = LoadSubgridIndex(fileName);

      if (!Result)
      {
        Result = ConstructSubgridIndex();

        if (Result)
        {
          if (SaveSubgridIndex(fileName))
            Log.LogInformation($"Saved constructed subgrid index file {fileName}");
          else
            Log.LogError($"Unable to save subgrid index file {fileName} - continuing with unsaved index");
        }
        else
          Log.LogError($"Unable to create and save subgrid index file {fileName}");
      }

      return Result;
    }

    /// <summary>
    /// Loads a subgrid spatial index for the design from a file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private bool LoadSpatialIndexFile(string fileName)
    {
      Log.LogInformation($"Loading spatial index file {fileName}");

      bool Result = LoadSpatialIndex(fileName);

      if (!Result)
      {
        // Build the subgrid tree based spatial index
        var indexBuilder = new OptimisedTTMSpatialIndexBuilder(Data, FCellSize);
        Result = indexBuilder.ConstructSpatialIndex();

        if (Result)
        {
          SpatialIndexOptimised = indexBuilder.SpatialIndexOptimised;
          SpatialIndexOptimisedTriangles = indexBuilder.SpatialIndexOptimisedTriangles;

          if (SaveSpatialIndex(fileName))
            Log.LogInformation($"Saved constructed spatial index file {fileName}");
          else
            Log.LogError($"Unable to save spatial index file {fileName} - continuing with unsaved index");
        }
        else
          Log.LogError($"Unable to create and save spatial index file {fileName}");
      }

      return Result;
    }

    /// <summary>
    /// Saves a subgrid existence map for the design to a file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private bool SaveSubgridIndex(string fileName)
    {
      try
      {
        // Write the index out to a file
        using (var fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
          FSubgridIndex.ToStream(fs);
        }

        if (!File.Exists(fileName))
        {
          Thread.Sleep(500); // Seems to be a Windows update problem hence introduce delay b4 checking again
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError($"Exception {e} SaveSubgridIndex");
      }

      return false;
    }

    /// <summary>
    /// Saves a subgrid existence map for the design to a file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private bool SaveSpatialIndex(string fileName)
    {
      try
      {
        // Write the index out to a file
        using (var fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
          using (var writer = new BinaryWriter(fs, Encoding.UTF8, true))
          {
            writer.Write((byte) 1); // Major version
            writer.Write((byte) 0); // Minor version

            // write out the array of triangle references
            writer.Write((long) SpatialIndexOptimisedTriangles.Length);
            foreach (int triIndex in SpatialIndexOptimisedTriangles)
              writer.Write(triIndex);
          }

          // Write the body of the subgrid tree containing references into the list of triangles
          SpatialIndexOptimised.ToStream(fs);
        }

        if (!File.Exists(fileName))
        {
          Thread.Sleep(500); // Seems to be a Windows update problem hence introduce delay b4 checking again
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError($"Exception {e} SaveSubgridIndex");
      }

      return false;
    }

    /// <summary>
    /// A reference to the internal subgrid existence map for the design
    /// </summary>
    /// <returns></returns>
    public override ISubGridTreeBitMask SubgridOverlayIndex() => FSubgridIndex;

    /// <summary>
    /// Computes the requested geometric profile over the design and returns the result
    /// as a vector of X, Y, Z, Station & TriangleIndex records
    /// </summary>
    /// <param name="profilePath"></param>
    /// <param name="cellSize"></param>
    /// <returns></returns>
    public override XYZS[] ComputeProfile(XYZ[] profilePath, double cellSize)
    {
      var profiler = DIContext.Obtain<IOptimisedTTMProfilerFactory>();//NewInstance( new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      throw new NotImplementedException();
    }
  }
}

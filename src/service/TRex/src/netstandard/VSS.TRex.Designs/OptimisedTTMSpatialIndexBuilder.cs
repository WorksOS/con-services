using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.TTM.Optimised;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.Designs
{
  public class OptimisedTTMSpatialIndexBuilder
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<OptimisedTTMSpatialIndexBuilder>();

    public TrimbleTINModel TTM { get; }

    public double CellSize { get; }

    public OptimisedSpatialIndexSubGridTree SpatialIndexOptimised { get; private set; }

    private int[] spatialIndexOptimisedTriangles = null;

    public int[] SpatialIndexOptimisedTriangles => spatialIndexOptimisedTriangles;

    private readonly Triangle[] TriangleItems;
    private readonly XYZ[] VertexItems;

    /// <summary>
    /// Constructs a builder from a given TTM model in the optimized format and other information
    /// </summary>
    /// <param name="ttm"></param>
    /// <param name="cellSize"></param>
    public OptimisedTTMSpatialIndexBuilder(TrimbleTINModel ttm, double cellSize)
    {
      TTM = ttm;
      CellSize = cellSize;

      TriangleItems = ttm.Triangles.Items;
      VertexItems = ttm.Vertices.Items;
    }

    /// <summary>
    /// Includes a triangle into the list of triangles that intersect the extent of a subgrid
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="triIndex"></param>
    private void IncludeTriangleInSubGridTreeIndex(NonOptimisedSpatialIndexSubGridTree tree, uint x, uint y, int triIndex)
    {
      // Get subgrid from tree, creating the path and leaf if necessary
      var leaf = tree.ConstructPathToCell(x, y, SubGridPathConstructionType.CreateLeaf) as NonOptimisedSpatialIndexSubGridLeaf;

      leaf.GetSubGridCellIndex(x, y, out byte SubGridX, out byte SubGridY);

      // Get the list of triangles for the given cell
      List<int> triangles = leaf.Items[SubGridX, SubGridY];

      // If there are none already create the list and assign it to the cell
      if (triangles == null)
      {
        triangles = new List<int>();
        leaf.Items[SubGridX, SubGridY] = triangles;
        triangles.Add(triIndex);
      }
      else
      {
        // Add the triangle to the cell, even if it is already there (duplicates will be taken care of later)
        // Note: Duplicates tend to occur one after the other, so do a trivial last triangle duplicate check here
        if (triangles[triangles.Count - 1] != triIndex)
        {
          triangles.Add(triIndex);
        }
      }
    }

    /// <summary>
    /// Flag to enable detailed removal of duplicate triangle references in the subgrid spatial over and above the
    /// last-triangle-duplicate check in the logic constructing the initial lists of triangle references in each leaf.
    /// </summary>
    public bool EnableDuplicateRemoval = false;

    /// <summary>
    /// Build a spatial index for the triangles in the TIN surface by assigning each triangle to every subgrid it intersects with
    /// </summary>
    /// <returns></returns>
    public bool ConstructSpatialIndex()
    {
      // Read through all the triangles in the model and, for each triangle,
      // determine which subgrids in the index intersect it and add it to those subgrids
      try
      {
        // Create the optimized subgrid tree spatial index that minimizes the number of allocations in the final result.
        SpatialIndexOptimised = new OptimisedSpatialIndexSubGridTree(SubGridTreeConsts.SubGridTreeLevels - 1, SubGridTreeConsts.SubGridTreeDimension * CellSize);

        var FSpatialIndex = new NonOptimisedSpatialIndexSubGridTree(SubGridTreeConsts.SubGridTreeLevels - 1, SubGridTreeConsts.SubGridTreeDimension * CellSize);

        Log.LogInformation($"In: Constructing subgrid index for design containing {TTM.Triangles.Items.Length} triangles");
        try
        {
          var cellScanner = new TriangleCellScanner(TTM);

          // Construct a subgrid tree containing list of triangles that intersect each on-the-ground subgrid
          int triangleCount = TTM.Triangles.Items.Length;
          for (int triIndex = 0; triIndex < triangleCount; triIndex++)
          {
            cellScanner.ScanCellsOverTriangle(FSpatialIndex,
              triIndex,
              (tree, x, y) => false,
              (tree, x, y, t) => IncludeTriangleInSubGridTreeIndex(tree as NonOptimisedSpatialIndexSubGridTree, x, y, t),
              cellScanner.AddTrianglePieceToSubgridIndex);
          }


          if (EnableDuplicateRemoval)
          {
            /////////////////////////////////////////////////
            // Remove duplicate triangles added to the lists
            /////////////////////////////////////////////////
            BitArray uniques = new BitArray(TriangleItems.Length);
            long TotalDuplicates = 0;

            FSpatialIndex.ScanAllSubGrids(leaf =>
            {
              // Iterate across all cells in each (level 5) leaf subgrid. Each cell represents 
              // a subgrid in the level 6 subgrid representing cells sampled across the surface at the
              // core cell size for the project
              SubGridUtilities.SubGridDimensionalIterator((x, y) =>
              {
                List<int> triList = FSpatialIndex[leaf.OriginX + x, leaf.OriginY + y];

                if (triList == null)
                  return;

                uniques.SetAll(false);

                int triListCount = triList.Count;
                int uniqueCount = 0;
                for (int i = 0; i < triListCount; i++)
                {
                  int triIndex = triList[i];
                  if (!uniques[triIndex])
                  {
                    triList[uniqueCount++] = triIndex;
                    uniques[triIndex] = true;
                  }
                  else
                  {
                    TotalDuplicates++;
                  }
                }

                if (uniqueCount < triListCount)
                  triList.RemoveRange(uniqueCount, triListCount - uniqueCount);
              });

              return true;
            });

            Console.WriteLine($"Total duplicates encountered: {TotalDuplicates}");
          }

          // Transform this subgrid tree into one where each on-the-ground subgrid is represented by an index and a number of triangles present in a
          // a single list of triangles.

          // Count the number of triangle references present in the tree
          int numTriangleReferences = 0;
          FSpatialIndex.ForEach(x =>
          {
            numTriangleReferences += x?.Count ?? 0;
            return true;
          });

          // Create the single array
          spatialIndexOptimisedTriangles = new int[numTriangleReferences];

          /////////////////////////////////////////////////
          // Iterate across all leaf subgrids
          //Copy all triangle lists into it, and add the appropriate reference blocks in the new tree.
          /////////////////////////////////////////////////

          int copiedCount = 0;

          TriangleArrayReference arrayReference = new TriangleArrayReference()
          {
            Count = 0,
            TriangleArrayIndex = 0
          };

          BoundingWorldExtent3D cellWorldExtent = new BoundingWorldExtent3D();

          FSpatialIndex.ScanAllSubGrids(leaf =>
          {
            // Iterate across all cells in each (level 5) leaf subgrid. Each cell represents 
            // a subgrid in the level 6 subgrid representing cells sampled across the surface at the
            // core cell size for the project
            SubGridUtilities.SubGridDimensionalIterator((x, y) =>
            {
              uint CellX = leaf.OriginX + x;
              uint CellY = leaf.OriginY + y;

              List<int> triList = FSpatialIndex[CellX, CellY];

              if (triList == null)
                return;

              /////////////////////////////////////////////////////////////////////////////////////////////////
              // Start: Determine the triangles that definitely cannot cover one or more cells in each subgrid

              double leafCellSize = SpatialIndexOptimised.CellSize / SubGridTreeConsts.SubGridTreeDimension;
              double halfLeafCellSize = leafCellSize / 2;
              double halfCellSizeMinusEpsilon = halfLeafCellSize - 0.0001;

              short trianglesCopiedToLeaf = 0;

              SpatialIndexOptimised.GetCellExtents(CellX, CellY, ref cellWorldExtent);

              // Compute the bounding structs for the triangles in this subgrid and remove any triangles whose
              // bounding struct is null (ie: no cell centers are covered by its bounding box).

              for (int i = 0; i < triList.Count; i++)
              {
                // Get the triangle...
                Triangle tri = TriangleItems[triList[i]];

                // Get the real world bounding box for the triangle
                // Note: As sampling occurs at cell centers shrink the effective bounding box for each triangle used
                // for calculating the cell bounding box by half a cell size (less a small Epsilon) so the cell bounding box
                // captures cell centers falling in the triangle world coordinate bounding box

                XYZ Vertex0 = VertexItems[tri.Vertex0];
                XYZ Vertex1 = VertexItems[tri.Vertex1];
                XYZ Vertex2 = VertexItems[tri.Vertex2];

                double TriangleWorldExtent_MinX = Math.Min(Vertex0.X, Math.Min(Vertex1.X, Vertex2.X)) + halfCellSizeMinusEpsilon;
                double TriangleWorldExtent_MinY = Math.Min(Vertex0.Y, Math.Min(Vertex1.Y, Vertex2.Y)) + halfCellSizeMinusEpsilon;
                double TriangleWorldExtent_MaxX = Math.Max(Vertex0.X, Math.Max(Vertex1.X, Vertex2.X)) - halfCellSizeMinusEpsilon;
                double TriangleWorldExtent_MaxY = Math.Max(Vertex0.Y, Math.Max(Vertex1.Y, Vertex2.Y)) - halfCellSizeMinusEpsilon;

                // Calculate cell coordinates relative to the origin of the subgrid
                int minCellX = (int)Math.Floor((TriangleWorldExtent_MinX - cellWorldExtent.MinX) / leafCellSize);
                int minCellY = (int)Math.Floor((TriangleWorldExtent_MinY - cellWorldExtent.MinY) / leafCellSize);
                int maxCellX = (int)Math.Floor((TriangleWorldExtent_MaxX - cellWorldExtent.MinX) / leafCellSize);
                int maxCellY = (int)Math.Floor((TriangleWorldExtent_MaxY - cellWorldExtent.MinY) / leafCellSize);

                // Check if the result bounds are valid - if not, there is no point including it
                if (minCellX > maxCellX || minCellY > maxCellY)
                {
                  // There are no cell probe positions that can lie in this triangle, ignore it
                  continue;
                }

                // Check if there is an intersection between the triangle cell bounds and the leaf cell bounds
                if (minCellX > SubGridTreeConsts.SubGridTreeDimensionMinus1 || minCellY > SubGridTreeConsts.SubGridTreeDimensionMinus1 || maxCellX < 0 || maxCellY < 0)
                {
                  // There is no bounding box intersection, ignore it
                  continue;
                }

                // Transform the cell bounds by clamping them to the bounds of this subgrid
                minCellX = minCellX <= 0 ? 0 : minCellX >= SubGridTreeConsts.SubGridTreeDimensionMinus1 ? SubGridTreeConsts.SubGridTreeDimensionMinus1 : minCellX;
                minCellY = minCellY <= 0 ? 0 : minCellY >= SubGridTreeConsts.SubGridTreeDimensionMinus1 ? SubGridTreeConsts.SubGridTreeDimensionMinus1 : minCellY;
                maxCellX = maxCellX <= 0 ? 0 : maxCellX >= SubGridTreeConsts.SubGridTreeDimensionMinus1 ? SubGridTreeConsts.SubGridTreeDimensionMinus1 : maxCellX;
                maxCellY = maxCellY <= 0 ? 0 : maxCellY >= SubGridTreeConsts.SubGridTreeDimensionMinus1 ? SubGridTreeConsts.SubGridTreeDimensionMinus1 : maxCellY;

                // Check all the cells in the subgrid covered by this bounding box to check if at least one cell will actively probe this triangle

                bool found = false;
                double _x = cellWorldExtent.MinX + minCellX * leafCellSize + halfLeafCellSize;

                for (int cellX = minCellX; cellX <= maxCellX; cellX++)
                {
                  double _y = cellWorldExtent.MinY + minCellY * leafCellSize + halfLeafCellSize;
                  for (int cellY = minCellY; cellY <= maxCellY; cellY++)
                  {
                    if (XYZ.GetTriangleHeight(Vertex0, Vertex1, Vertex2, _x, _y) != Common.Consts.NullDouble)
                    {
                      found = true;
                      break;
                    }

                    _y += leafCellSize;
                  }

                  if (found)
                    break;

                  _x += leafCellSize;
                }

                if (!found)
                {
                  // No cell in the subgrid intersects with the triangle - ignore it
                  continue;
                }

                // This triangle is a candidate for being probed, copy it into the array
                trianglesCopiedToLeaf++;
                spatialIndexOptimisedTriangles[copiedCount++] = triList[i];
              }
              // End: Determine the triangles that definitely cannot cover one or more cells in each subgrid
              ///////////////////////////////////////////////////////////////////////////////////////////////

              arrayReference.Count = trianglesCopiedToLeaf;

              // Add new entry for optimized tree
              SpatialIndexOptimised[leaf.OriginX + x, leaf.OriginY + y] = arrayReference;

              // Set copied count into the array reference for the next leaf so it captures the starting location in the overall array for it
              arrayReference.TriangleArrayIndex = copiedCount;
            });

            return true;
          });

          Console.WriteLine($"Number of vertices in model {VertexItems.Length}");
          Console.WriteLine($"Number of triangles in model {TriangleItems.Length}");
          Console.WriteLine($"Number of original triangle references in index: {spatialIndexOptimisedTriangles.Length}");
          Console.WriteLine($"Number of triangle references removed as un-probable: {spatialIndexOptimisedTriangles.Length - copiedCount}");

          // Finally, resize the master triangle reference array to remove the unused entries due to un-probable triangles
          Array.Resize(ref spatialIndexOptimisedTriangles, copiedCount);

          Console.WriteLine($"Final number of triangle references in index: {spatialIndexOptimisedTriangles.Length}");
        }
        finally
        {
          // Emit some logging indicating likely efficiency of index.
          long sumTriangleReferences = 0;
          long sumTriangleLists = 0;
          long sumLeafSubGrids = 0;
          long sumNodeSubGrids = 0;

          FSpatialIndex.ScanAllSubGrids(l =>
          {
            sumLeafSubGrids++;
            return true;
          },
            n =>
            {
              sumNodeSubGrids++;
              return SubGridProcessNodeSubGridResult.OK;
            });

          FSpatialIndex.ForEach(x =>
          {
            sumTriangleLists++;
            sumTriangleReferences += x?.Count ?? 0;
            return true;
          });

          Log.LogInformation(
            $"Constructed subgrid index for design containing {TTM.Triangles.Items.Length} triangles, using {sumLeafSubGrids} leaf and {sumNodeSubGrids} node subgrids, {sumTriangleLists} triangle lists and {sumTriangleReferences} triangle references");
        }

        return true;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception in TTTMDesign.ConstructSpatialIndex");
        return false;
      }
    }
  }
}

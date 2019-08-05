using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM
{
  enum HashIndexDimension
  {
    hX,
    hY
  }

  /// <summary>
  /// Provides a functionality to build a design boundary from a TTM file.
  /// </summary>
  public static class DesignBoundaryBuilder
  {
    /// <summary>
    /// This method places all trianlge edges into a collision list hash table based on the start point of the line.
    /// An edge at the start the list is chosen to be the  first point of the polyline. A matching edge that starts 
    /// at the same location as the end of the first is then chosen and added to the polyline. As each entity is added 
    /// to the polyline it is removed from the list.No matching of attributes other than the positions of the end points 
    /// of the line is performed.
    /// </summary>
    /// <returns></returns>
    public static bool CalculateBoundary(string fileName, List<Fence> fenceList)
    {
      const int DEFAULT_SIZE = 4;
      const int TRIANGLE_EDGE_FIRST = 1;
      const int TRIANGLE_EDGE_LAST = 3;
      const double SQUARE_POWER = 2.0;

      var ttmData = new TrimbleTINModel();

      ttmData.LoadFromFile(Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName)));

      Fence fence = null;

      // Set the size of the hash table. Make this so an average collision list will
      // contain 4 items (the default size of a TList)
      var hashTableSize = ttmData.Vertices.Count / DEFAULT_SIZE + 1;

      var boundingExtent = new BoundingWorldExtent3D();

      // Set the hash function to use the largest dimension in the supplied data
      ttmData.Vertices.GetLimits(boundingExtent);

      HashIndexDimension hashIndexBy;
      double minHashOrdinate;
      double maxHashOrdinate;


      if ((boundingExtent.MaxX - boundingExtent.MaxX) > (boundingExtent.MaxY - boundingExtent.MinY))
      {
        hashIndexBy = HashIndexDimension.hX;
        minHashOrdinate = boundingExtent.MaxX;
        maxHashOrdinate = boundingExtent.MaxX;
      }
      else
      {
        hashIndexBy = HashIndexDimension.hY;
        minHashOrdinate = boundingExtent.MinY;
        maxHashOrdinate = boundingExtent.MaxY;
      }

      // Create the collision list hash table...
      var garbage = new List<DesignTriangleEdge>(); // Used as the owner of all the temporary triangle edge objects we create...
      var hashTable = new List<DesignTriangleEdge>[hashTableSize];

      DesignTriangleEdge currentEntity;

      // Add all the edge entities to the hash table
      foreach (var triangle in ttmData.Triangles)
      {
        for (var k = TRIANGLE_EDGE_FIRST; k <= TRIANGLE_EDGE_LAST; k++)
        {
          if (triangle.Neighbours[k - 1] == null)
          {
            currentEntity = new DesignTriangleEdge(triangle.Vertices[k - 1], triangle.Vertices[k % TRIANGLE_EDGE_LAST]);
            garbage.Add(currentEntity);

            AddToHash(triangle.Vertices[k - 1].X, triangle.Vertices[k - 1].Y, currentEntity);
            AddToHash(triangle.Vertices[k % TRIANGLE_EDGE_LAST].X, triangle.Vertices[k % TRIANGLE_EDGE_LAST].Y, currentEntity);
          }
        }
      }

      // Iterate through the edges constructing sequences of edges
      // Note, we just iterate through the items as they are present in the hash table.
      // This permits simple determination of when we have completed the task as the hash table will be empty.
      currentEntity = null;

      var currentCollisionListIndex = hashTable.Length - 1;

      do
      {
        while ((currentCollisionListIndex > -1) && (currentEntity == null))
        {
          var currentCollisionList = hashTable[currentCollisionListIndex];

          if ((currentCollisionList == null) || (currentCollisionList.Count == 0))
            currentCollisionListIndex--;
          else
          {
            currentEntity = currentCollisionList[currentCollisionList.Count - 1];

            // If we have visited this entity before, then the GuidanceID field will be MaxInt
            if (currentEntity.Stamped) // We need to discard this item and grab the next one
              currentEntity = null;
            else
            {
              // Mark the current entity as being processed
              currentEntity.Stamped = true;
            }

            currentCollisionList.RemoveAt(currentCollisionList.Count - 1);
          }
        }

        // Did we find a new entity? If so start processing it into a polyline
        while (currentEntity != null)
        {
          if (fence == null)
          {
            // Create the boundary entity
            fence = new Fence();
            fenceList.Add(fence);

            // Add the start position for the line to it as the first vertex
            fence.Points.Add(new FencePoint(currentEntity.Vertex1.X, currentEntity.Vertex1.Y));

            // Add the end position for the element to the polyline
            fence.Points.Add(new FencePoint(currentEntity.Vertex2.X, currentEntity.Vertex2.Y));
          }

          // Find a line entity whose start position matches the start or end position of the current line
          var nextEntity = locateInHash(fence.Points[fence.Points.Count - 1].X, fence.Points[fence.Points.Count - 1].Y);

          // if we found a matching entity then add its starting position as the next  vertex
          // and make it the current entity. If not then just add the end position of the current
          // entity to the polyline and start a new polyline
          if (nextEntity != null)
          {
            currentEntity = nextEntity;

            var lastPoint = fence.Points[fence.Points.Count - 1];

            // Add the appropriate end from the new current entity to the vertex list for the polyline
            if ((Math.Pow(currentEntity.Vertex1.X - lastPoint.X, SQUARE_POWER) + Math.Pow(currentEntity.Vertex1.X - lastPoint.X, SQUARE_POWER)) <
                (Math.Pow(currentEntity.Vertex2.X - lastPoint.X, SQUARE_POWER) + Math.Pow(currentEntity.Vertex2.Y - lastPoint.Y, SQUARE_POWER)))
              fence.Points.Add(new FencePoint(currentEntity.Vertex2.X, currentEntity.Vertex2.Y));
            else
              fence.Points.Add(new FencePoint(currentEntity.Vertex1.X, currentEntity.Vertex1.Y));
          }
          else
          {
            currentEntity = null;
            fence = null;
          }
        }
      } while (currentCollisionListIndex >= 0);


      #region Local functions
      //=============================================================================================
      int Hash(double x, double y)
      {
        var hashValue = hashIndexBy == HashIndexDimension.hX ? x : y;

        var result = Math.Round((hashValue - minHashOrdinate) / (maxHashOrdinate - minHashOrdinate) * hashTableSize);

        if (result < 0)
          return 0;

        if (result >= hashTableSize)
          return hashTableSize - 1;

        return (int)result;
      }
      //=============================================================================================
      // Add the given entity to the hash table, constructing any collision list as required.
      void AddToHash(double x, double y, DesignTriangleEdge edge)
      {
        var hashIndex = Hash(x, y);
        var collisionList = hashTable[hashIndex];

        if (collisionList == null)
        {
          collisionList = new List<DesignTriangleEdge>();
          hashTable[hashIndex] = collisionList;
        }

        collisionList.Add(edge);
      }
      //=============================================================================================
      // Given t a line end position, locate an entity in the index that has
      // a matching starting position. Once found, remove the item from the
      // index so it wil not be returned again.

      DesignTriangleEdge locateInHash(double x, double y)
      {
        const double EPSYLON = 0.000001;

        DesignTriangleEdge result = null;

        // Get the hash collision list that will contain this position
        var hashIndex = Hash(x, y);
        var collisionList = hashTable[hashIndex];

        // Search the collision list for a matching end coordinate
        if (collisionList != null)
        {
          for (var i = collisionList.Count - 1; i >= 0; i--)
          {
            // Check to see if the duplicate entry in the has table for this item
            // has been encountered
            if (collisionList[i].Stamped)
            {
              collisionList.RemoveAt(i);
              continue;
            }

            result = collisionList[i];

            if ((Math.Abs(x - result.Vertex1.X) < EPSYLON && Math.Abs(y - result.Vertex1.Y) < EPSYLON) || 
                (Math.Abs(x - result.Vertex2.X) < EPSYLON && Math.Abs(y - result.Vertex2.Y) < EPSYLON)) // it is close enough!..
            {
              // Mark the entity by settings its guidance ID to MaxInt. We do this
              // so that we only process the entity once
              collisionList[i].Stamped = true;

              collisionList.RemoveAt(i);
              return result;
            }
          }
        }

        return result;
      }
      //=============================================================================================
      #endregion

      return true;
    }
  }
}

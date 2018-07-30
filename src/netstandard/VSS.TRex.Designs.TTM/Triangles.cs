using System;
using System.Collections.Generic;

namespace VSS.TRex.Designs.TTM
{
    public class Triangles : List<Triangle>
    {
        //  private
        //    FCrossingTriangleQuery: TCrossingTriangleQuery;

        //  protected
        //    procedure FixCrossingTriangle(Triangle: TTriangle;
        //        MaxShortestSide: Double);
        //    function GetTriangleToRemove(Tri1, Tri2: TTriangle ): TTriangle; virtual;
        //    procedure ShrinkTriangleSideToPoint(Triangle: TTriangle; SideToShrink: TTriangleSide );
        //    procedure FlipTriangleDiagonal(Triangle: TTriangle; SideToFlip: TTriangleSide );
        //    procedure FixCrossingTriangles(MaxShortestSide: Double );
        //        procedure RemoveTriangleWithFewestNeighbours(Triangle1, Triangle2: TTriangle );

        public Triangles()
        {
        }

      /// <summary>
      /// The default delegate for creating triangles present in the triangles collection for the surface
      /// </summary>
        public Func<TriVertex, TriVertex, TriVertex, Triangle> CreateTriangleFunc { set; get; } = (v0, v1, v2) => new Triangle(v0, v1, v2);

        public Triangle CreateTriangle(TriVertex Vertex1, TriVertex Vertex2, TriVertex Vertex3) => CreateTriangleFunc(Vertex1, Vertex2, Vertex3);

        public Triangle AddTriangle(TriVertex Vertex1, TriVertex Vertex2, TriVertex Vertex3)
        {
            if ((Vertex1 == Vertex2) || (Vertex1 == Vertex3) || (Vertex2 == Vertex3))
            {
                return null;
            }

            Triangle Result = CreateTriangle(Vertex1, Vertex2, Vertex3);
            Add(Result);

            return Result;
        }

        public void RemoveTriangle(Triangle triangle)
        {
            // Disconnect neighbours
            for (int Side = 0; Side < 2; Side++)
            {
                if (triangle.Neighbours[Side] != null)
                {
                    for (int NbrSide = 0; Side < 2; Side++)
                    {
                        if (triangle == triangle.Neighbours[Side].Neighbours[NbrSide])
                        {
                            triangle.Neighbours[Side].Neighbours[NbrSide] = null;
                        }
                    }
                }
            }

            // Remove from list
            this[triangle.Tag - 1] = null;
        }

        //        property CrossingTriangleQuery: TCrossingTriangleQuery read FCrossingTriangleQuery write FCrossingTriangleQuery;

        public void NumberTriangles()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Tag = i + 1;
            }
        }

      /// <summary>
      /// Remove all null triangle references from the list.
      /// </summary>
      public void Pack()
      {
        int index_to = 0;

        for (int index_from = 0; index_from < Count; index_from++)
        {
          if (this[index_from] != null)
            this[index_to++] = this[index_from];
        }

        RemoveRange(index_to, Count - index_to);
      }

      // procedure DumpTriangleList(Filename : TFilename);
    }
}

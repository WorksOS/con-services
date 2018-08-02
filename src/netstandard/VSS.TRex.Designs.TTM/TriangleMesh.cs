using System.Collections.Generic;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM
{
    public class TriangleMesh
    {
        private TriVertices FVertices;
        private Triangles FTriangles;

        protected virtual void CreateLists(out TriVertices vertices, out Triangles triangles)
        {
            vertices = new TriVertices();
            triangles = new Triangles();
        }

        protected virtual void SnapToOutputResolution()
        {
            // Descendents should do something here if required
        }

        public TriangleMesh()
        {
            CreateLists(out FVertices, out FTriangles);
        }

        public TriVertices Vertices { get { return FVertices; } }

        public Triangles Triangles { get { return FTriangles; } }

        private Triangle FindNeighbour(List<List<Triangle>> Trianglelists, Triangle triangle, TriVertex fromVertex, TriVertex toVertex)
        {
            for (int i = 0; i < Trianglelists[fromVertex.Tag].Count; i++)
            {
                Triangle Result = Trianglelists[fromVertex.Tag][i];

                if (Result != triangle && Trianglelists[toVertex.Tag].IndexOf(Result) >= 0)
                    return Result;
            }

            return null;
        }

        public void BuildTriangleLinks()
        {
            // Ensure all the indices are correct

            //  Triangles.DumpTriangleList('c:\TriangleList_BeforeNumber.txt');
            //  Vertices.DumpVertexList('c:\VertexList_BeforeNumber.txt');
            Vertices.NumberVertices();

            Triangles.NumberTriangles();

            //  Vertices.DumpVertexList('c:\VertexList_AfterNumber.txt');
            //  Triangles.DumpTriangleList('c:\TriangleList_AfterNumber.txt');

            // Create point triangle lists
            List<List<Triangle>> Trianglelists = new List<List<Triangle>>(Vertices.Count + 1);

            // Add a dummy list to make the indices of the lists agree with the indices in the tag property
            Trianglelists.Add(new List<Triangle>());

            for (int i = 0; i < Vertices.Count; i++)
            {
                Trianglelists.Add(new List<Triangle>());
            }

            // Vertices.DumpVertexList('c:\VertexList_AfterTriangleListCreate.txt');

            // Associate triangles with points
            for (int i = 0; i < Triangles.Count; i++)
            {
                for (int Side = 0; Side < 3; Side++)
                {
                    Trianglelists[Triangles[i].Vertices[Side].Tag].Add(Triangles[i]);
                }
            }

            // Find the neighbour for each triangle side
            for (int i = 0; i < Triangles.Count; i++)
            {
              for (int Side = 0; Side < 3; Side++)
              {
                if (Triangles[i].Neighbours[Side] != null)
                  continue;

                TriVertex FromVertex = Triangles[i].Vertices[Side];
                TriVertex ToVertex = Triangles[i].Vertices[XYZ.NextSide(Side)];

                Triangle Nbr = FindNeighbour(Trianglelists, Triangles[i], FromVertex, ToVertex);
                Triangles[i].Neighbours[Side] = Nbr;
                if (Nbr != null)
                {
                  int NbrSide = Nbr.GetSideIndex(FromVertex, ToVertex);
                  if (Nbr.Neighbours[NbrSide] == null)
                  {
                    Nbr.Neighbours[NbrSide] = Triangles[i];
                  }
                  else
                  {
                    Triangles[i].Neighbours[Side] = null;
                  }
                }
              }
            }
        }


        public Triangle GetTriangleAtPoint(double X, double Y, out double Z)
        {
            Z = Common.Consts.NullReal;

            for (int i = 0; i < Triangles.Count; i++)
            {
                Triangle Result = Triangles[i];

                if (Result.PointInTriangle(X, Y))
                {
                    Z = Result.GetHeight(X, Y);
                    return Result;
                }
            }

            return null;
        }

        public virtual void Clear()
        {
            FTriangles.Clear();
            FVertices.Clear();
        }

      //    procedure FixCrossingTriangles(MaxShortestSide: Double );

    }
}

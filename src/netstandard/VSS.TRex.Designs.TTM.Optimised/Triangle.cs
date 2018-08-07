using System;
using System.Diagnostics;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM.Optimised
{
    /// <summary>
    /// Describes a triangle in the TIN mesh
    /// </summary>
    public class Triangle
    {
        /// <summary>
        /// A 'tag' used for various purposes in TTM processing
        /// </summary>
        public int Tag { get; set; }

        public TriVertex Vertex0, Vertex1, Vertex2;

      public TriVertex GetVertex(int index)
      {
        switch (index)
        {
          case 0: return Vertex0;
          case 1: return Vertex1;
          case 2: return Vertex2;
          default: Debug.Assert(false, "Invalid vertex index");
            return null;
        }
     }

      public void SetVertex(int index, TriVertex vertex)
      {
        switch (index)
        {
          case 0:
            Vertex0 = vertex;
            break;
          case 1:
            Vertex1 = vertex;
            break;
          case 2:
            Vertex2 = vertex;
            break;
        }
      }

      public Triangle Neighbour0, Neighbour1, Neighbour2;

      public Triangle GetNeighbour(int index)
      {
        switch (index)
        {
          case 0: return Neighbour0;
          case 1: return Neighbour1;
          case 2: return Neighbour2;
          default:
            Debug.Assert(false, "Invalid neighbour index");
            return null;
        }
      }

      public void SetNeighbour(int index, Triangle neighbour)
      {
        switch (index)
        {
          case 0:
            Neighbour0 = neighbour;
            break;
          case 1:
            Neighbour1 = neighbour;
            break;
          case 2:
            Neighbour2 = neighbour;
            break;
        }
      }

    /// <summary>
    ///  FFlags is a general purpose flags field for a triangle. It is not persistent
    /// (ie: is not written to the TTM file)
    /// </summary>
    public ushort Flags { get; set; }

        protected bool GetFlag(int Index) => (Flags & (1 << Index)) != 0;

        protected void SetFlag(int Index, bool Value)
        {
            Flags = (ushort)(Value ? Flags | (1 << Index) : Flags & ~(1 << Index));
        }

        public Triangle()
        {
        }

        public Triangle(TriVertex vertex0, TriVertex vertex1, TriVertex vertex2) : this()
        {
            Vertex0 = vertex0;
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Neighbour0 = null;
            Neighbour1 = null;
            Neighbour2 = null;

            Flags = 0;
        }

        public bool Side1StaticFlag { get { return GetFlag(Consts.side1StaticFlagIndex); } set { SetFlag(Consts.side1StaticFlagIndex, value); } }
        public bool Side2StaticFlag { get { return GetFlag(Consts.side2StaticFlagIndex); } set { SetFlag(Consts.side2StaticFlagIndex, value); } }
        public bool Side3StaticFlag { get { return GetFlag(Consts.side3StaticFlagIndex); } set { SetFlag(Consts.side3StaticFlagIndex, value); } }
        public bool IsDeletedFlag { get { return GetFlag(Consts.IsDeletedFlagIndex); } set { SetFlag(Consts.IsDeletedFlagIndex, value); } }
        public bool IsDiscardedFlag { get { return GetFlag(Consts.IsDiscardedFlagIndex); } set { SetFlag(Consts.IsDiscardedFlagIndex, value); } }

        public bool IsEdgeTriangle() => Neighbour0 == null || Neighbour1 == null || Neighbour2 == null;

        public bool IsClockwise() => XYZ.PointOnRight(Vertex0.XYZ, Vertex1.XYZ, Vertex2.XYZ );

        public double Area() => XYZ.GetTriArea(Vertex0.XYZ, Vertex1.XYZ, Vertex2.XYZ);

        public double GetHeight(double X, double Y) => XYZ.GetTriangleHeight(Vertex0.XYZ, Vertex1.XYZ, Vertex2.XYZ, X, Y );

        public XYZ Centroid() => XYZ.GetTriCentroid(Vertex0.XYZ, Vertex1.XYZ, Vertex2.XYZ);

        public bool PointInTriangle(double X, double Y)
        {
            return XYZ.PointInTriangle(Vertex0.XYZ, Vertex1.XYZ, Vertex2.XYZ, X, Y);
        }

        public bool PointInTriangleInclusive(double X, double Y)
        {
            return XYZ.PointInTriangleInclusive(Vertex0.XYZ, Vertex1.XYZ, Vertex2.XYZ, X, Y);
        }

      public bool CrossesNeighbour(int Side)
      {
        Triangle NbrTri = GetNeighbour(Side);

        if (NbrTri == null)
          return false;

        TriVertex SideStartPt = GetVertex(Side);
        TriVertex SideEndPt = GetVertex(XYZ.NextSide(Side));
        TriVertex OppositePt = GetVertex(XYZ.PrevSide(Side));
        int NbrSide = NbrTri.GetSideIndex(SideStartPt, SideEndPt);
        TriVertex NbrOppositePt = NbrTri.GetVertex(XYZ.PrevSide(NbrSide));

        return XYZ.PointOnRight(SideStartPt.XYZ, SideEndPt.XYZ, OppositePt.XYZ) == XYZ.PointOnRight(SideStartPt.XYZ, SideEndPt.XYZ, NbrOppositePt.XYZ);
      }

      public bool GetCrossingNeighbour(out int Side)
        {
            Side = 0;

            for (int i = 0; i < 3; i++)
            {
                if (CrossesNeighbour(i))
                {
                    Side = i;
                    return true;
                }
            }

            return false;
        }

        public int GetSideIndex(TriVertex VertexA, TriVertex VertexB)
        {
            int Result = GetPointIndex(VertexA);

            if (VertexB != GetVertex(XYZ.NextSide(Result)))
            {
                Result = XYZ.PrevSide(Result);
                Debug.Assert(GetVertex(Result) == VertexB, "Invalid vertex pair in GetSideIndex");
            }

            return Result;
        }

        public int GetPointIndex(TriVertex V)
        {
            int Result = FindPointIndex(V);
            Debug.Assert(Result != -1, "Invalid vertex in GetPointIndex");

            return Result;
        }

        public int FindPointIndex(TriVertex V)
        {
            if (V == Vertex0) return 0;
            if (V == Vertex1) return 1;
            if (V == Vertex2) return 2;
            return -1;
        }

        public void GetMinMaxSides(out int ShortestSide, out int LongestSide,
        out double MinSideLen, out double MaxSideLen)
        {
            LongestSide = 0;
            ShortestSide = 0;
            MaxSideLen = XYZ.Get3DLength(Vertex1.XYZ, Vertex2.XYZ);
            MinSideLen = MaxSideLen;

            for (int s = 1; s < 2; s++)
            {
                double SideLen = XYZ.Get3DLength(GetVertex(s).XYZ, GetVertex(XYZ.NextSide(s)).XYZ);
                if (SideLen > MaxSideLen)
                {
                    MaxSideLen = SideLen;
                    LongestSide = s;
                }
                if (SideLen < MinSideLen)
                {
                    MinSideLen = SideLen;
                    ShortestSide = s;
                }
            }
        }

        public int GetNeighbourIndex(Triangle Neighbour)
        {
          if (Neighbour0 == Neighbour) return 0;
          if (Neighbour1 == Neighbour) return 1;
          if (Neighbour2 == Neighbour) return 2;

          return -1;
        }

        public int NeighbourCount()
        {
            int Result = 3;

            if (Neighbour0 == null) Result--;
            if (Neighbour1 == null) Result--;
            if (Neighbour2 == null) Result--;

            return Result;
        }

        public bool GetDuplicateNeighbour(out Triangle DuplNeighbour)
        {
            DuplNeighbour = null;

            for (int n = 0; n < 2; n++)
            {
                Triangle Nbr = GetNeighbour(n);

                if (Nbr != null
                  && (Nbr.FindPointIndex(Vertex0) != -1)
                  && (Nbr.FindPointIndex(Vertex1) != -1)
                  && (Nbr.FindPointIndex(Vertex2) != -1))
                {
                    DuplNeighbour = Nbr;
                    break;
                }
            }
            return DuplNeighbour != null;
        }

        public bool HasDuplicateVertices()
        {
            return (Vertex0 == Vertex1) || (Vertex1 == Vertex2) || (Vertex2 == Vertex0);
        }

        public void GetExtents(out double MinX, out double MinY, out double MaxX, out double MaxY)
        {
          MinX = Math.Min(Math.Min(Vertex0.X, Vertex1.X), Vertex2.X);
          MinY = Math.Min(Math.Min(Vertex0.Y, Vertex1.Y), Vertex2.Y);
          MaxX = Math.Max(Math.Max(Vertex0.X, Vertex1.X), Vertex2.X);
          MaxY = Math.Max(Math.Max(Vertex0.Y, Vertex1.Y), Vertex2.Y);
        }
    }
}

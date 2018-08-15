using System.Diagnostics;
using System.Linq;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.TTM
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

    public TriVertex[] Vertices = new TriVertex[3];
    public Triangle[] Neighbours = new Triangle[3];

    /// <summary>
    ///  FFlags is a general purpose flags field for a triangle. It is not persistent
    /// (ie: is not written to the TTM file)
    /// </summary>
    public ushort Flags { get; set; }

    protected bool GetFlag(int Index) => (Flags & (1 << Index)) != 0;

    protected void SetFlag(int Index, bool Value)
    {
      Flags = (ushort) (Value ? Flags | (1 << Index) : Flags & ~(1 << Index));
    }

    public Triangle()
    {

    }

    public Triangle(TriVertex Vertex1, TriVertex Vertex2, TriVertex Vertex3) : this()
    {
      Vertices[0] = Vertex1;
      Vertices[1] = Vertex2;
      Vertices[2] = Vertex3;
      Neighbours[0] = null;
      Neighbours[1] = null;
      Neighbours[2] = null;

      Flags = 0;
    }

    public bool Side1StaticFlag
    {
      get { return GetFlag(Consts.side1StaticFlagIndex); }
      set { SetFlag(Consts.side1StaticFlagIndex, value); }
    }

    public bool Side2StaticFlag
    {
      get { return GetFlag(Consts.side2StaticFlagIndex); }
      set { SetFlag(Consts.side2StaticFlagIndex, value); }
    }

    public bool Side3StaticFlag
    {
      get { return GetFlag(Consts.side3StaticFlagIndex); }
      set { SetFlag(Consts.side3StaticFlagIndex, value); }
    }

    public bool IsDeletedFlag
    {
      get { return GetFlag(Consts.IsDeletedFlagIndex); }
      set { SetFlag(Consts.IsDeletedFlagIndex, value); }
    }

    public bool IsDiscardedFlag
    {
      get { return GetFlag(Consts.IsDiscardedFlagIndex); }
      set { SetFlag(Consts.IsDiscardedFlagIndex, value); }
    }

    public bool IsEdgeTriangle() => Neighbours[0] == null || Neighbours[1] == null || Neighbours[2] == null;

    public bool IsClockwise() => XYZ.PointOnRight(Vertices[0].XYZ, Vertices[1].XYZ, Vertices[2].XYZ);

    public double Area() => XYZ.GetTriArea(Vertices[0].XYZ, Vertices[1].XYZ, Vertices[2].XYZ);

    public double GetHeight(double X, double Y) => XYZ.GetTriangleHeight(Vertices[0].XYZ, Vertices[1].XYZ, Vertices[2].XYZ, X, Y);

    public XYZ Centroid() => XYZ.GetTriCentroid(Vertices[0].XYZ, Vertices[1].XYZ, Vertices[2].XYZ);

    public bool PointInTriangle(double X, double Y)
    {
      return XYZ.PointInTriangle(Vertices[0].XYZ, Vertices[1].XYZ, Vertices[2].XYZ, X, Y);
    }

    public bool PointInTriangleInclusive(double X, double Y)
    {
      return XYZ.PointInTriangleInclusive(Vertices[0].XYZ, Vertices[1].XYZ, Vertices[2].XYZ, X, Y);
    }

    public bool CrossesNeighbour(int Side)
    {
      Triangle NbrTri = Neighbours[Side];

      if (NbrTri == null)
        return false;

      TriVertex SideStartPt = Vertices[Side];
      TriVertex SideEndPt = Vertices[XYZ.NextSide(Side)];
      TriVertex OppositePt = Vertices[XYZ.PrevSide(Side)];
      int NbrSide = NbrTri.GetSideIndex(SideStartPt, SideEndPt);
      TriVertex NbrOppositePt = NbrTri.Vertices[XYZ.PrevSide(NbrSide)];

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

      if (VertexB != Vertices[XYZ.NextSide(Result)])
      {
        Result = XYZ.PrevSide(Result);
        Debug.Assert(Vertices[Result] == VertexB, "Invalid vertex pair in GetSideIndex");
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
      if (V == Vertices[0]) return 0;
      if (V == Vertices[1]) return 1;
      if (V == Vertices[2]) return 2;
      return -1;
    }

    public void GetMinMaxSides(out int ShortestSide, out int LongestSide,
      out double MinSideLen, out double MaxSideLen)
    {
      LongestSide = 0;
      ShortestSide = 0;
      MaxSideLen = XYZ.Get3DLength(Vertices[1].XYZ, Vertices[2].XYZ);
      MinSideLen = MaxSideLen;

      for (int s = 1; s < 2; s++)
      {
        double SideLen = XYZ.Get3DLength(Vertices[s].XYZ, Vertices[XYZ.NextSide(s)].XYZ);
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
      for (int i = 0; i < 3; i++)
        if (Neighbours[i] == Neighbour)
          return i;

      return -1;
    }

    public int NeighbourCount()
    {
      int Result = 3;

      for (int s = 0; s < 3; s++)
        if (Neighbours[s] == null)
          Result--;

      return Result;
    }

    public bool GetDuplicateNeighbour(out Triangle DuplNeighbour)
    {
      DuplNeighbour = null;

      for (int n = 0; n < 2; n++)
      {
        Triangle Nbr = Neighbours[n];

        if (Nbr != null
            && (Nbr.FindPointIndex(Vertices[0]) != -1)
            && (Nbr.FindPointIndex(Vertices[1]) != -1)
            && (Nbr.FindPointIndex(Vertices[2]) != -1))
        {
          DuplNeighbour = Nbr;
          break;
        }
      }

      return DuplNeighbour != null;
    }

    public bool HasDuplicateVertices()
    {
      return (Vertices[0] == Vertices[1]) || (Vertices[1] == Vertices[2]) || (Vertices[2] == Vertices[0]);
    }

    private void GetExtents(out double MinX, out double MinY, out double MaxX, out double MaxY)
    {
      // NB: Not as performant as it could be. Revisit it this method is called often
      MinX = Vertices.Min(x => x.X);
      MaxX = Vertices.Max(x => x.X);
      MinY = Vertices.Min(x => x.Y);
      MaxY = Vertices.Max(x => x.Y);
    }
  }
}

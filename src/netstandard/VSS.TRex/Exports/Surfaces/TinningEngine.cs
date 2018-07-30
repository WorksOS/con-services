using System;
using System.Diagnostics;
using VSS.TRex.Designs.TTM;

namespace VSS.TRex.Exports.Surfaces
{
  public class TinningEngine
  {
    public TrimbleTINModel TIN { get; set; }

//    public GridToTINDecimator Decimator {get; set;}

    /// <summary>
    /// List of triangles that need adjusting to the new coord
    /// </summary>
    private TriListNode[] AffectedList;

    /// <summary>
    /// List of triangles that may need adjusting }
    /// </summary>
    private TriListNode[] CandidateList;

    private AffSideNode[] AffSideList;

    private int NumAffected { get; set; }
    private int NumCandidates { get; set; }
    private int NumAffSides { get; set; }

    /// <summary>
    /// SuccLastTriangle and SuccSuccLastTriangle represent the one or two new
    /// triangles that will be created when processing triangles affected by the
    /// DeLauney insertion of a new vertex into the surface.
    /// </summary>
    private Triangle SuccLastTriangle { get; set; }

    /// <summary>
    /// SuccLastTriangle and SuccSuccLastTriangle represent the one or two new
    /// triangles that will be created when processing triangles affected by the
    /// DeLauney insertion of a new vertex into the surface.
    /// </summary>
    private Triangle SuccSuccLastTriangle { get; set; }

    public TinningEngine()
    {
      TIN = new TrimbleTINModel();

      AffSideList = new AffSideNode[1000];
      CandidateList = new TriListNode[1000];
      AffSideList = new AffSideNode[1000];
    }

    /// <summary>
    /// Remove the synthetic corner triangles and vertices created as a framework to build the decimated
    /// TIN surface within
    /// </summary>
    /// <param name="maxRealVertex"></param>
    protected void RemoveCornerTriangles(int maxRealVertex)
    {
      // Remove all triangles that have one of the MBR vertices as a corner vertex
      for (int I = TIN.Triangles.Count - 1; I > 0; I--)
      {
        Triangle Tri = TIN.Triangles[I];
        if (Tri.Vertices[0].Tag > maxRealVertex ||
            Tri.Vertices[1].Tag > maxRealVertex ||
            Tri.Vertices[2].Tag > maxRealVertex)
          TIN.Triangles.RemoveTriangle(Tri);
      }

      //TIN.Triangles.Pack;
      TIN.Triangles.NumberTriangles();

      // Remove the four MBR vertices from the TIN
      for (int I = 0; I < 4; I++)
        TIN.Vertices.RemoveAt(TIN.Vertices.Count - 1);
    }

    /// <summary>
    /// { Returns the triangle neighbouring aTriangle which is nearer to
    /// aPoint than aTriangle.If aPoint is in aTriangle then aTriangle is
    /// returned, and found set to true. If aPoint is outside the model,
    /// returns Nil - requires model to be convex. }
    /// </summary>
    /// <param name="aPoint"></param>
    /// <param name="aTriangle"></param>
    /// <param name="lastTri"></param>
    /// <param name="found"></param>
    /// <returns></returns>
    protected Triangle GetBestTri(TriVertex aPoint, Triangle aTriangle, Triangle lastTri, out bool found)
    {
      Triangle Result = null;
      found = false;

      TriVertex firstPoint = aTriangle.Vertices[0];
      TriVertex secondPoint = aTriangle.Vertices[1];

      if (aTriangle.Neighbours[0] != lastTri && TinningUtils.DefinitelyLeftOfBaseLine(aPoint, firstPoint, secondPoint))
        Result = aTriangle.Neighbours[0];
      else
      {
        TriVertex thirdPoint = aTriangle.Vertices[2];
        if (aTriangle.Neighbours[1] != lastTri && TinningUtils.DefinitelyLeftOfBaseLine(aPoint, secondPoint, thirdPoint))
          Result = aTriangle.Neighbours[1];
        else if (aTriangle.Neighbours[2] != lastTri && TinningUtils.DefinitelyLeftOfBaseLine(aPoint, thirdPoint, firstPoint))
          Result = aTriangle.Neighbours[2];
        else // to the right of each line -must be inside aTriangle 
        {
          Result = aTriangle;
          found = true;
        }
      }

      return Result;
    }

    protected void makeUpdatedTriangle(Triangle tri,
      TriVertex firstCoord,
      TriVertex secondCoord,
      TriVertex thirdCoord,
      Triangle firstSide,
      Triangle secondSide,
      Triangle thirdSide,
      int theFlags,
      bool isDeleted)
    {
      Debug.Assert(tri != firstSide && tri != secondSide && tri != thirdSide, "Triangle cannot be its own neighbour");

      tri.Vertices[0] = firstCoord;
      tri.Vertices[1] = secondCoord;
      tri.Vertices[2] = thirdCoord;
      tri.Neighbours[0] = firstSide;
      tri.Neighbours[1] = secondSide;
      tri.Neighbours[2] = thirdSide;

      tri.Flags = (ushort) (theFlags & ~(Consts.IsDeletedFlag + Consts.IsDiscardedFlag + Consts.IsTriDrawnFlag + Consts.IsContDrawnFlag));
      if (isDeleted)
        tri.IsDeletedFlag = true;

      TriangleUpdated(tri);
    }

    /// <summary>
    /// Delegate allowing a client to supply behaviour triggered by addition of triangles 
    /// </summary>
    public Action<Triangle> TriangleAdded = tri => { };

    /// <summary>
    ///  Delagate allowing a client to supply behaviour triggered by addition of triangles 
    /// </summary>
    public Action<Triangle> TriangleUpdated = tri => { };

//    protected void TriangleAdded(Triangle tri)
    //    {
    //      Decimator.TriangleAdded(tri);
    //    }

//    protected void TriangleUpdated(Triangle tri)
//    {
//      Decimator.TriangleUpdated(tri);
//    }

    protected Triangle NewTriangle(TriVertex coord1, TriVertex coord2, TriVertex coord3,
      Triangle side1, Triangle side2, Triangle side3,
      int theFlags,
      bool deleteIt)
    {
      Triangle result;

      if (SuccLastTriangle != null)
      {
        result = SuccLastTriangle;

        if ((coord1.X == coord2.X && coord1.Y == coord2.Y) ||
            (coord2.X == coord3.X && coord2.Y == coord3.Y) ||
            (coord3.X == coord1.X && coord3.Y == coord1.Y))
        {
          Debug.Assert(false, "Coordinates for new triangle are not unique");
        }

        result.Vertices[0] = coord1;
        result.Vertices[1] = coord2;
        result.Vertices[2] = coord3;

        TIN.Triangles.Add(result);

        TriangleAdded(result);

        // NOTE: This triangle will not yet exist in the TTM index so we must add it now
        //      FIndex.AddTriangle(Result, Result.Tag - 1);
      }
      else
      {
        result = TIN.Triangles.AddTriangle(coord1, coord2, coord3);

        TriangleAdded(result);
      }

      Debug.Assert(result != side1 && result != side2 && result != side3, "Triangle cannot be its own neighbour");

      result.Neighbours[0] = side1;
      result.Neighbours[1] = side2;
      result.Neighbours[2] = side3;

      result.Flags = (ushort) (theFlags & ~(Consts.IsDeletedFlag + Consts.IsDiscardedFlag +
                                            Consts.IsTriDrawnFlag + Consts.IsContDrawnFlag));

      if (deleteIt)
        result.IsDeletedFlag = true;

      return result;
    }

    /// <summary>
    /// search triList for theTri. Return the index of this node in the list 
    /// </summary>
    /// <param name="triList"></param>
    /// <param name="theTri"></param>
    /// <param name="NumEntries"></param>
    /// <returns></returns>
    protected int getNode(TriListNode[] triList, Triangle theTri, int NumEntries)
    {
      for (int I = 0; I < NumEntries; I++)
        if (triList[I].Tri == theTri)
          return I;

      return -1;
    }

    protected int GetFlags(StatusType[] status, bool staticFlag, TriVertex lastPoint, TriVertex nextPoint)
    {
      int Result = 0;

      if (staticFlag)
        Result = Result | Consts.side3StaticFlag;

      if (lastPoint == status[0].firstCoord || lastPoint == status[0].secondCoord)
        if (status[0].isStatic)
          Result = Result | Consts.side2StaticFlag;

      if (lastPoint == status[1].firstCoord || lastPoint == status[1].secondCoord)
        if (status[1].isStatic)
          Result = Result | Consts.side2StaticFlag;

      if (nextPoint == status[0].firstCoord || nextPoint == status[0].secondCoord)
        if (status[0].isStatic)
          Result = Result | Consts.side1StaticFlag;

      if (nextPoint == status[1].firstCoord || nextPoint == status[1].secondCoord)
        if (status[1].isStatic)
          Result = Result | Consts.side1StaticFlag;

      return Result;
    }

    protected void UpdateNeighbour(Triangle theTri, TriVertex thePoint, Triangle newTri)
    {
      if (theTri != null)
        for (int j = 0; j < 3; j++)
          if (theTri.Vertices[j] == thePoint)
            theTri.Neighbours[(j + 1) % 3] = newTri;
    }

    /// <summary>
    /// { sidePtr points to what will be side[3] of the first updated triangle.
    /// return the triangle that will be its side[2].  Generally it will be
    /// the second of the 2 new triangles formed.If the new coord is being
    /// inserted on the edge of the model(this is impossible if forming),
    /// only one new triangle will be formed by this operation.If this is the
    /// case, the last side may be this triangle, or the edge itself. }
    /// </summary>
    /// <param name="sidePoint"></param>
    /// <param name="status"></param>
    /// <param name="forming"></param>
    protected Triangle GetLastSide(TriVertex sidePoint, StatusType[] status, bool forming)
    {
      Triangle Result = null;

      Triangle AddEmptyTriangle(int Offset)
      {
        Triangle Result2 = TIN.Triangles.CreateTriangle(null, null, null);
        Result2.Tag = TIN.Triangles.Count + Offset;

        return Result2;
      }

      SuccLastTriangle = null;
      SuccSuccLastTriangle = null;

      if (forming || !(status[0].edge || status[1].edge))
      {
        SuccLastTriangle = AddEmptyTriangle(1);
        SuccSuccLastTriangle = AddEmptyTriangle(2);
        Result = SuccSuccLastTriangle;
      }
      else if ((status[0].edge && sidePoint == status[0].secondCoord) ||
               (status[1].edge && sidePoint == status[1].secondCoord))
        Result = null;
      else
      {
        SuccLastTriangle = AddEmptyTriangle(1);
        Result = SuccLastTriangle;
      }

      return Result;
    }

    /// <summary>
    /// sidePtr points to what will be side[2] of the next new/updated triangle.
    /// Return the triangle that will be its side[1].  Generally it will be
    /// next triangle in the affectedPtr list.If we have run out of these,
    /// It will be the first or second of the to-be-created triangles.If the
    /// currently being made triangle is the last one, return firstTri.If
    /// the new coord is being inserted on the edge of the model(impossible
    /// if forming), the next side may be the edge. }
    /// </summary>
    /// <param name="sideIndex"></param>
    /// <param name="status"></param>
    /// <param name="forming"></param>
    /// <param name="index"></param>
    /// <param name="firstTri"></param>
    /// <returns></returns>
    protected Triangle GetNextSide(int sideIndex, StatusType[] status, bool forming, int index, Triangle firstTri)
    {
      if (sideIndex == NumAffSides)
        return firstTri;

      if (!forming &&
          (status[0].edge && AffSideList[sideIndex + 1].point == status[0].firstCoord) &&
          (status[1].edge && AffSideList[sideIndex + 1].point == status[1].firstCoord))
        return null;

      if (index + 1 < NumAffected)
        return AffectedList[index + 1].Tri;

      if (index > NumAffected)
      {
        if (forming || !(status[0].edge || status[1].edge))
          return SuccSuccLastTriangle;
        else
          return SuccLastTriangle;
      }

      return SuccLastTriangle;
    }

    /// <summary>
    /// sidePtr points to what will be side[3] of the first updated triangle.
    /// Return this triangle, the first in the affectedTris list.If the new
    /// coord is being inserted on the edge of the model(this is impossible
    /// if forming), return edge_index if the last new triangle formed will
    /// not be a neighbour to firstTri. 
    /// </summary>
    /// <param name="sidePoint"></param>
    /// <param name="status"></param>
    /// <param name="forming"></param>
    /// <param name="affectedTri"></param>
    /// <returns></returns>
    protected Triangle GetFirstTri(TriVertex sidePoint, StatusType[] status, bool forming, Triangle affectedTri)
    {
      if (!forming && (status[0].edge && sidePoint == status[0].firstCoord || status[1].edge && sidePoint == status[0].firstCoord))
        return null;

      return affectedTri;
    }

    protected bool InList(Triangle theTri, TriListNode[] theList, int numEntries)
    {
      for (int i = 0; i < numEntries; i++)
        if (theList[i].Tri == theTri)
          return true;

      return false;
    }

    /// <summary>
    /// Constructs the list of affected sides from the most recent point addition
    /// make affSideList describe a walk around the edge of the polygon making up
    // affectedTris - ie the boundary between affectedTris and surroundingTris
    /// </summary>
    /// <param name="forming"></param>
    protected void MakeAffSideList(bool forming)
    {
      //        Index         : Integer;
      //      TestIndex: Integer;
      //      triPtr: TTriListNodePtr;
      //      done: boolean;
      //      j: Integer;
      //      nextPoint: TTriVertex;
      //      SideIdx: Integer;
      //      Found: Boolean;

      void LengthenAffSideList()
      {
        Array.Resize(ref AffSideList, AffSideList.Length + 1000);

        // Reset all the next pointers in the affected side list
        for (int I = 0; I < NumAffSides - 2; I++)
          AffSideList[I].Next = I + 1;
      }

      //get Index to point to affectedTri with a side on the edge of the
      //  polygon.Set side[j] of this triangle to the edge side 
      int Index = 0;
      bool Found = false;
      int SideIdx = 0;

      while (Index < NumAffected && !Found)
      {
        for (int J = 0; J < 3; J++)
          if (AffectedList[Index].Tri.Neighbours[J] == null || InList(AffectedList[Index].Tri.Neighbours[J], CandidateList, NumCandidates))
          {
            SideIdx = J;
            Found = true;
            break;
          }

        if (Found)
          break;

        Index++;
      }

      if (NumAffSides >= AffSideList.Length)
        LengthenAffSideList();
      NumAffSides++;

      AffSideList[NumAffSides - 1].point = AffectedList[Index].Tri.Vertices[SideIdx];
      AffSideList[NumAffSides - 1].tri = AffectedList[Index].Tri;
      AffSideList[NumAffSides - 1].deleted = AffectedList[Index].Tri.IsDeletedFlag;
      AffSideList[NumAffSides - 1].side = AffectedList[Index].Tri.Neighbours[SideIdx];
      if (!forming)
        AffSideList[NumAffSides - 1].isStatic = EdgeIsStatic(AffectedList[Index].Tri, SideIdx);

      AffSideList[NumAffSides - 1].Next = -1;
      if (NumAffSides > 1)
        AffSideList[NumAffSides - 2].Next = NumAffSides - 1;

      // set nextPoint to the clockwise - most point of this side - the next side
      // in the list will share this point }
      TriVertex nextPoint = AffectedList[Index].Tri.Vertices[SideIdx % 3];

      bool done = false;
      while (!done)
      {
        //if the next side(clockwise) of the triPtr triangle adjoins a triangle
        // within the polygon move triPtr to the adjoining triangle
        //  this is repeated until we find a side on the edge of the polygon
        //  that shares nextPoint 

        int TestIndex = getNode(AffectedList, AffectedList[Index].Tri.Neighbours[SideIdx % 3], NumAffected);
        if (TestIndex != -1)
        {
          Index = TestIndex;
          while (TestIndex != -1)
          {
            for (int j = 0; j < 3; j++)
              if (AffectedList[Index].Tri.Vertices[j] == nextPoint)
              {
                SideIdx = j;
                break;
              }

            TestIndex = getNode(AffectedList, AffectedList[Index].Tri.Neighbours[SideIdx], NumAffected);
            if (TestIndex != -1)
              Index = TestIndex;
          }
        }
        else
          SideIdx = SideIdx % 3;

        if (NumAffSides >= AffSideList.Length)
          LengthenAffSideList();
        NumAffSides++;

        AffSideList[NumAffSides - 1].tri = AffectedList[Index].Tri;
        AffSideList[NumAffSides - 1].deleted = AffectedList[Index].Tri.IsDeletedFlag;
        AffSideList[NumAffSides - 1].point = AffectedList[Index].Tri.Vertices[SideIdx];
        AffSideList[NumAffSides - 1].side = AffectedList[Index].Tri.Neighbours[SideIdx];
        if (!forming)
          AffSideList[NumAffSides - 1].isStatic = EdgeIsStatic(AffectedList[Index].Tri, SideIdx);

        AffSideList[NumAffSides - 1].Next = -1;
        if (NumAffSides > 1)
          AffSideList[NumAffSides - 2].Next = NumAffSides - 1;

        nextPoint = AffectedList[Index].Tri.Vertices[SideIdx % 3];

        // keep going until we reach the start point 
        done = (nextPoint == AffSideList[0].point);
      }
    }

    protected bool EdgeIsADelBound(Triangle tri, int side)
    {
      return tri.Neighbours[side] != null && tri.IsDeletedFlag ^ tri.Neighbours[side].IsDeletedFlag;
    }

    protected bool EdgeIsStatic(Triangle tri, int side)
    {
      if (side == 0) return tri.Side1StaticFlag || EdgeIsADelBound(tri, side);
      if (side == 1) return tri.Side2StaticFlag || EdgeIsADelBound(tri, side);
      if (side == 2) return tri.Side3StaticFlag || EdgeIsADelBound(tri, side);

      return false;
    }

    /// <summary>
    /// Returns true if the circumcircle of theTri contains theCoordRec 
    /// </summary>
    /// <param name="theTri"></param>
    /// <param name="theCoord"></param>
    /// <returns></returns>
    protected bool Influenced(Triangle theTri, TriVertex theCoord)
    {
      double cotan = TinningUtils.Cotangent(theTri.Vertices[2], theTri.Vertices[0], theTri.Vertices[1]);
      if (cotan > -1E20)
      {
        double cNorth = ((theTri.Vertices[1].Y + theTri.Vertices[0].Y) / 2) -
                        ((theTri.Vertices[1].X - theTri.Vertices[0].X) / 2) *
                        cotan;
        double cEast = ((theTri.Vertices[1].X + theTri.Vertices[0].X) / 2) +
                       ((theTri.Vertices[1].Y - theTri.Vertices[0].Y) / 2) *
                       cotan;
        double radSq = Math.Pow(cNorth - theTri.Vertices[0].Y, 2) + Math.Pow(cEast - theTri.Vertices[0].X, 2);

        return Math.Pow(theCoord.X - cEast, 2) + Math.Pow(theCoord.Y - cNorth, 2) < radSq;
      }

      return false;
    }

    /// <summary>
    /// Return true if the end points of 'side' side of triBuffer match one
    /// of the status pair 
    /// </summary>
    /// <param name="tri"></param>
    /// <param name="side"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    protected bool StatusOverrides(Triangle tri, int side, StatusType[] status)
    {
      return (tri.Vertices[side] == status[0].firstCoord && tri.Vertices[(side + 1) % 3] == status[0].secondCoord)
             ||
             (tri.Vertices[side] == status[1].firstCoord && tri.Vertices[(side + 1) % 3] == status[1].secondCoord);
    }

    /// <summary>
    /// Return true if theSide side of tri is a boundary or breakline 
    /// </summary>
    /// <param name="tri"></param>
    /// <param name="theSide"></param>
    /// <returns></returns>
    protected bool CrossingStaticSide(Triangle tri, int theSide)
    {
      if (theSide == 0) return tri.Side1StaticFlag || EdgeIsADelBound(tri, theSide);
      if (theSide == 1) return tri.Side2StaticFlag || EdgeIsADelBound(tri, theSide);
      if (theSide == 2) return tri.Side3StaticFlag || EdgeIsADelBound(tri, theSide);

      return false;
    }

    /// <summary>
    /// If theTri is not in affectedList or candidateList, add it to the
    /// candidate list.candidateList has dummy node at its head. If unAffected then set the
    /// notAffected field of the candidate. 
    /// </summary>
    /// <param name="theTri"></param>
    /// <param name="unAffected"></param>
    protected void AddCandidate(Triangle theTri, bool unAffected)
    {
      if (theTri == null)
        return;

      for (int I = 0; I < NumAffected; I++)
        if (AffectedList[I].Tri == theTri)
          return;

      for (int I = 0; I < NumCandidates; I++)
        if (CandidateList[I].Tri == theTri)
          return;

      if (NumCandidates >= CandidateList.Length)
        Array.Resize(ref CandidateList, CandidateList.Length + 1000);
      NumCandidates++;

      CandidateList[NumCandidates - 1].Tri = theTri;
      CandidateList[NumCandidates - 1].NotAffected = unAffected;
    }

    protected Triangle LocateTriangle(TriVertex coord, Triangle currentTri, bool forming)
    {
      // Locate the triangle that includes Coord
      // First check if the current triangle includes Coord
      if (currentTri != null && currentTri.PointInTriangleInclusive(coord.X, coord.Y))
        return currentTri;

      // Current Tri was not the one, but there is no spatial indexing support, so give up
      return null;
    }

    /// <summary>
    /// Note - null implementation as there is no context for max min
    /// </summary>
    /// <param name="aPoint"></param>
    /// <returns></returns>
    protected bool OutsideMaxMin(TriVertex aPoint) => false;

    /// <summary>
    /// Ensure lastTri is not discarded (ie invalid). If so, return a valid triangle
    /// </summary>
    /// <param name="lastTri"></param>
    protected void CheckLastTri(ref Triangle lastTri)
    {
      if (lastTri == null)
        lastTri = TIN.Triangles[0];

      if (lastTri.IsDiscardedFlag)
        for (int i = 0; i < TIN.Triangles.Count; i++)
          if (!TIN.Triangles[i].IsDiscardedFlag)
          {
            lastTri = TIN.Triangles[i];
            return;
          }

      if (lastTri.IsDiscardedFlag)
        lastTri = null;
    }

    /// <summary>
    /// NOTE - if aPoint is possibly a point currently in the model, it is
    /// faster and safer to use getTriangle().  Do not assume that
    /// locateTriangle() will actually find the triangle that an existing
    /// point is a vertex of, due to floating point inaccuracies
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="lastTri"></param>
    /// <param name="forming"></param>
    /// <param name="checkIt"></param>
    /// <returns></returns>
    protected Triangle LocateTriangle2(TriVertex coord, Triangle lastTri, bool forming, bool checkIt)
    {
      int nSteps = 0;
      bool found = false;
      bool outsideModel = OutsideMaxMin(coord);

      if (lastTri == null)
        lastTri = TIN.Triangles[0];

      if (checkIt)
        CheckLastTri(ref lastTri);

      if (lastTri == null) // No undiscarded triangles left 
        return null;

      int StartAt = 0;
      Triangle currentTri = lastTri;
      while (!outsideModel && !found)
      {
        if (++nSteps > 10000)
        {
          // TODO? Inc(SurfaceWalkOverflowCount);
          nSteps = 0;

          // Try again from another start triangle
          StartAt = (StartAt + 7919) % TIN.Triangles.Count; // 7919 is an appropriate sized prime
          lastTri = TIN.Triangles[StartAt];
          currentTri = lastTri;
        }

        Triangle nextTri = GetBestTri(coord, currentTri, lastTri, out found);
        lastTri = currentTri;
        currentTri = nextTri;
        outsideModel = currentTri == null;
      }

      return found ? currentTri : null;
    }

    public TriVertex AddVertex(double x, double y, double z) => TIN.Vertices.AddPoint(x, y, z);
    public TriVertex AddVertex(TriVertex vertex) => TIN.Vertices.AddPoint(vertex.X, vertex.Y, vertex.Z);

    public Triangle AddTriangle(TriVertex V1, TriVertex V2, TriVertex V3)
    {
      // Add the triangle and assign its tag member to be its position in the list
      // (ie: it will be the last one)
      Triangle tri = TIN.Triangles.AddTriangle(V1, V2, V3);
      tri.Tag = TIN.Triangles.Count;

      TriangleAdded(tri);

      return tri;
    }

    public void AddTriangle(double X1, double Y1, double Z1,
      double X2, double Y2, double Z2,
      double X3, double Y3, double Z3)
    {
      AddTriangle(TIN.Vertices.AddPoint(X1, Y1, Z1),
        TIN.Vertices.AddPoint(X1, Y1, Z1),
        TIN.Vertices.AddPoint(X1, Y1, Z1));
    }


    /// <summary>
    /// add four coords to the model, which form the minimum bounding rectangle
    /// about the selected points.Return these. }
    /// </summary>
    /// <param name="tl"></param>
    /// <param name="tr"></param>
    /// <param name="bl"></param>
    /// <param name="br"></param>
    protected void MakeMinimumBoundingRectangle(out TriVertex tl, out TriVertex tr, out TriVertex bl, out TriVertex br)
    {
      const int aBit = 10; // Arbitrary size expansion for encompassing rectangle

      tl = AddVertex(TIN.Header.MinimumEasting - aBit, TIN.Header.MaximumNorthing + aBit, 0);
      tr = AddVertex(TIN.Header.MaximumEasting + aBit, TIN.Header.MaximumNorthing + aBit, 0);
      bl = AddVertex(TIN.Header.MinimumEasting - aBit, TIN.Header.MinimumNorthing - aBit, 0);
      br= AddVertex(TIN.Header.MaximumEasting + aBit, TIN.Header.MinimumNorthing - aBit, 0);
    }

    /// <summary>
    /// Add the two starting triangles.
    /// </summary>
    /// <param name="tl"></param>
    /// <param name="tr"></param>
    /// <param name="bl"></param>
    /// <param name="br"></param>
    protected void CreateInitialTriangles(TriVertex tl, TriVertex tr, TriVertex bl, TriVertex br)
    {
      AddTriangle(tl, tr, bl);
      AddTriangle(bl, tr, br);

      // Ensure their neighbours are correct
      TIN.Triangles[0].Neighbours[1] = TIN.Triangles[1];
      TIN.Triangles[1].Neighbours[0] = TIN.Triangles[0];

      TIN.Triangles.NumberTriangles();
    }

    /// <summary>
    /// The triangles in the affected list form a polygon about the coord being
    /// inserted.Update these triangles, and form new triangles using each
    /// side of the polygon as baselines, with the new coord
    /// </summary>
    /// <param name="newCoord"></param>
    /// <param name="forming"></param>
    /// <param name="status"></param>
    protected void AlterAffected(TriVertex newCoord, bool forming, StatusType[] status)
    {
      // determine the edge of the polygon -fill affSideList to describe this makeAffSideList(forming);

      if (NumAffected == 0) // Nothing to do!
        return;

      int sidePtr = 0;
      int AffectedIdx = 0;

      //first triangle in the affected list 
      Triangle firstTri = GetFirstTri(AffSideList[sidePtr].point, status, forming, AffectedList[0].Tri);

      // triangle on side 1 of new/ updated triangle - generally the previously made / updated one
      Triangle lastSide = GetLastSide(AffSideList[sidePtr].point, status, forming);

      // Make two new triangle
      while (sidePtr != -1) //  more triangles to update/ make 
      {
        if (forming ||
            !(status[0].edge && AffSideList[sidePtr].point == status[0].firstCoord ||
              status[1].edge && AffSideList[sidePtr].point == status[1].firstCoord))
        {
          TriVertex nextPoint;
          if (AffSideList[sidePtr].Next == -1)
            nextPoint = AffSideList[0].point;
          else
            nextPoint = AffSideList[AffSideList[sidePtr].Next].point;

          int theFlags;
          if (forming)
            theFlags = 0;
          else
            theFlags = GetFlags(status, AffSideList[sidePtr].isStatic, AffSideList[sidePtr].point, nextPoint);

          // triangle on side 2 of new/ updated triangle - generally the next to be made / updated
          Triangle nextSide = GetNextSide(sidePtr, status, forming, AffectedIdx, firstTri);

          if (AffectedIdx >= NumAffected)
          {
            // the last one or two triangles will be new, rather than updated 
            Debug.Assert(SuccLastTriangle != null, "Cannot use a nil triangle for a new triangle");
            lastSide = NewTriangle(nextPoint,
              newCoord,
              AffSideList[sidePtr].point,
              nextSide,
              lastSide,
              AffSideList[sidePtr].side,
              theFlags,
              AffSideList[sidePtr].deleted);

            SuccLastTriangle = SuccSuccLastTriangle;
            SuccSuccLastTriangle = null;
          }
          else
          {
            //  update affectedPtr^.tri triangle 
            makeUpdatedTriangle(AffectedList[AffectedIdx].Tri,
              nextPoint,
              newCoord,
              AffSideList[sidePtr].point,
              nextSide,
              lastSide,
              AffSideList[sidePtr].side,
              theFlags,
              AffSideList[sidePtr].deleted);

            lastSide = AffectedList[AffectedIdx].Tri;
            AffectedIdx++;
          }

          // tell the triangles neighbour that the triangle has become its new neighbour 
          UpdateNeighbour(AffSideList[sidePtr].side, AffSideList[sidePtr].point, lastSide);

          /*
              // DEBUG check
              for I := 0 to FTIN.Triangles.Count - 1 do
              with FTIN.Triangles[I] do
            for J := 1 to 3 do
                with Vertex[J] do
            Assert(X > 1, 'Bad X value'); { SKIP}
          */
        }
        else
          lastSide = null;

        sidePtr = AffSideList[sidePtr].Next;
      }

      /*Some debugging code useful for tracking down issues when the 1 or 2 empty triangles
     
        that may get created as a part of resolving affected triangles to not get used as expected
       if Assigned(SUCCSuccLastTriangle) then
    begin
      Assert(Not Assigned(SUCCSuccLastTriangle.Vertex[1]), 'SUCCSuccLastTriangle appears to point to non-null vertices');
      { SKIP}
      FreeAndNil(SUCCSuccLastTriangle);
      end;

      if Assigned(SuccLastTriangle) then
        begin
      Assert(Not Assigned(SuccLastTriangle.Vertex[1]), 'SuccLastTriangle appears to point to non-null vertices');
      { SKIP}
      FreeAndNil(SuccLastTriangle);
      end;

      Assert(not assigned(SuccLastTriangle) and not assigned(SUCCSuccLastTriangle));
      */
    }

    protected void InitLists(Triangle firstTri)
    {
      NumAffSides = 0;

      // Add a dummy node at the head of the affected node list that contains
      // the passed first triangle

      NumAffected = 1;
      AffectedList[0].Tri = firstTri;

      NumCandidates = 0;
    }

    /// <summary>
    /// Add theCoord in currentTri to model. 
    /// </summary>
    /// <param name="theCoord"></param>
    /// <param name="currentTri"></param>
    /// <param name="status"></param>
    /// <param name="forming"></param>
    protected void AddCoordToModel(TriVertex theCoord, Triangle currentTri, StatusType[] status, bool forming)
    {
      bool notAffected; // put the triangle into the candidate list, but note that it cannot be affected 

      InitLists(currentTri);

      // add first triangle's neighbours to the candidate list
      for (int j = 0; j < 3; j++)
      {
        if (forming)
          notAffected = false;
        else
          notAffected = CrossingStaticSide(currentTri, j) && !StatusOverrides(currentTri, j, status);
        AddCandidate(currentTri.Neighbours[j], notAffected);
      }

      // go through the candidate list - if a triangle is influenced by the new
      // coord, move the triangle to the affected list, and add its neighbours
      // to the candidate list 

      int CandidateIdx = 0;
      while (CandidateIdx < NumCandidates)
      {
        if (!CandidateList[CandidateIdx].NotAffected &&
            Influenced(CandidateList[CandidateIdx].Tri, theCoord))
        {
          // remove from candidate list and add to affected list
          if (NumAffected >= AffectedList.Length)
            Array.Resize(ref AffectedList, AffectedList.Length + 1000);
          NumAffected++;

          AffectedList[NumAffected - 1] = CandidateList[CandidateIdx];
          CandidateList[CandidateIdx].Tri = null;

          // add the current candidate triangle's neighbours to the candidate list 
          //with FAffectedList[FNumAffected - 1] do
          for (int j = 0; j < 3; j++)
          {
            if (forming)
              notAffected = false;
            else
              notAffected = CrossingStaticSide(AffectedList[NumAffected - 1].Tri, j);
            AddCandidate(AffectedList[NumAffected - 1].Tri.Neighbours[j], notAffected);
          }
        }

        CandidateIdx++;
      }

      // Note: Candidate list will have holes in it, remove them, but do not remove
      //       the first candidate from the list
      int Diff = 0;
      for (int i = 0; i < NumCandidates; i++)
        if (CandidateList[i].Tri == null)
          Diff++;
        else if (Diff > 0)
          CandidateList[i - Diff] = CandidateList[i];

      NumCandidates -= Diff;

      // make appropriate changes to all the affected triangles
      AlterAffected(theCoord, forming, status);
    }

    /// <summary>
    /// IncorporateCoord adds a vertex into the TIN by locating the triangle
    /// the coordinate lies in then adding the vertex to the model
    /// </summary>
    /// <param name="theCoord"></param>
    /// <param name="currentTri"></param>
    /// <param name="forming"></param>
    /// <returns></returns>
    protected bool IncorporateCoord(TriVertex theCoord, ref Triangle currentTri, bool forming)
    {
      // Todo: This is never modified from it's null state???????????
      StatusType[] status = new StatusType[2]; 

      currentTri = LocateTriangle2(theCoord, currentTri, !forming, false);
      if (currentTri == null)
      {
        TIN.SaveToFile(@"c:\TinProgress.ttm", true);
        return false;
      }

      //  if (!forming)
      //    CheckStatus(coordBuffer, checkBuffer, status);

      AddCoordToModel(theCoord, currentTri, status, forming);

      Debug.Assert(SuccLastTriangle != null, "Not all created triangles used.");

      return true;
    }

    public void InitialiseInitialTriangles(double MinX, double MinY,
      double MaxX, double MaxY,
      double VertexHeight,
      out Triangle TLTri, out Triangle BRTri)
    {
      // Create the four corner vertices
      TriVertex TL = AddVertex(MinX, MaxY, VertexHeight);
      TriVertex TR = AddVertex(MaxX, MaxY, VertexHeight);
      TriVertex BL = AddVertex(MinX, MinY, VertexHeight);
      TriVertex BR = AddVertex(MaxX, MinY, VertexHeight);

      // Add the two starting triangles.
      AddTriangle(TL, TR, BL);
      AddTriangle(BL, TR, BR);

      TLTri = TIN.Triangles[0];
      BRTri = TIN.Triangles[1];

      // Ensure their neighbours are correct
      TLTri.Neighbours[1] = BRTri;
      BRTri.Neighbours[0] = TLTri;

      TIN.Triangles.NumberTriangles();
    }

    /// <summary>
    /// IncorporateCoordIntoTriangle adds a vertex into the given triangle
    /// already existing in the model
    /// </summary>
    /// <param name="theCoord"></param>
    /// <param name="tri"></param>
    /// <returns></returns>
    public bool IncorporateCoordIntoTriangle(TriVertex theCoord, Triangle tri)
    {
      StatusType[] status = new StatusType[2]; // TODO This is never initialised from its null state

      AddCoordToModel(theCoord, tri, status, true);

      if (SuccLastTriangle != null)
        Debug.Assert(false, "Not all created triangles used.");

      return true;
    }

    /// <summary>
    /// Builds a triangle mesh from the vertices contained in the TIN model. All
    /// triangles are discarded. When using BuildTINMesh, it is sufficent to
    /// populate the vertex list (using AddVertex()) with all the vertices
    /// requiring tinning, then calling BuildTINMesh. All points to be tinned must be
    /// added prior to calling BuilTINMesh. To build TIN surfaces incrementally
    /// use InitialiseInitialTriangles() and IncorporateCoordIntoTriangle() to
    /// construct the TIN mesh.
    /// </summary>
    /// <returns></returns>
    public bool BuildTINMesh()
    {
      Triangle CurrentTri = null;

      double MinElevation = 0, MaxElevation = 0;

      // Clear all existing triangles
      TIN.Triangles.Clear();
      TIN.Triangles.Capacity = TIN.Vertices.Count * 2;

      // Update the physical extents of the TIN model
      TTMHeader LocalHeader = TIN.Header;
      TIN.Vertices.GetLimits(ref LocalHeader.MinimumEasting, ref LocalHeader.MinimumNorthing, ref MinElevation,
        ref LocalHeader.MaximumEasting, ref LocalHeader.MaximumNorthing, ref MaxElevation);
      TIN.Header = LocalHeader;

      // Set up the spatial index used during surface generation:
      //  FIndex.Initialise(FTIN);

      // Set up the initial state to insert the coordinates into
      MakeMinimumBoundingRectangle(out TriVertex TL, out TriVertex TR, out TriVertex BL, out TriVertex BR);
      CreateInitialTriangles(TL, TR, BL, BR);

      // Make sure all the vertices are numbered correctly, along with the 4
      // MBR vertices
      TIN.Vertices.NumberVertices();

      // Subtract the origin from the vertices to preserve numeric precision
      for (int I = 0; I < TIN.Vertices.Count; I++)
      {
        TIN.Vertices[I].X -= TIN.Header.MinimumEasting;
        TIN.Vertices[I].Y -= TIN.Header.MinimumNorthing;
      }

      // Iterate through all the vertices adding them to the surface
      DateTime StartTime= DateTime.Now;
      //LocateTriangle2StepCount:= 0;
      //CandidateListReassignmentSteps:= 0;
      //AddCandidateCalls:= 0;
      //GetNodeTestCount:= 0;
      //InListTestCount:= 0;
      //SurfaceWalkOverflowCount:= 0;

      // Don't read the 4 vertices we added for the bounding rectangle tris
      int MaxRealVertex = TIN.Vertices.Count - 1 - 4;
      for (int I = 0; I < MaxRealVertex; I++)
        if (!IncorporateCoord(TIN.Vertices[I], ref CurrentTri, true))
          return false;

      DateTime FinishTime = DateTime.Now;
      try
      {
        // TODO: Readd when logging available
        /*SIGLogMessage.PublishNoODS(Self,
                                   format('Coordinate incorporation took %s to process %d vertices into %d triangles using %d surface traverse steps and %d candidate reassignments and %d AddCandidate() calls and %d GetNode() tests and %d InList() tests and %d quadtree lookups' + ' at a rate of %.3f vertices/sec, encountering %d surface walk overflows', { SKIP}
                                          [FormatDateTime('nn:ss.zzz', FinishTime - StartTime), {SKIP}
                                           FTIN.Vertices.Count,
                                           FTIN.Triangles.Count,
                                           LocateTriangle2StepCount,
                                           CandidateListReassignmentSteps,
                                           AddCandidateCalls,
                                           GetNodeTestCount,
                                           InListTestCount,
                                           QuadTreeLookUpTestCount,
                                           FTIN.Vertices.Count / ((FinishTime - StartTime) * (24*60*60)),
                                           SurfaceWalkOverflowCount]),
                                   slmcMessage);*/
      }
      catch
      {
        // swallow exception due to logging...
      }

  // Add the origin back to the vertex positions to re-translate than back to their correct positions
    for (int I = 0; I < TIN.Vertices.Count; I++)
      {
        TIN.Vertices[I].X += TIN.Header.MinimumEasting;
        TIN.Vertices[I].Y += TIN.Header.MinimumNorthing;
      }

      RemoveCornerTriangles(MaxRealVertex + 1);
      return true;
    }
  }
}

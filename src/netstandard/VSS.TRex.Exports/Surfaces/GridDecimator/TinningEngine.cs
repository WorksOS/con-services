using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Geometry;

namespace VSS.TRex.Exports.Surfaces.GridDecimator
{
  public class TinningEngine
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(nameof(TinningEngine));

    public TrimbleTINModel TIN { get; set; }

    /// <summary>
    /// List of triangles that need adjusting to the new coord
    /// </summary>
    private TriListNode[] affectedList;

    /// <summary>
    /// List of triangles that may need adjusting }
    /// </summary>
    private TriListNode[] candidateList;

    private AffSideNode[] affSideList;

    private int numAffected { get; set; }
    private int numCandidates { get; set; }
    private int numAffSides { get; set; }

    /// <summary>
    /// succLastTriangle and succSuccLastTriangle represent the one or two new
    /// triangles that will be created when processing triangles affected by the
    /// DeLauney insertion of a new vertex into the surface.
    /// </summary>
    private Triangle succLastTriangle { get; set; }

    /// <summary>
    /// succLastTriangle and succSuccLastTriangle represent the one or two new
    /// triangles that will be created when processing triangles affected by the
    /// DeLauney insertion of a new vertex into the surface.
    /// </summary>
    private Triangle succSuccLastTriangle { get; set; }

    private long surfaceWalkOverflowCount;

    public TinningEngine()
    {
      TIN = new TrimbleTINModel();

      affSideList = new AffSideNode[1000];
      candidateList = new TriListNode[1000];
      affectedList = new TriListNode[1000];
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
      Triangle Result;
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
      Triangle thirdSide)
    {
      Debug.Assert(tri != firstSide && tri != secondSide && tri != thirdSide, "Triangle cannot be its own neighbour");

      tri.Vertices[0] = firstCoord;
      tri.Vertices[1] = secondCoord;
      tri.Vertices[2] = thirdCoord;
      tri.Neighbours[0] = firstSide;
      tri.Neighbours[1] = secondSide;
      tri.Neighbours[2] = thirdSide;

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

    protected Triangle NewTriangle(TriVertex coord1, TriVertex coord2, TriVertex coord3, Triangle side1, Triangle side2, Triangle side3)
    {
      Triangle result;

      if (succLastTriangle != null)
      {
        result = succLastTriangle;

        if (coord1.X == coord2.X && coord1.Y == coord2.Y ||
            coord2.X == coord3.X && coord2.Y == coord3.Y ||
            coord3.X == coord1.X && coord3.Y == coord1.Y)
        {
          Debug.Assert(false, "Coordinates for new triangle are not unique");
        }

        result.Vertices[0] = coord1;
        result.Vertices[1] = coord2;
        result.Vertices[2] = coord3;

        TIN.Triangles.Add(result);

        TriangleAdded(result);
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

    protected void UpdateNeighbour(Triangle theTri, TriVertex thePoint, Triangle newTri)
    {
      if (theTri != null)
        for (int j = 0; j < 3; j++)
          if (theTri.Vertices[j] == thePoint)
            theTri.Neighbours[XYZ.PrevSide(j)] = newTri;
    }

    /// <summary>
    /// Creates a pait of new triangles to use...
    /// </summary>
    protected Triangle GetLastSide()
    {
      Triangle AddEmptyTriangle(int Offset)
      {
        Triangle Result = TIN.Triangles.CreateTriangle(null, null, null);
        Result.Tag = TIN.Triangles.Count + Offset;

        return Result;
      }

      succLastTriangle = AddEmptyTriangle(1);
      succSuccLastTriangle = AddEmptyTriangle(2);

      return succSuccLastTriangle;
    }

    /// <summary>
    /// sidePtr points to what will be side[2] of the next new/updated triangle.
    /// Return the triangle that will be its side[1].  Generally it will be
    /// next triangle in the affectedPtr list.If we have run out of these,
    /// It will be the first or second of the to-be-created triangles.If the
    /// currently being made triangle is the last one, return firstTri. }
    /// </summary>
    /// <param name="sideIndex"></param>
    /// <param name="index"></param>
    /// <param name="firstTri"></param>
    /// <returns></returns>
    protected Triangle GetNextSide(int sideIndex, int index, Triangle firstTri)
    {
      if (affSideList[sideIndex].Next == -1)
        return firstTri;

      if (index + 1 < numAffected)
        return affectedList[index + 1].Tri;

      if (index >= numAffected)
      {
        return succSuccLastTriangle;
      }

      return succLastTriangle;
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
    /// affectedTris - ie the boundary between affectedTris and surroundingTris
    /// </summary>
    protected void MakeAffSideList()
    {
      void LengthenAffSideList()
      {
        Array.Resize(ref affSideList, affSideList.Length + 1000);

        // Reset all the next pointers in the affected side list
        for (int I = 0; I < numAffSides - 2; I++)
          affSideList[I].Next = I + 1;

        affSideList[numAffSides - 1].Next = -1;
      }

      //get Index to point to affectedTri with a side on the edge of the
      //  polygon.Set side[j] of this triangle to the edge side 
      int Index = 0;
      bool Found = false;
      int SideIdx = 0;

      while (Index < numAffected)
      {
        for (int J = 0; J < 3; J++)
        {
          if (affectedList[Index].Tri.Neighbours[J] == null || InList(affectedList[Index].Tri.Neighbours[J], candidateList, numCandidates))
          {
            SideIdx = J;
            Found = true;
            break;
          }
        }

        if (Found)
          break;

        Index++;
      }

      if (numAffSides >= affSideList.Length)
        LengthenAffSideList();
      numAffSides++;

      affSideList[numAffSides - 1].point = affectedList[Index].Tri.Vertices[SideIdx];
      affSideList[numAffSides - 1].tri = affectedList[Index].Tri;
      affSideList[numAffSides - 1].side = affectedList[Index].Tri.Neighbours[SideIdx];

      affSideList[numAffSides - 1].Next = -1;
      if (numAffSides > 1)
        affSideList[numAffSides - 2].Next = numAffSides - 1;

      // set nextPoint to the clockwise - most point of this side - the next side
      // in the list will share this point }
      TriVertex nextPoint = affectedList[Index].Tri.Vertices[XYZ.NextSide(SideIdx)];

      bool done = false;
      while (!done)
      {
        // if the next side(clockwise) of the triPtr triangle adjoins a triangle
        // within the polygon move triPtr to the adjoining triangle
        //  this is repeated until we find a side on the edge of the polygon
        //  that shares nextPoint 

        int TestIndex = getNode(affectedList, affectedList[Index].Tri.Neighbours[XYZ.NextSide(SideIdx)], numAffected);
        if (TestIndex != -1)
        {
          Index = TestIndex;
          while (TestIndex != -1)
          {
            for (int j = 0; j < 3; j++)
              if (affectedList[Index].Tri.Vertices[j] == nextPoint)
              {
                SideIdx = j;
                break;
              }

            TestIndex = getNode(affectedList, affectedList[Index].Tri.Neighbours[SideIdx], numAffected);
            if (TestIndex != -1)
              Index = TestIndex;
          }
        }
        else
          SideIdx = XYZ.NextSide(SideIdx);

        if (numAffSides >= affSideList.Length)
          LengthenAffSideList();
        numAffSides++;

        affSideList[numAffSides - 1].tri = affectedList[Index].Tri;
        affSideList[numAffSides - 1].point = affectedList[Index].Tri.Vertices[SideIdx];
        affSideList[numAffSides - 1].side = affectedList[Index].Tri.Neighbours[SideIdx];

        affSideList[numAffSides - 1].Next = -1;
        if (numAffSides > 1)
          affSideList[numAffSides - 2].Next = numAffSides - 1;

        nextPoint = affectedList[Index].Tri.Vertices[XYZ.NextSide(SideIdx)];

        // keep going until we reach the start point 
        done = nextPoint == affSideList[0].point;
      }
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

      for (int I = 0; I < numAffected; I++)
        if (affectedList[I].Tri == theTri)
          return;

      for (int I = 0; I < numCandidates; I++)
        if (candidateList[I].Tri == theTri)
          return;

      if (numCandidates >= candidateList.Length)
        Array.Resize(ref candidateList, candidateList.Length + 1000);
      numCandidates++;

      candidateList[numCandidates - 1].Tri = theTri;
      candidateList[numCandidates - 1].NotAffected = unAffected;
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
    /// <param name="checkIt"></param>
    /// <returns></returns>
    protected Triangle LocateTriangle2(TriVertex coord, Triangle lastTri, bool checkIt)
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
          surfaceWalkOverflowCount++;
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

    public Triangle AddTriangle(TriVertex V1, TriVertex V2, TriVertex V3)
    {
      // Add the triangle and assign its tag member to be its position in the list (ie: it will be the last one)
      Triangle tri = TIN.Triangles.AddTriangle(V1, V2, V3);
      tri.Tag = TIN.Triangles.Count;

      TriangleAdded(tri);

      return tri;
    }

    /// <summary>
    /// Add four coords to the model, which form the minimum bounding rectangle
    /// about the selected points.Return these.
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
      br = AddVertex(TIN.Header.MaximumEasting + aBit, TIN.Header.MinimumNorthing - aBit, 0);
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
    protected void AlterAffected(TriVertex newCoord)
    {
      // determine the edge of the polygon -fill affSideList to describe this makeAffSideList
      MakeAffSideList();

      if (numAffected == 0) // Nothing to do!
        return;

      int sidePtr = 0;
      int AffectedIdx = 0;

      // First triangle in the affected list 
      Triangle firstTri = affectedList[0].Tri;

      // triangle on side 1 of new/ updated triangle - generally the previously made / updated one
      Triangle lastSide = GetLastSide();

      // Make two new triangle
      while (sidePtr != -1) //  more triangles to update/ make 
      {
        TriVertex nextPoint = affSideList[sidePtr].Next == -1 ? affSideList[0].point : affSideList[affSideList[sidePtr].Next].point;

        // triangle on side 2 of new/ updated triangle - generally the next to be made / updated
        Triangle nextSide = GetNextSide(sidePtr, AffectedIdx, firstTri);

        if (AffectedIdx >= numAffected)
        {
          // the last one or two triangles will be new, rather than updated 
          Debug.Assert(succLastTriangle != null, "Cannot use a nil triangle for a new triangle");
          lastSide = NewTriangle(nextPoint,
            newCoord,
            affSideList[sidePtr].point,
            nextSide,
            lastSide,
            affSideList[sidePtr].side);

          succLastTriangle = succSuccLastTriangle;
          succSuccLastTriangle = null;
        }
        else
        {
          //  update affectedPtr^.tri triangle 
          makeUpdatedTriangle(affectedList[AffectedIdx].Tri,
            nextPoint,
            newCoord,
            affSideList[sidePtr].point,
            nextSide,
            lastSide,
            affSideList[sidePtr].side);

          lastSide = affectedList[AffectedIdx].Tri;
          AffectedIdx++;
        }

        // tell the triangles neighbour that the triangle has become its new neighbour 
        UpdateNeighbour(affSideList[sidePtr].side, affSideList[sidePtr].point, lastSide);

        sidePtr = affSideList[sidePtr].Next;
      }

      /*//Some debugging code useful for tracking down issues when the 1 or 2 empty triangles     
        //that may get created as a part of resolving affected triangles to not get used as expected
       if (Assigned(SUCCSuccLastTriangle))
      Assert(Not Assigned(SUCCSuccLastTriangle.Vertex[1]), 'SUCCSuccLastTriangle appears to point to non-null vertices');

      if (Assigned(succLastTriangle))
      Assert(Not Assigned(succLastTriangle.Vertex[1]), 'succLastTriangle appears to point to non-null vertices');

      Assert(not assigned(succLastTriangle) and not assigned(SUCCSuccLastTriangle));
      */
    }

    protected void InitLists(Triangle firstTri)
    {
      numAffSides = 0;

      // Add a dummy node at the head of the affected node list that contains the passed first triangle
      numAffected = 1;
      affectedList[0].Tri = firstTri;

      numCandidates = 0;
    }

    /// <summary>
    /// Add theCoord in currentTri to model. 
    /// </summary>
    /// <param name="theCoord"></param>
    /// <param name="currentTri"></param>
    protected void AddCoordToModel(TriVertex theCoord, Triangle currentTri)
    {
      InitLists(currentTri);

      // add first triangle's neighbours to the candidate list
      for (int j = 0; j < 3; j++)
      {
        // put the triangle into the candidate list, but note that it cannot be affected 
        AddCandidate(currentTri.Neighbours[j], false);
      }

      // go through the candidate list - if a triangle is influenced by the new
      // coord, move the triangle to the affected list, and add its neighbours
      // to the candidate list 

      int CandidateIdx = 0;
      while (CandidateIdx < numCandidates)
      {
        if (!candidateList[CandidateIdx].NotAffected &&
            Influenced(candidateList[CandidateIdx].Tri, theCoord))
        {
          // remove from candidate list and add to affected list
          if (numAffected >= affectedList.Length)
            Array.Resize(ref affectedList, affectedList.Length + 1000);
          numAffected++;

          affectedList[numAffected - 1] = candidateList[CandidateIdx];
          candidateList[CandidateIdx].Tri = null;

          // add the current candidate triangle's neighbours to the candidate list 
          for (int j = 0; j < 3; j++)
          {
            // put the triangle into the candidate list, but note that it cannot be affected 
            AddCandidate(affectedList[numAffected - 1].Tri.Neighbours[j], false);
          }
        }

        CandidateIdx++;
      }

      // Note: Candidate list will have holes in it, remove them, but do not remove the first candidate from the list
      int Diff = 0;
      for (int i = 1; i < numCandidates; i++)
      {
        if (candidateList[i].Tri == null)
          Diff++;
        else if (Diff > 0)
          candidateList[i - Diff] = candidateList[i];
      }

      numCandidates -= Diff;

      // make appropriate changes to all the affected triangles
      AlterAffected(theCoord);
    }

    /// <summary>
    /// IncorporateCoord adds a vertex into the TIN by locating the triangle
    /// the coordinate lies in then adding the vertex to the model
    /// </summary>
    /// <param name="theCoord"></param>
    /// <param name="currentTri"></param>
    /// <returns></returns>
    protected bool IncorporateCoord(TriVertex theCoord, ref Triangle currentTri)
    {
      currentTri = LocateTriangle2(theCoord, currentTri, false);
      if (currentTri == null)
      {
        TIN.SaveToFile(@"c:\TinProgress.ttm", true);
        return false;
      }

      AddCoordToModel(theCoord, currentTri);

      Debug.Assert(succLastTriangle != null, "Not all created triangles used.");

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
      AddCoordToModel(theCoord, tri);

      if (succLastTriangle != null)
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

      // Set up the initial state to insert the coordinates into
      MakeMinimumBoundingRectangle(out TriVertex TL, out TriVertex TR, out TriVertex BL, out TriVertex BR);
      CreateInitialTriangles(TL, TR, BL, BR);

      // Make sure all the vertices are numbered correctly, along with the 4 MBR vertices
      TIN.Vertices.NumberVertices();

      // Subtract the origin from the vertices to preserve numeric precision
      for (int I = 0; I < TIN.Vertices.Count; I++)
      {
        TIN.Vertices[I].X -= TIN.Header.MinimumEasting;
        TIN.Vertices[I].Y -= TIN.Header.MinimumNorthing;
      }

      // Iterate through all the vertices adding them to the surface
      DateTime StartTime = DateTime.Now;
      surfaceWalkOverflowCount = 0;

      // Don't read the 4 vertices we added for the bounding rectangle tris
      int MaxRealVertex = TIN.Vertices.Count - 1 - 4;
      for (int I = 0; I < MaxRealVertex; I++)
        if (!IncorporateCoord(TIN.Vertices[I], ref CurrentTri))
          return false;

      DateTime FinishTime = DateTime.Now;
      try
      {
        Log.LogInformation($"Coordinate incorporation took {FinishTime - StartTime} to process {TIN.Vertices.Count} vertices into {TIN.Triangles.Count} triangles " +
                           $"at a rate of {TIN.Vertices.Count / ((FinishTime - StartTime).TotalSeconds)} vertices/sec, encountering {surfaceWalkOverflowCount} surface walk overflows");
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

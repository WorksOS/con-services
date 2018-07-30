using System;
using System.Diagnostics;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Tests.Exports.Surfaces;
using VSS.TRex.Utilities;

/*
  This unit provides a grid decimator capable of turning very large scale grids
  of data into TIN format. It's design used a combination of the TINGEN Quad-Edge
  grid decimator and the SVO tinning engine based on the DQM tinning engine.

  To Use:
    1. Create a <TSVOICGridToTINDecimator> instance
    2. Set the Datastore property
    3. Set the decimation extents
    4. Set the tolerance required
    5. Call BuildMesh
    6. Extract the resulting TIN model.
 */

namespace VSS.TRex.Exports.Surfaces
{
  public class GridToTINDecimator
  {
    private double NullVertexHeight = -9999;
    private const int kMaxSeedIntervals = 500;
    private const int kDefaultSeedPointInterval = 40;

    /// <summary>
    /// TinningEngine encapsulates the knowledge of how to construct TIN surfaces
    /// using Delauney constraints from points/vertices
    /// </summary>
    private TinningEngine Engine;

    /// <summary>
    /// FDataStore is a reference to a client data store that contains the
    /// grid if point information we are creating the TIN surface from
    /// </summary>
    public GenericSubGridTree<float> DataStore = new GenericSubGridTree<float>();

    /// <summary>
    /// Tolerance represents the maximum acceptable difference between the height
    /// of a grid point and a triangle face in the TIN mesh.
    /// </summary>
    public double Tolerance = 0.0;

    /// <summary>
    /// PointLimit is the maximum number of points to insert into the tin
    /// </summary>
    public int PointLimit = 2000000;

    /// <summary>
    /// GridExtents records the extents of the grid information held in DataStore
    /// that will be converted into a TIN.
    /// </summary>
    public BoundingIntegerExtent2D GridCalcExtents = new BoundingIntegerExtent2D(0, 0, 0, 0);
    public BoundingIntegerExtent2D GridExtents = new BoundingIntegerExtent2D(0, 0, 0, 0);

    /// <summary>
    /// DecimationExtents contains the world coordinates of the region of the
    /// IC data store grid to be decimated into a TIN mesh.
    /// </summary>
    public BoundingWorldExtent3D DecimationExtents = new BoundingWorldExtent3D();

    /// <summary>
    /// Heap contains the heap of triangles that remain to be processed.
    /// </summary>
    private TINHeap Heap = new TINHeap(50000);

    /// IsUsed keeps track of which cells have been used in the construction of the TIN
    private SubGridTreeBitMask IsUsed = new SubGridTreeBitMask();

    /// <summary>
    /// Aborted is a flag that allows an external agency to abort the process
    /// </summary>
    public bool Aborted = false;

    /// <summary>
    /// GridOriginOffsetX and GridOriginOffsetY detail the offset from the
    /// cartesian coordinate system origin to the origin of the rectangle
    /// enclosing the grid cells that we are converting in to a TIN.
    /// We do this to improve numeric accuracy when creating the TIN mesh.
    /// The origin is expressed as a whole number of grid cells.
    /// </summary>
    public int GridOriginOffsetX;

    public int GridOriginOffsetY;

    /// <summary>
    /// Zplane is the plane derived from the three points of a triangle. Used
    /// when scanning grid points over the triangle to determine the best one
    /// to insert next.
    /// </summary>
    private Plane Zplane = new Plane();

    /// <summary>
    /// Candidate is a description of a candidate grid point being considered
    /// as the next grid point to be inserted
    /// </summary>
    private Candidate Candidate = new Candidate();

    /// <summary>
    /// v0, v1, v2 are height ordered references to the vertices in a triangle
    /// that is begin scanned
    /// </summary>
    private TriVertex v0 = null;
    private TriVertex v1 = null;
    private TriVertex v2 = null;

    /// <summary>
    /// ScanTri is a reference to the triangle being scanned
    /// </summary>
    private GridToTINTriangle ScanTri = null;

    private bool DontScanTriangles = false;

    private bool FoundGridData;

    private long ElevationCellValuesRetrieved = 0;
    private long ElevationCellValuesRetrievedAreNull = 0;
    private long ScanTriangleInvocations = 0;

    private long NumTrivialTrianglesPassedToScanWholeTriangleGeometry = 0;

    private int SeedPointInterval = kDefaultSeedPointInterval;

    /// <summary>
    /// XSeedIntervalStep and YSeedIntervalStep are the seed intervals
    /// used when seeding the original vertices into the TTM beign generated
    /// </summary>
    private int XSeedIntervalStep;
    private int YSeedIntervalStep;

    private int CachedInUseMaps_SubgridRow;
    private int InUse_MinXTriangleScanRange;
    private int Elevations_MinXTriangleScanRange;

    private InUseSubGridMap[] CachedInUseMaps;
    private CachedSubGridMap[] CachedElevationSubgrids;

    private double[] NullElevationsArray;
    private double[] Elevations;

    private long TriangleScanInvocationNumber;

    public DecimationResult BuildMeshFaultCode;

    private void SetDecimationExtents(BoundingWorldExtent3D value)
    {
      DecimationExtents = value;

      Debug.Assert(DataStore != null, "Cannot set decimation exents without a datastore");

      // Convert the world coordinate range into the grid cell range that it covers
      DataStore?.CalculateRegionGridCoverage(value, out GridCalcExtents);
    }

    private void AddCandidateToHeap()
    {
      if (Candidate.Import < Tolerance)
      {
        if (ScanTri.HeapIndex != GridToTINHeapNode.NOT_IN_HEAP)
        {
          Heap.Kill(ScanTri.HeapIndex);
          ScanTri.HeapIndex = GridToTINHeapNode.NOT_IN_HEAP;
        }

        //      {$IFDEF DEBUG}
        //      Heap.CheckListConsistency();
        //      {$ENDIF}
      }
      else
      {
        if (ScanTri.HeapIndex == GridToTINHeapNode.NOT_IN_HEAP)
        {
//          {$IFDEF DEBUG}
//          for I := 0 to Heap.Count - 1 do
//            begin
//              if (Heap[I].sx = Candidate.x) and (Heap[I].sy = Candidate.y) then
//                begin
//                  Engine.TIN.SaveToFile('C:\temp\GRID_ToTIN_Decimator_Assert_surface.ttm'); {SKIP}
//
//                  Debug.Assert(False, 'Adding candidate to heap with same insertion point'); {SKIP}
//                end;
//            end;
//          {$ENDIF}

          Heap.Insert(ScanTri, Candidate.Import);
        }
        else
          Heap.Update(ScanTri, Candidate.Import);

        //      {$IFDEF DEBUG}
        //      if Heap[ScanTri.HeapIndex].Tri <> ScanTri then
        //        Debug.Assert(False, 'Tri references different'); {SKIP}
        //      {$ENDIF}

        // Update the 3D grid position information in the new/updated heap node
        Heap[ScanTri.HeapIndex].sx = Candidate.X;
        Heap[ScanTri.HeapIndex].sy = Candidate.Y;
        Heap[ScanTri.HeapIndex].sz = Candidate.Z;

        //      {$IFDEF DEBUG}
        //      Heap.CheckListConsistency();
        //      {$ENDIF}
      }
    }

    private void InitialiseTriangleVertexOrdering()
    {
      v0 = ScanTri.Vertices[0];
      v1 = ScanTri.Vertices[1];
      v2 = ScanTri.Vertices[2];

      // Sort the three vertices in ascending Y order. The three compares and swaps
      // are more efficient than calling qsort to do it.
      if (v0.Y > v1.Z) MinMax.Swap(ref v0, ref v1);
      if (v1.Y > v2.Z) MinMax.Swap(ref v1, ref v2);
      if (v0.Y > v1.Z) MinMax.Swap(ref v0, ref v1);
    }

    protected void GetHeightForTriangleScan(int x, int y, bool spotElevationOnly, int numElevationsToScan, double[] elevations)
    {
      int TestX, TestY;
      GenericLeafSubGrid<float> CacheSubgrid;
      int CacheSubgridIndex = 0;

      void GetCacheElevationMap()
      {
        if (CachedElevationSubgrids[CacheSubgridIndex].TriangleScanInvocationNumber == TriangleScanInvocationNumber)
          CacheSubgrid = CachedElevationSubgrids[CacheSubgridIndex].SubGrid;
        else
        {
          CacheSubgrid = (GenericLeafSubGrid<float>) DataStore.LocateSubGridContaining((uint)TestX, (uint)TestY);
          CachedElevationSubgrids[CacheSubgridIndex].SubGrid = CacheSubgrid;
          CachedElevationSubgrids[CacheSubgridIndex].TriangleScanInvocationNumber = TriangleScanInvocationNumber;
        }
      }

      TestX = x + GridOriginOffsetX;
      TestY = y + GridOriginOffsetY;

      int SubGridX = TestX % SubGridTree.SubGridTreeDimension;
      int SubGridY = TestY % SubGridTree.SubGridTreeDimension;

      // Get the initial ExistanceBitMask from the bitmask cache
      if (spotElevationOnly)
        CacheSubgrid = (GenericLeafSubGrid<float>)DataStore.LocateSubGridContaining((uint)TestX, (uint)TestY);
      else
      {
        CacheSubgridIndex = TestX % SubGridTree.SubGridTreeDimension - Elevations_MinXTriangleScanRange % SubGridTree.SubGridTreeDimension;
        GetCacheElevationMap();
      }

      int elevationIndex = 0;
      while (numElevationsToScan > 0)
      {
        if (CacheSubgrid == null)
        {
          int NumValuesFromThisSubgrid = SubGridTree.SubGridTreeDimension - SubGridX;
          if (numElevationsToScan < NumValuesFromThisSubgrid)
            NumValuesFromThisSubgrid = numElevationsToScan;

          // Set all the elavations for cells in tihs subgrid to null 
          for (int i = 0; i < NumValuesFromThisSubgrid; i++)
            elevations[elevationIndex++] = Common.Consts.NullHeight;

          SubGridX += NumValuesFromThisSubgrid; // Move to next cell in scan line

          numElevationsToScan -= NumValuesFromThisSubgrid;
          ElevationCellValuesRetrieved += NumValuesFromThisSubgrid;

          ElevationCellValuesRetrievedAreNull += NumValuesFromThisSubgrid;
        }
        else
        {
          float elev = CacheSubgrid.Items[SubGridX, SubGridY];

          if (elev == Common.Consts.NullHeight)
          {
            Elevations[elevationIndex++] = NullVertexHeight;
            ElevationCellValuesRetrievedAreNull++;
          }
          else
            Elevations[elevationIndex++] = elev;

          SubGridX++; // Move to next cell in scan line

          numElevationsToScan--;
          ElevationCellValuesRetrieved++;
        }

        // Get next subgrid if necessary
        if (SubGridX == SubGridTree.SubGridTreeDimension && numElevationsToScan > 0 && !spotElevationOnly)
        {
          TestX += SubGridTree.SubGridTreeDimension;
          CacheSubgridIndex++;
          GetCacheElevationMap();
          SubGridX = 0;
        }
      }
    }

    protected void scan_triangle_line(int _y, double x1, double x2, ref int NumImportUpdates)
    {
      int _x;
      SubGridTreeLeafBitmapSubGrid ExistanceBitMask;
      bool YOrdinateIsASeedVertexRow;
      int BitMaskCacheSubgridIndex;

      void GetInUseExistanceMap()
      {
        if (CachedInUseMaps[BitMaskCacheSubgridIndex].TriangleScanInvocationNumber == TriangleScanInvocationNumber)
          ExistanceBitMask = CachedInUseMaps[BitMaskCacheSubgridIndex].InUseMap;
        else
        {
          ExistanceBitMask = (SubGridTreeLeafBitmapSubGrid)IsUsed.ConstructPathToCell((uint)_x, (uint)_y, SubGridPathConstructionType.ReturnExistingLeafOnly);
          CachedInUseMaps[BitMaskCacheSubgridIndex].InUseMap = ExistanceBitMask;
          CachedInUseMaps[BitMaskCacheSubgridIndex].TriangleScanInvocationNumber = TriangleScanInvocationNumber;
        }
      }

      int startx = (int)Math.Ceiling(Math.Min(x1, x2));
      int endx = (int)Math.Floor(Math.Min(x1, x2));

      if (startx > endx)
        return;

      double z0 = Zplane.Evaluate(startx, _y);
      double dz = Zplane.a;
      _x = startx;

      int BitMaskIndexX = _x & SubGridTree.SubGridLocalKeyMask;
      int BitMaskIndexY = _x & SubGridTree.SubGridLocalKeyMask;

      // Get the initial ExistanceBitMask from the bitmask cache
      BitMaskCacheSubgridIndex = startx / SubGridTree.SubGridTreeDimension - InUse_MinXTriangleScanRange / SubGridTree.SubGridTreeDimension;
      GetInUseExistanceMap();

      YOrdinateIsASeedVertexRow = _y % YSeedIntervalStep == 0;

      int NumElevationsToScan = endx - startx + 1;
      if (NumElevationsToScan > Elevations.Length)
        Array.Resize(ref Elevations, NumElevationsToScan + 100);

      GetHeightForTriangleScan(startx, _y, false, NumElevationsToScan, Elevations /*@(FElevations[0])*/);

      for (int I = 0; I < NumElevationsToScan - 1; I++)
      {
        if (ExistanceBitMask == null ||
            !ExistanceBitMask.Bits.BitSet(BitMaskIndexX, BitMaskIndexY))
        {
          double _z = Elevations[I];
          double Diff = Math.Abs(_z - z0);

          // Note: There is a final 'IsUsed' check to make sure this point is not one of the
          // initial seed points placed into the TIN surface
          if (Diff > Candidate.Import)
          if (!YOrdinateIsASeedVertexRow || (_x % XSeedIntervalStep != 0))
          {
            Candidate.Import = Diff;
            Candidate.X = _x;
            Candidate.Y = _y;
            Candidate.Z = _z;
            NumImportUpdates++;
          }
        }

        z0 = z0 + dz;
        _x++;

        BitMaskIndexX++;
        if (BitMaskIndexX == SubGridTree.SubGridTreeDimension && I != (NumElevationsToScan - 1))
        {
          BitMaskCacheSubgridIndex++;
          GetInUseExistanceMap();
          BitMaskIndexX = 0;
        }
      }
    }

    /// <summary>
    /// ScanWholeTriangleGeometry iterates over all grid points in a triangle
    /// and for each point determines is suitablility as the next grid point to
    /// add in to the triangle.
    /// </summary>
    /// <returns></returns>
    private bool ScanWholeTriangleGeometry()
    {
      int starty, endy;

      void InitCacheIndices()
      {
        TriangleScanInvocationNumber++;
        CachedInUseMaps_SubgridRow = (starty + GridOriginOffsetY) / SubGridTree.SubGridTreeDimension;
      }

      void UpdateCacheIndices(int y)
      {
        if (CachedInUseMaps_SubgridRow != (y + GridOriginOffsetY) / SubGridTree.SubGridTreeDimension)
        {
          CachedInUseMaps_SubgridRow = (y + GridOriginOffsetY) / SubGridTree.SubGridTreeDimension;
          TriangleScanInvocationNumber++;
        }
      }

      int NumImportUpdates = 0;
      bool ProcessedCentralRow = false;

      // Determine the entire x coordinate range that will be traversed in this triangle
      InUse_MinXTriangleScanRange = (int) Math.Truncate(Math.Ceiling(Math.Min(Math.Min(v0.X, v1.X), v2.X)));

      Elevations_MinXTriangleScanRange = InUse_MinXTriangleScanRange + GridOriginOffsetX;

      double x1 = v0.X;
      double x2 = v0.X;

      double dx2 = (v2.X - v0.X) / (1.0 * (v2.Y - v0.Y));

      if (v1.Y != v0.Y)
      {
        double dx1 = (v1.X - v0.X) / (1.0 * (v1.Y - v0.Y));

        starty = (int) Math.Round(v0.Y);
        endy = (int) Math.Round(v1.Y);

        InitCacheIndices();

        ProcessedCentralRow = true;

        for (int y = starty; y <= endy; y++)
        {
          UpdateCacheIndices(y);
          scan_triangle_line(y, x1, x2, ref NumImportUpdates);
          x1 = x1 + dx1;
          x2 = x2 + dx2;
        }

        x2 = x2 - dx2;
      }

      /////////////////////////////

      if (v2.Y != v1.Y)
      {
        double dx1 = (v2.X - v1.X) / (1.0 * (v2.Y - v1.Y));
        x1 = v1.X;

        starty = (int) Math.Round(v1.Y);
        endy = (int) Math.Round(v2.Y);

        InitCacheIndices();

        if (ProcessedCentralRow)
        {
          starty++;
          x1 = x1 + dx1;
          x2 = x2 + dx2;
        }

        for (int y = starty; y <= endy; y++)
        {
          UpdateCacheIndices(y);
          scan_triangle_line(y, x1, x2, ref NumImportUpdates);
          x1 = x1 + dx1;
          x2 = x2 + dx2;
        }

      }

      bool Result = NumImportUpdates > 0;
      if (Result && (NumImportUpdates == 1))
      {
        // Check that the triangle is not flat (null) and the 'Import' value is not the null value
        if (v0.Z == NullVertexHeight && v0.Z == v1.Z && v1.Z == v2.Z && Candidate.Import == Math.Abs(NullVertexHeight))
          Result = false; // Yes it is, discard this triangle
      }

      return Result;
    }

    private void TriangleAdded(Triangle tri)
    {
      // Every new triangle must be added to the heap for consideration in processing
      // ... but only if it is not a nil triangle

      ScanTri = (GridToTINTriangle)tri;

      if (ScanTri.Vertices[0] != null)
        Update();

      ScanTri = null;
    }

    private void TriangleUpdated(Triangle tri)
    {
      // Every updated triangle must be rescanned to update its candidate grid point
      ScanTri = (GridToTINTriangle) tri;

      if (ScanTri.Vertices[0] != null)
        Update();

      ScanTri = null;
    }

    private void Update() => ScanTriangle(false);

    private double MaxError() => Heap?.Top.Import ?? 0;

    /// <summary>
    /// GreedyIndsert pulls the triangle with the greatest error from the top of the
    /// heap and insert the grid position within that triangle that represents
    /// that error into the triangle.
    /// </summary>
    /// <returns></returns>
    private bool greedyInsert()
    {
      GridToTINHeapNode HeapNode = Heap.Extract();

      if (HeapNode == null)
        return false;

      Select(HeapNode.sx, HeapNode.sy, HeapNode.sz, HeapNode.Tri);

      return true;
    }

    /// <summary>
    /// Select 'selects' the given position into the TIN model
    /// </summary>
    protected void Select(int sx, int sy, double sz, GridToTINTriangle T)
    {
      IsUsed[(uint)sx, (uint)sy] = true;

      // Add the new position as a vertex into the model and add that new vertex to the mesh
      Engine.IncorporateCoordIntoTriangle(Engine.AddVertex(sx, sy, sz), T);
    }

    private void CreateDecimationState()
    {
      //FIsUsed = TSubGridTreeBitMask.Create(FDataStore.NumLevels, FDataStore.CellSize);

      Engine = new TinningEngine
      {
        TriangleAdded = TriangleAdded,
        TriangleUpdated = TriangleUpdated
      };
      Engine.TIN.Triangles.CreateTriangleFunc = (_v0, _v1, _v2) => new GridToTINTriangle(_v0, _v1, _v2);

      // Allocate an appropriately sized cache array for the InUse and elevation subgrids
      CachedElevationSubgrids = new CachedSubGridMap[1000];
      CachedInUseMaps = new InUseSubGridMap[1000];

      for (int I = 0; I < CachedElevationSubgrids.Length; I++)
      {
        CachedElevationSubgrids[I].SubGrid = null;
        CachedElevationSubgrids[I].TriangleScanInvocationNumber = 0;
        CachedInUseMaps[I].InUseMap = null;
        CachedInUseMaps[I].TriangleScanInvocationNumber = 0;
      }

      Elevations = new double[0];

      TriangleScanInvocationNumber = 0;
    }

    public void Refresh() => CreateDecimationState();

    public GridToTINDecimator(GenericSubGridTree<float> dataStore)
    {
      DataStore = dataStore;

      CreateDecimationState();
    }

    public bool BuildMesh()
    {
      DateTime StartTime;

      int NXSeedIntervals, NYSeedIntervals;

      int _X, _Y;
      double[] _Z = new double[1];
      double XFactor, YFactor;

      void ConstructSeedTriangleMesh1()
      {
        DontScanTriangles = true;
        try
        {
          // First create the two new triangles
          BoundingIntegerExtent2D InitialTINExtents = new BoundingIntegerExtent2D(GridExtents.MinX - 100,
            GridExtents.MinY - 100,
            GridExtents.MaxX + 100,
            GridExtents.MaxY + 100);

          Engine.InitialiseInitialTriangles(InitialTINExtents.MinX,
            InitialTINExtents.MinY,
            InitialTINExtents.MaxX,
            InitialTINExtents.MaxY,
            NullVertexHeight,
            out Triangle TLTri, out Triangle BRTri);

          // Seed the model with a small number of points from the grid

          NXSeedIntervals = Math.Min(GridExtents.SizeX / SeedPointInterval + 1, kMaxSeedIntervals);
          NYSeedIntervals = Math.Min(GridExtents.SizeY / SeedPointInterval + 1, kMaxSeedIntervals);

          XFactor = GridExtents.SizeX / NXSeedIntervals;
          YFactor = GridExtents.SizeY / NYSeedIntervals;

          // TODO Readd when logging available
          //SIGLogMessage.PublishNoODS(Self, Format('Creating %d seed positions into extent of area being TINNed using a seed interval of %d and max seed intervals of %d', 
          //  [(NXSeedIntervals + 1) * (NYSeedIntervals + 1), FSeedPointInterval, kMaxSeedIntervals]), slmcMessage);
          if (NXSeedIntervals > 0 && NYSeedIntervals > 0)
          {
            // Insert the seeds as new grid points into the relevant triangles
            for (int I = 0; I <= NXSeedIntervals; I++)
            for (int J = 0; J <= NYSeedIntervals; J++)
            {
              _X = (int) Math.Round(I * XFactor);
              _Y = (int) Math.Round(J * YFactor);

              // Find the triangle this seed point lies in
              Triangle tri = Engine.TIN.GetTriangleAtPoint(_X, _Y, out double _);

              if (tri == null)
              {
                // Probably exactly on a triangle edge - don't worry about it
                // Dont bother to insert this point
                continue;
              }

              GetHeightForTriangleScan(_X, _Y, true, 1, _Z);

              // Insert the seed point into it
              Engine.IncorporateCoordIntoTriangle(Engine.AddVertex(_X, _Y, _Z[0]), tri);
            }

          }
        }
        finally
        {
          DontScanTriangles = false;
        }

        // Now scan all the triangles we have made
        for (int I = 0; I < Engine.TIN.Triangles.Count; I++)
        {
          ScanTri = (GridToTINTriangle) Engine.TIN.Triangles[I];
          ScanTriangle(false);
        }

        ScanTri = null;
      }

      void ConstructSeedTriangleMesh2()
      {
        int _X1, _Y1;
        int _X2, _Y2;
        int _X3, _Y3;
        int _X4, _Y4;
        double[] _Z1 = new double[1];
        double[] _Z2 = new double[1];
        double[] _Z3 = new double[1];
        double[] _Z4 = new double[1];

        void PerformTriangleAdditionToHeap()
        {
          InitialiseTriangleVertexOrdering();

          Candidate = new Candidate();

          BoundingIntegerExtent2D Extents = new BoundingIntegerExtent2D((int)Math.Round(GridOriginOffsetX + Math.Min(Math.Min(v0.X, v1.X), v2.X)),
            (int)Math.Round(GridOriginOffsetY + v0.Y),
            (int)Math.Round(GridOriginOffsetX + Math.Max(Math.Max(v0.X, v1.X), v2.X)),
            (int)Math.Round(GridOriginOffsetY + v2.Y));

          // Determine if there is any data in the grid to be processed
          FoundGridData = false;
          DataStore.Root.ScanSubGrids(Extents, 
            leaf =>
            {
              FoundGridData = true;
              return false; // Terminate the scan
            }, //OnProcessLeafSubgrid, 
            NodeSubGrid => SubGridProcessNodeSubGridResult.OK);
            

          // If there is some grid data in the triangle area then add the triangle to
          // the heap with a default large error (ie: don't waste time scanning it
          // we know we will be scanning it again later).
          if (FoundGridData)
            ScanTriangle(true);
        }

        DontScanTriangles = true;
        try
        {
          // Seed the model with a small number of points from the grid

          NXSeedIntervals = Math.Min(GridExtents.SizeX / SeedPointInterval + 1, kMaxSeedIntervals);
          NYSeedIntervals = Math.Min(GridExtents.SizeY / SeedPointInterval + 1, kMaxSeedIntervals);

          XSeedIntervalStep = GridExtents.SizeX / NXSeedIntervals + 1;
          YSeedIntervalStep = GridExtents.SizeY / NYSeedIntervals + 1;

          /// TODO readd when loggin available
          //SIGLogMessage.PublishNoODS(Self, Format('Creating %d seed positions into extent of area being TINNed using X/Y seed intervals of %d/%d', 
           // [(NXSeedIntervals + 1) * (NYSeedIntervals + 1), FXSeedIntervalStep, FYSeedIntervalStep]), slmcMessage);

          // Insert the seeds as new grid points into the relevant triangles
          if (NXSeedIntervals > 0 && NYSeedIntervals > 0)
            for (int I = 0; I <= NXSeedIntervals + 1; I++)
            for (int J = 0; J <= NYSeedIntervals + 1; J++)
            {
              _X1 = I * XSeedIntervalStep;
              _Y1 = J * YSeedIntervalStep;

              _X2 = (I + 1) * XSeedIntervalStep;
              _Y2 = J * YSeedIntervalStep;

              _X3 = I * XSeedIntervalStep;
              _Y3 = (J + 1) * YSeedIntervalStep;

              _X4 = (I + 1) * XSeedIntervalStep;
              _Y4 = (J + 1) * YSeedIntervalStep;

              GetHeightForTriangleScan(_X1, _Y1, true, 1, _Z1);
              GetHeightForTriangleScan(_X2, _Y2, true, 1, _Z2);
              GetHeightForTriangleScan(_X3, _Y3, true, 1, _Z3);
              GetHeightForTriangleScan(_X4, _Y4, true, 1, _Z4);

              // Create both triangles across the panel keeping the vertex order as clockwise
              ScanTri = (GridToTINTriangle)Engine.TIN.Triangles.AddTriangle(Engine.TIN.Vertices.AddPoint(_X1, _Y1, _Z1[0]), Engine.TIN.Vertices.AddPoint(_X2, _Y2, _Z2[0]), Engine.TIN.Vertices.AddPoint(_X3, _Y3, _Z3[0]));
              PerformTriangleAdditionToHeap();
              ScanTri = (GridToTINTriangle)Engine.TIN.Triangles.AddTriangle(Engine.TIN.Vertices.AddPoint(_X3, _Y3, _Z3[0]), Engine.TIN.Vertices.AddPoint(_X2, _Y2, _Z2[0]), Engine.TIN.Vertices.AddPoint(_X4, _Y4, _Z4[0]));
              PerformTriangleAdditionToHeap();
            }
        }
        finally
        {
          DontScanTriangles = false;
        }

        Engine.TIN.BuildTriangleLinks();
        Engine.TIN.BuildEdgeList();

        ScanTri = null;
      }

      BuildMeshFaultCode = DecimationResult.NoError;
      StartTime = DateTime.Now;

      Debug.Assert(DataStore != null, "No Datastor");

      if (DataStore == null)
      {
        BuildMeshFaultCode = DecimationResult.NoDataStore;
        return false;
      }

      Debug.Assert(Engine.TIN.Vertices.Count == 0, "TIN engine mesh is not empty");

      if (Engine.TIN.Vertices.Count != 0)
      {
        BuildMeshFaultCode = DecimationResult.NoData;
        return false;
      }

      if (GridCalcExtents.Area() <= 0)
      {
        BuildMeshFaultCode = DecimationResult.NoData;
        return false;
      }

      NullVertexHeight = DecimationExtents.MinZ - (100 * Tolerance) - 10000.0;

      // Alter the grid extents to that its origin is at (0, 0)
      GridExtents = GridCalcExtents;

      // Determine the grid origin offset.
      GridOriginOffsetX = GridExtents.MinX;
      GridOriginOffsetY = GridExtents.MinY;

      GridExtents.Offset(-GridOriginOffsetX, -GridOriginOffsetY);

      // Initialise the TIN engine to receive the vertices
      Engine.TIN.Vertices.InitPointSearch(GridExtents.MinX - 100,
        GridExtents.MinY - 100,
        GridExtents.MaxX + 100,
        GridExtents.MaxY + 100,
        PointLimit * 3);
      ConstructSeedTriangleMesh2();

      //  {$IFDEF DEBUG}
      //  FTinningEngine.TIN.SaveToFile('C:\Temp\IC_GRIDToTIN_AfterInitialIntervalTesselation.ttm', True); {SKIP}
      //  {$ENDIF}

      // Add grid points into the triangle with the largest error (importance)
      // until there are no triangles whose error fall outside of the tolerance

//      {$IFDEF DEBUG}
//      SIGLogMessage.PublishNoODS(Self, Format('GridToTIN: Mesh size = %d tris. Heap Size = %d. MaxError = %.5f',
//                                          [Engine.TIN.Triangles.Count,
//                                           Heap.Count, MaxError]), slmcDebug);
//      {$ENDIF}

      while (MaxError() > Tolerance && Engine.TIN.Vertices.Count < PointLimit && !Aborted)
      {
        if (!greedyInsert()) // Some error has occurred
        {
          BuildMeshFaultCode = DecimationResult.Unknown;
          return false;
        }

        /*
         * {$IFDEF DEBUG}
         
        if (FTinningEngine.TIN.Triangles.Count MOD 200000) = 0 then
          begin
            SIGLogMessage.PublishNoODS(Self, Format('GridToTIN: Mesh size = %d tris. Heap Size = %d. MaxError = %.5f', {SKIP}
                                                    [FTinningEngine.TIN.Triangles.Count,
                                                     Fheap.Count, MaxError]), slmcDebug);
            SIGLogMessage.PublishNoODS(Self, Format('ElevationCellValuesRetrieved = %d', [FElevationCellValuesRetrieved]), slmcDebug);
            SIGLogMessage.PublishNoODS(Self, Format('ElevationCellValuesRetrievedAreNull = %d', [FElevationCellValuesRetrievedAreNull]), slmcDebug);
            SIGLogMessage.PublishNoODS(Self, Format('FScanTriangleInvocations = %d', [FScanTriangleInvocations]), slmcDebug);
            SIGLogMessage.PublishNoODS(Self, Format('FNumTrivialTrianglesPassedToScanWholeTriangleGeometry = %d', [FNumTrivialTrianglesPassedToScanWholeTriangleGeometry]), slmcDebug);
          end;
        {$ENDIF}
        */
      }

      /*
  SIGLogMessage.PublishNoODS(Self, Format('Finished: Mesh = %d tris. Heap = %d. Initial Tolerance = %.3f', {SKIP}
                                          [FTinningEngine.TIN.Triangles.Count, Fheap.Count, FTolerance]), slmcDebug);
  SIGLogMessage.PublishNoODS(Self, Format('ElevationCellValuesRetrieved = %d', [FElevationCellValuesRetrieved]), slmcDebug);
  SIGLogMessage.PublishNoODS(Self, Format('ElevationCellValuesRetrievedAreNull = %d', [FElevationCellValuesRetrievedAreNull]), slmcDebug);
  SIGLogMessage.PublishNoODS(Self, Format('FScanTriangleInvocations = %d', [FScanTriangleInvocations]), slmcDebug);
  SIGLogMessage.PublishNoODS(Self, Format('FNumTrivialTrianglesPassedToScanWholeTriangleGeometry = %d', [FNumTrivialTrianglesPassedToScanWholeTriangleGeometry]), slmcDebug);
  */

      if (Engine.TIN.Vertices.Count >= PointLimit)
      {
        // Todo: Readd logging
        //SIGLogMessage.PublishNoODS(Self, Format('TIN construction aborted after adding a maximum of %d vertices to the surface being constructed.',  
        //[FTinningEngine.TIN.Vertices.Count]), slmcMessage);
        BuildMeshFaultCode = DecimationResult.TrianglesExceeded;
        return false;
      }

//{$IFDEF DEBUG}
//  AssignFile(F, 'c:\temp\IC_GRIDToTIN_HeapRemainder.txt'); {SKIP}
//  try
//    Rewrite(F);
//    for I  := 0 to FHeap.Count - 1 do
//      Writeln(F, format('HeapNode: %d, MaxError = %.3f', [I, FHeap[I].Import]));
//  finally
//    CloseFile(F);
//  end;
//{$ENDIF}

//{$IFDEF DEBUG}
//  FTinningEngine.TIN.SaveToFile('C:\Temp\IC_GRIDToTIN_BeforeNullRemoval.ttm', True); {SKIP}
//{$ENDIF}

      // We have now constructed the surface - now remove all the vertices at elevation NullVertexHeight
      RemoveCornerAndNullTriangles();

      // Now convert all vertex coordinates to their true world positions
      // Calculate the vertex offset to take into account the local grid origin
      // offset and the fact the vertex heights are measured at the center of each cell
      double VertexOffsetX = (DataStore.CellSize * (GridOriginOffsetX - DataStore.IndexOriginOffset)) + (DataStore.CellSize / 2);
      double VertexOffsetY = (DataStore.CellSize * (GridOriginOffsetY - DataStore.IndexOriginOffset)) + (DataStore.CellSize / 2);

      for (int I = 0; I < Engine.TIN.Vertices.Count - 1; I++)
      {
        Engine.TIN.Vertices[I].X = Engine.TIN.Vertices[I].X * DataStore.CellSize + VertexOffsetX;
        Engine.TIN.Vertices[I].Y = Engine.TIN.Vertices[I].Y * DataStore.CellSize + VertexOffsetX;
      }

      // TODO readd when logging available
      //SIGLogMessage.PublishNoODS(Self,
      //                       Format('GridToTIN: %d tris. Seed interval = %d. Time = %s', {SKIP}
      //                              [FTinningEngine.TIN.Triangles.Count,
      //                               FSeedPointInterval,
      //                               FormatDateTime('nn:ss.zzz', Now - StartTime)]),
      //                       slmcMessage);

      return true;
    }

    private void ScanTriangle(bool force)
    {
      bool ValidTriangle;

      if (DontScanTriangles && !force)
        return;

      ScanTriangleInvocations++;

      Candidate = new Candidate();

      InitialiseTriangleVertexOrdering();

      try
      {
        ValidTriangle = Zplane.Init(ScanTri.Vertices[0], ScanTri.Vertices[1], ScanTri.Vertices[2]);
      }
      catch (Exception E)
      {
        // TODO readd when logging available
        //SIGLogMessage.PublishNoODS(Self, Format('Exception ''%s'' in FZplane.Init. Vertices are V1=%s, V2=%s, V3=%s',
        //  [E.Message, ScanTri.Vertices[0].AsText, ScanTri.Vertices[1].AsText, ScanTri.Vertices[2].AsText]), slmcException);
        ValidTriangle = false;
      }

      // ***** Check how many trivial triangles (one cell) make it here. These could be
      // pruned before ever being placed into the heap.....

      if (ValidTriangle && ScanWholeTriangleGeometry())
        // We have now found the appropriate candidate point.
        AddCandidateToHeap();
      else if (ScanTri.HeapIndex != GridToTINHeapNode.NOT_IN_HEAP)
      {
        Heap.Kill(ScanTri.HeapIndex);
        ScanTri.HeapIndex = GridToTINHeapNode.NOT_IN_HEAP;
      }
    }

    private void RemoveCornerAndNullTriangles()
    {
      TrimbleTINModel TIN = Engine.TIN;

      double NullHeightLimit = DecimationExtents.MinZ - 0.001;

      // Remove all triangles that have a null height vertex as a corner
      for (int I = TIN.Triangles.Count - 1; I >= 0; I--)
      {
        if (TIN.Triangles[I].Vertices[0].Z < NullHeightLimit ||
            TIN.Triangles[I].Vertices[1].Z < NullHeightLimit ||
            TIN.Triangles[I].Vertices[2].Z < NullHeightLimit)
          TIN.Triangles.RemoveTriangle(TIN.Triangles[I]);
      }

      TIN.Triangles.Pack();
      TIN.Triangles.NumberTriangles();

      // Remove all vertices with null heights
      for (int I = TIN.Vertices.Count - 1; I >= 0; I--)
        if (TIN.Vertices[I].Z < NullHeightLimit)
          TIN.Vertices[I] = null;

      TIN.Vertices.Pack();
      TIN.Vertices.NumberVertices();
    }
  }
}

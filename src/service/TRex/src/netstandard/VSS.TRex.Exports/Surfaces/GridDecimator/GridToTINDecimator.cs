using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Tests.Exports.Surfaces.GridDecimator;
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

namespace VSS.TRex.Exports.Surfaces.GridDecimator
{
  public class GridToTINDecimator
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    private const double kNullVertexHeight = -9999;
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
    public GenericSubGridTree<float, GenericLeafSubGrid_Float> DataStore { set; get; }

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

    /// <summary>
    /// IsUsed keeps track of which cells have been used in the construction of the TIN
    /// </summary>
    private SubGridTreeBitMask IsUsed;

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
    private Candidate Candidate; 

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

    private double[] Elevations;

    private long TriangleScanInvocationNumber;

    public DecimationResult BuildMeshFaultCode;

    public double NullVertexHeight;

    public void SetDecimationExtents(BoundingWorldExtent3D value)
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
      if (v0.Y > v1.Y) MinMax.Swap(ref v0, ref v1);
      if (v1.Y > v2.Y) MinMax.Swap(ref v1, ref v2);
      if (v0.Y > v1.Y) MinMax.Swap(ref v0, ref v1);
    }

    protected void GetHeightForTriangleScan(int x, int y, bool spotElevationOnly, int numElevationsToScan, double[] elevations)
    {
      int TestX, TestY;
      GenericLeafSubGrid_Float CacheSubgrid;
      int CacheSubgridIndex = 0;

      void GetCacheElevationMap()
      {
        if (CachedElevationSubgrids[CacheSubgridIndex].TriangleScanInvocationNumber == TriangleScanInvocationNumber)
          CacheSubgrid = CachedElevationSubgrids[CacheSubgridIndex].SubGrid;
        else
        {
          CacheSubgrid = (GenericLeafSubGrid_Float) DataStore.LocateSubGridContaining((uint)TestX, (uint)TestY);
          CachedElevationSubgrids[CacheSubgridIndex].SubGrid = CacheSubgrid;
          CachedElevationSubgrids[CacheSubgridIndex].TriangleScanInvocationNumber = TriangleScanInvocationNumber;
        }
      }

      TestX = x + GridOriginOffsetX;
      TestY = y + GridOriginOffsetY;

      int SubGridX = TestX % SubGridTreeConsts.SubGridTreeDimension;
      int SubGridY = TestY % SubGridTreeConsts.SubGridTreeDimension;

      // Get the initial ExistenceBitMask from the bitmask cache
      if (spotElevationOnly)
        CacheSubgrid = (GenericLeafSubGrid_Float)DataStore.LocateSubGridContaining((uint)TestX, (uint)TestY);
      else
      {
        CacheSubgridIndex = TestX / SubGridTreeConsts.SubGridTreeDimension - Elevations_MinXTriangleScanRange / SubGridTreeConsts.SubGridTreeDimension;
        GetCacheElevationMap();
      }

      int elevationIndex = 0;
      while (numElevationsToScan > 0)
      {
        if (CacheSubgrid == null)
        {
          int NumValuesFromThisSubgrid = SubGridTreeConsts.SubGridTreeDimension - SubGridX;
          if (numElevationsToScan < NumValuesFromThisSubgrid)
            NumValuesFromThisSubgrid = numElevationsToScan;

          // Set all the elavations for cells in this subgrid to null 
          for (int i = 0; i < NumValuesFromThisSubgrid; i++)
            elevations[elevationIndex++] = NullVertexHeight;

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
            elevations[elevationIndex++] = NullVertexHeight;
            ElevationCellValuesRetrievedAreNull++;
          }
          else
            elevations[elevationIndex++] = elev;

          SubGridX++; // Move to next cell in scan line

          numElevationsToScan--;
          ElevationCellValuesRetrieved++;
        }

        // Get next subgrid if necessary
        if (SubGridX == SubGridTreeConsts.SubGridTreeDimension && numElevationsToScan > 0 && !spotElevationOnly)
        {
          TestX += SubGridTreeConsts.SubGridTreeDimension;
          CacheSubgridIndex++;
          GetCacheElevationMap();
          SubGridX = 0;
        }
      }
    }

    protected void Scan_triangle_line(int _y, double x1, double x2, ref int NumImportUpdates)
    {
      int _x;
      SubGridTreeLeafBitmapSubGrid ExistenceBitMask;
      bool YOrdinateIsASeedVertexRow;
      int BitMaskCacheSubgridIndex;

      void GetInUseExistenceMap()
      {
        if (CachedInUseMaps[BitMaskCacheSubgridIndex].TriangleScanInvocationNumber == TriangleScanInvocationNumber)
          ExistenceBitMask = CachedInUseMaps[BitMaskCacheSubgridIndex].InUseMap;
        else
        {
          ExistenceBitMask = (SubGridTreeLeafBitmapSubGrid)IsUsed.ConstructPathToCell((uint)_x, (uint)_y, SubGridPathConstructionType.ReturnExistingLeafOnly);
          CachedInUseMaps[BitMaskCacheSubgridIndex].InUseMap = ExistenceBitMask;
          CachedInUseMaps[BitMaskCacheSubgridIndex].TriangleScanInvocationNumber = TriangleScanInvocationNumber;
        }
      }

      int startx = (int)Math.Ceiling(Math.Min(x1, x2));
      int endx = (int)Math.Floor(Math.Max(x1, x2));

      if (startx > endx)
        return;

      double z0 = Zplane.Evaluate(startx, _y);
      double dz = Zplane.a;
      _x = startx;

      int BitMaskIndexX = _x & SubGridTreeConsts.SubGridLocalKeyMask;
      int BitMaskIndexY = _x & SubGridTreeConsts.SubGridLocalKeyMask;

      // Get the initial ExistenceBitMask from the bitmask cache
      BitMaskCacheSubgridIndex = startx / SubGridTreeConsts.SubGridTreeDimension - InUse_MinXTriangleScanRange / SubGridTreeConsts.SubGridTreeDimension;
      GetInUseExistenceMap();

      YOrdinateIsASeedVertexRow = _y % YSeedIntervalStep == 0;

      int NumElevationsToScan = endx - startx + 1;
      if (NumElevationsToScan > Elevations.Length)
        Array.Resize(ref Elevations, NumElevationsToScan + 100);

      GetHeightForTriangleScan(startx, _y, false, NumElevationsToScan, Elevations);

      for (int I = 0; I < NumElevationsToScan; I++)
      {
        if (ExistenceBitMask == null || !ExistenceBitMask.Bits.BitSet(BitMaskIndexX, BitMaskIndexY))
        {
          double _z = Elevations[I];
          double Diff = Math.Abs(_z - z0);

          // Note: There is a final 'IsUsed' check to make sure this point is not one of the
          // initial seed points placed into the TIN surface
          if (Diff > Candidate.Import)
          {
            if (!YOrdinateIsASeedVertexRow || _x % XSeedIntervalStep != 0)
            {
              Candidate.Import = Diff;
              Candidate.X = _x;
              Candidate.Y = _y;
              Candidate.Z = _z;
              NumImportUpdates++;
            }
          }
        }

        z0 += dz;
        _x++;

        BitMaskIndexX++;
        if (BitMaskIndexX == SubGridTreeConsts.SubGridTreeDimension && I != NumElevationsToScan - 1)
        {
          BitMaskCacheSubgridIndex++;
          GetInUseExistenceMap();
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
        CachedInUseMaps_SubgridRow = (starty + GridOriginOffsetY) / SubGridTreeConsts.SubGridTreeDimension;
      }

      void UpdateCacheIndices(int y)
      {
        if (CachedInUseMaps_SubgridRow != (y + GridOriginOffsetY) / SubGridTreeConsts.SubGridTreeDimension)
        {
          CachedInUseMaps_SubgridRow = (y + GridOriginOffsetY) / SubGridTreeConsts.SubGridTreeDimension;
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
          Scan_triangle_line(y, x1, x2, ref NumImportUpdates);
          x1 += dx1;
          x2 += dx2;
        }

        x2 -= dx2;
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
          x1 += dx1;
          x2 += dx2;
        }

        for (int y = starty; y <= endy; y++)
        {
          UpdateCacheIndices(y);
          Scan_triangle_line(y, x1, x2, ref NumImportUpdates);
          x1 += dx1;
          x2 += dx2;
        }
      }

      bool Result = NumImportUpdates > 0;
      if (Result && NumImportUpdates == 1)
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

    private double MaxError() => Heap.Top?.Import ?? 0;

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
      IsUsed = new SubGridTreeBitMask(DataStore.NumLevels, DataStore.CellSize);

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

    public GridToTINDecimator(GenericSubGridTree<float, GenericLeafSubGrid_Float> dataStore)
    {
      DataStore = dataStore;

      CreateDecimationState();
    }

    void ConstructSeedTriangleMesh()
    {
      double[] _Z1 = new double[1];
      double[] _Z2 = new double[1];
      double[] _Z3 = new double[1];
      double[] _Z4 = new double[1];

      void PerformTriangleAdditionToHeap()
      {
        InitialiseTriangleVertexOrdering();

        Candidate = new Candidate(int.MinValue);

        BoundingIntegerExtent2D Extents = new BoundingIntegerExtent2D((int)Math.Round(GridOriginOffsetX + Math.Min(Math.Min(v0.X, v1.X), v2.X)),
          (int)Math.Round(GridOriginOffsetY + v0.Y),
          (int)Math.Round(GridOriginOffsetX + Math.Max(Math.Max(v0.X, v1.X), v2.X)),
          (int)Math.Round(GridOriginOffsetY + v2.Y));

        // Determine if there is any data in the grid to be processed
        bool FoundGridData = false;
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

        int NXSeedIntervals = Math.Min(GridExtents.SizeX / SeedPointInterval + 1, kMaxSeedIntervals);
        int NYSeedIntervals = Math.Min(GridExtents.SizeY / SeedPointInterval + 1, kMaxSeedIntervals);

        XSeedIntervalStep = GridExtents.SizeX / NXSeedIntervals + 1;
        YSeedIntervalStep = GridExtents.SizeY / NYSeedIntervals + 1;

        Log.LogInformation($"Creating {(NXSeedIntervals + 1) * (NYSeedIntervals + 1)} seed positions into extent of area being TINNed using X/Y seed intervals of {XSeedIntervalStep}/{YSeedIntervalStep}");

        // Insert the seeds as new grid points into the relevant triangles
        if (NXSeedIntervals > 0 && NYSeedIntervals > 0)
          for (int I = 0; I <= NXSeedIntervals + 1; I++)
          {
            for (int J = 0; J <= NYSeedIntervals + 1; J++)
            {
              int _X1 = I * XSeedIntervalStep;
              int _Y1 = J * YSeedIntervalStep;

              int _X2 = (I + 1) * XSeedIntervalStep;
              int _Y2 = J * YSeedIntervalStep;

              int _X3 = I * XSeedIntervalStep;
              int _Y3 = (J + 1) * YSeedIntervalStep;

              int _X4 = (I + 1) * XSeedIntervalStep;
              int _Y4 = (J + 1) * YSeedIntervalStep;

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
      }
      finally
      {
        DontScanTriangles = false;
      }

      Engine.TIN.BuildTriangleLinks();
      Engine.TIN.BuildEdgeList();

      ScanTri = null;
    }

    public bool BuildMesh()
    {
      BuildMeshFaultCode = DecimationResult.NoError;
      DateTime StartTime = DateTime.Now;

      Debug.Assert(DataStore != null, "No Datastor");

      if (DataStore == null)
      {
        BuildMeshFaultCode = DecimationResult.NoDataStore;
        return false;
      }

      Debug.Assert(Engine.TIN.Vertices.Count == 0, "TIN engine mesh is not empty");

      if (Engine.TIN.Vertices.Count != 0)
      {
        BuildMeshFaultCode = DecimationResult.DestinationTINNotEmpty;
        return false;
      }

      if (GridCalcExtents.Area() <= 0)
      {
        BuildMeshFaultCode = DecimationResult.NoData;
        return false;
      }

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

      NullVertexHeight = DecimationExtents.MinZ - 100 * Tolerance - 10000.0;

      ConstructSeedTriangleMesh();

      //  {$IFDEF DEBUG}
      //  FTinningEngine.TIN.SaveToFile('C:\Temp\IC_GRIDToTIN_AfterInitialIntervalTesselation.ttm', True); {SKIP}
      //  {$ENDIF}

      // Add grid points into the triangle with the largest error (importance)
      // until there are no triangles whose error fall outside of the tolerance

      Log.LogDebug($"Finished: Mesh size = {Engine.TIN.Triangles.Count} tris. Heap size = {Heap.Count}. Max error = {MaxError()}");

      while (MaxError() > Tolerance && Engine.TIN.Vertices.Count < PointLimit && !Aborted)
      {
        if (!greedyInsert()) // Some error has occurred
        {
          BuildMeshFaultCode = DecimationResult.Unknown;
          return false;
        }

        Log.LogDebug($"Finished: Mesh size = {Engine.TIN.Triangles.Count} tris. Heap size = {Heap.Count}. Max error = {MaxError()}");
        Log.LogDebug($"ElevationCellValuesRetrieved = {ElevationCellValuesRetrieved}");
        Log.LogDebug($"ElevationCellValuesRetrievedAreNull = {ElevationCellValuesRetrievedAreNull}");
        Log.LogDebug($"FScanTriangleInvocations = {ScanTriangleInvocations}");
        Log.LogDebug($"FNumTrivialTrianglesPassedToScanWholeTriangleGeometry = {NumTrivialTrianglesPassedToScanWholeTriangleGeometry}");
      }

      Log.LogInformation($"Finished: Mesh = {Engine.TIN.Triangles.Count} tris. Heap = {Heap.Count}. Initial Tolerance = {Tolerance}");
      Log.LogInformation($"ElevationCellValuesRetrieved = {ElevationCellValuesRetrieved}");
      Log.LogInformation($"ElevationCellValuesRetrievedAreNull = {ElevationCellValuesRetrievedAreNull}");
      Log.LogInformation($"FScanTriangleInvocations = {ScanTriangleInvocations}");
      Log.LogInformation($"FNumTrivialTrianglesPassedToScanWholeTriangleGeometry = {NumTrivialTrianglesPassedToScanWholeTriangleGeometry}");
  
      if (Engine.TIN.Vertices.Count >= PointLimit)
      {
        Log.LogInformation($"TIN construction aborted after adding a maximum of {Engine.TIN.Vertices.Count} vertices to the surface being constructed.");
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
      double VertexOffsetX = DataStore.CellSize * (GridOriginOffsetX - DataStore.IndexOriginOffset) + DataStore.CellSize / 2;
      double VertexOffsetY = DataStore.CellSize * (GridOriginOffsetY - DataStore.IndexOriginOffset) + DataStore.CellSize / 2;

      for (int I = 0; I < Engine.TIN.Vertices.Count; I++)
      {
        Engine.TIN.Vertices[I].X = Engine.TIN.Vertices[I].X * DataStore.CellSize + VertexOffsetX;
        Engine.TIN.Vertices[I].Y = Engine.TIN.Vertices[I].Y * DataStore.CellSize + VertexOffsetY;
      }

      Log.LogInformation($"GridToTIN: {Engine.TIN.Triangles.Count} tris. Seed interval = {SeedPointInterval}. Time = {DateTime.Now - StartTime}");

      return true;
    }

    private void ScanTriangle(bool force)
    {
      bool ValidTriangle;

      if (DontScanTriangles && !force)
        return;

      ScanTriangleInvocations++;

      Candidate = new Candidate(int.MinValue);

      InitialiseTriangleVertexOrdering();

      try
      {
        ValidTriangle = Zplane.Init(ScanTri.Vertices[0], ScanTri.Vertices[1], ScanTri.Vertices[2]);
      }
      catch (Exception E)
      {
        Log.LogError($"Exception in FZplane.Init. Vertices are V1={ScanTri.Vertices[0]}, V2={ScanTri.Vertices[0]}, V3={ScanTri.Vertices[2]}", E);
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
      for (int I = 0; I < TIN.Triangles.Count; I++)
      {
        if (TIN.Triangles[I].Vertices[0].Z < NullHeightLimit ||
            TIN.Triangles[I].Vertices[1].Z < NullHeightLimit ||
            TIN.Triangles[I].Vertices[2].Z < NullHeightLimit)
        {
          //Debug.Assert(TIN.Triangles[I].Tag - 1 == I, "Tag and triangle index inconsistent");
          TIN.Triangles.RemoveTriangle(TIN.Triangles[I]);
        }
      }

      TIN.Triangles.Pack();
      TIN.Triangles.NumberTriangles();

      // Remove all vertices with null heights
      for (int I = 0; I < TIN.Vertices.Count; I++)
        if (TIN.Vertices[I].Z < NullHeightLimit)
          TIN.Vertices[I] = null;

      TIN.Vertices.Pack();
      TIN.Vertices.NumberVertices();
    }

    /// <summary>
    /// Returns the TIN constructed from the decimated grid dat
    /// </summary>
    /// <returns></returns>
    public TrimbleTINModel GetTIN() => Engine.TIN;
  }
}

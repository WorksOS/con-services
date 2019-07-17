using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.QuantizedMesh.MeshUtils;
using VSS.TRex.QuantizedMesh.Models;
using VSS.TRex.Types;

namespace VSS.TRex.QuantizedMesh.GridFabric
{
  public class QMTileGenerator
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<QMTileGenerator>();

    /// <summary>
    /// FDataStore is a reference to a client data store that contains the
    /// grid if point information we are creating the TIN surface from
    /// </summary>
    private GenericSubGridTree<float, GenericLeafSubGrid_Float> DataStore { get; }
    /// <summary>
    /// IsUsed keeps track of which cells have been used in the construction of the TIN
    /// </summary>
    private SubGridTreeBitMask IsUsed;

    public RequestErrorStatus BuildTileFaultCode = RequestErrorStatus.Unknown;

    public int GridSize { get; set; }
    public double MinHeight { get; set; }
    public double MaxHeight { get; set; }
    public string DEMLocation { get; set; }

    /// <summary>
    /// Controls value assigned to no data cells
    /// </summary>
    public float NoDataElevation = 0;
    /// <summary>
    /// Tolerance represents the maximum acceptable difference between the height
    /// of a grid point and a triangle face in the TIN mesh.
    /// </summary>
    public double Tolerance = 0.0;

    /// <summary>
    /// Resulting quantized mesh tile
    /// </summary>
    public byte[] QMTile = null;

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

    public void SetDecimationExtents(BoundingWorldExtent3D value)
    {
      DecimationExtents = value;

      //Debug.Assert(DataStore != null, "Cannot set decimation extents without a data store");

      // Convert the world coordinate range into the grid cell range that it covers
      DataStore.CalculateRegionGridCoverage(value, out GridCalcExtents);
    }

    public QMTileGenerator(GenericSubGridTree<float, GenericLeafSubGrid_Float> dataStore)
    {
      DataStore = dataStore ?? throw new TRexTINException("No data store provided to decimator");

   //   CreateDecimationState();
    }

    /// <summary>
    /// Temporary solution to create a tile for testing
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="ecefPoints"></param>
    /// <returns></returns>
    private ElevationData GetFakeData(LLBoundingBox bbox, out Vector3[] ecefPoints)
    {
      ElevationData eData;
      /* bowling green
      LLBoundingBox data = new LLBoundingBox(172.632951, -43.548006, 172.633542, -43.547679);
      bool dataAvailable = data.East >= bbox.West && data.West <= bbox.East && data.North >= bbox.South && data.South <= bbox.North;
      LLBoundingBox data2 = new LLBoundingBox(-0.124629, 51.500745, 1.0, 51.6);
      bool dataAvailable2 = data2.East >= bbox.West && data2.West <= bbox.East && data2.North >= bbox.South && data2.South <= bbox.North;
      */

      LLBoundingBox data = new LLBoundingBox(172.575794, -43.549286, 172.581284, -43.545544);
      bool dataAvailable = data.East >= bbox.West && data.West <= bbox.East && data.North >= bbox.South && data.South <= bbox.North;

      int _gridSize;

      if (dataAvailable)
        _gridSize = GridSize;
      else
        _gridSize = 2; // small no data tile

      eData = new ElevationData(_gridSize); // elevation grid
      ecefPoints = new Vector3[_gridSize * _gridSize]; // ecef grid

      int PixelCount = _gridSize - 1;
      var yRange = bbox.North - bbox.South;
      var xRange = bbox.East - bbox.West;
      var xStep = xRange / PixelCount;
      var yStep = yRange / PixelCount;
      float minElev = float.PositiveInfinity;
      float maxElev = float.NegativeInfinity;

      Random rnd = new Random();
      int i = 0;
      //  float hgt = 0;
      var HgtMode = 2;
      // walk the grid and populate
      for (int y = 0; y < GridSize; y++)
      {
        for (int x = 0; x < GridSize; x++)
        {
          var lat = bbox.South + (y * yStep);
          var lon = bbox.West + (x * xStep);

          if (HgtMode == 1)
          {
            eData.Elev[i] = rnd.Next(1, 8000);
          }
          else if (HgtMode == 2) // this mode creates a 10m elevations when inside BB 
          {
            if ((lat >= -43.548006 && lat <= -43.547679) && (lon >= 172.632951 && lon <= 172.633542))
              eData.Elev[i] = (float)10.0;
            else
              eData.Elev[i] = NoDataElevation; 
          }

          if (eData.Elev[i] < minElev)
            minElev = eData.Elev[i];

          if (eData.Elev[i] > maxElev)
            maxElev = eData.Elev[i];

          // Make ecef point list for later header calculations
          ecefPoints[i] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(lon), Y = MapUtils.Deg2Rad(lat), Z = eData.Elev[i] });

          i++;
        }
      }

      eData.MaximumHeight = maxElev;
      eData.MinimumHeight = minElev;
      return eData;
    }

    private ElevationData GetData(LLBoundingBox bbox, out Vector3[] ecefPoints)
    {
      //todo get data from TRex
      //Boolean dataAvailable = true;
      // if ((lat >= -43.548006 & lat <= -43.547679) & (lon >= 172.632951 & lon <= 172.633542))
      ElevationData ed;

      LLBoundingBox data = new LLBoundingBox(172.632951, -43.548006, 172.633542, -43.547679);
      bool dataAvailable = data.East >= bbox.West && data.West <= bbox.East && data.North >= bbox.South && data.South <= bbox.North;
      LLBoundingBox data2 = new LLBoundingBox(-0.124629, 51.500745, 1.0, 51.6);
      bool dataAvailable2 = data2.East >= bbox.West && data2.West <= bbox.East && data2.North >= bbox.South && data2.South <= bbox.North;

      int _gridSize;

      if (dataAvailable | dataAvailable2)
        _gridSize = GridSize;
      else
        _gridSize = 2;
      ed = new ElevationData(_gridSize);
      ecefPoints = new Vector3[_gridSize * _gridSize];

      int PixelCount = _gridSize - 1;

      var yRange = bbox.North - bbox.South;
      var xRange = bbox.East - bbox.West;
      var xStep = xRange / PixelCount;
      var yStep = yRange / PixelCount;
      float minElev = float.PositiveInfinity;
      float maxElev = float.NegativeInfinity;
      Random rnd = new Random();
      int i = 0;
    //  float hgt = 0;
      var HgtMode = 2;
      for (int y = 0; y < GridSize; y++)
      {
        for (int x = 0; x < GridSize; x++)
        {
          var lat = bbox.South + (y * yStep);
          var lon = bbox.West + (x * xStep);


          if (HgtMode == 1)
          {
            ed.Elev[i] = rnd.Next(1, 8000);
          }
          else if (HgtMode == 2)
          {
            if ((lat >= -43.548006 & lat <= -43.547679) & (lon >= 172.632951 & lon <= 172.633542))
              ed.Elev[i] = (float)10.0;
            else if ((lat >= 51.500745 & lat <= 51.6) & (lon >= -0.124629 & lon <= 1.0))
              ed.Elev[i] = rnd.Next(1, 10);
            else
              ed.Elev[i] = 0;
          }

          if (ed.Elev[i] < minElev)
            minElev = ed.Elev[i];

          if (ed.Elev[i] > maxElev)
            maxElev = ed.Elev[i];

          // Make ecef point list for later header calculations
          ecefPoints[i] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(lon), Y = MapUtils.Deg2Rad(lat), Z = ed.Elev[i] });

          i++;
        }
      }

      ed.MaximumHeight = maxElev;
      ed.MinimumHeight = minElev;
      return ed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bbox"></param>
    /// <returns></returns>
    private ElevationData MakeTileDEM(LLBoundingBox bbox)
    {

      // Steps
      // Using bounding box get elevation data for tile
      // Calculate header info

      Vector3[] ecefPoints;
//      ElevationData eData = GetData(bbox, out ecefPoints); // todo get from TRex
      ElevationData eData = GetFakeData(bbox, out ecefPoints); // fake it for now

      // Work out bounding sphere for Cesium tile
      TileInfo tileInfo = new TileInfo();
      var tileHeader = tileInfo.CalculateHeaderInfo(ref ecefPoints);
      eData.CenterX = tileHeader.CenterX;
      eData.CenterY = tileHeader.CenterY;
      eData.CenterZ = tileHeader.CenterZ;
      eData.BoundingSphereCenterX = tileHeader.CenterX;
      eData.BoundingSphereCenterY = tileHeader.CenterY;
      eData.BoundingSphereCenterZ = tileHeader.CenterZ;
      eData.BoundingSphereRadius = tileHeader.BoundingSphereRadius;

      // Work out tile HorizonOcclusionPoint
      var hop = HorizonOcclusionPoint.FromPoints(ecefPoints, tileInfo.BoundingSphere);
      eData.HorizonOcclusionPointX = hop.X;
      eData.HorizonOcclusionPointY = hop.Y;
      eData.HorizonOcclusionPointZ = hop.Z;
    
      // Todo if lighting is needed do it here

      return eData;
    }

    /// <summary>
    /// Generate quantized mesh tile from the supplied grid
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public bool BuildQMTile(LLBoundingBox rect)
    {
      // Get elevations from Trex 
      try
      {
        var grid = MakeTileDEM(rect); //todo check for processing errors

        // Turn grid into a quantized mesh
        var vertices = MeshBuilder.MakeQuantizedMesh(ref grid);

        var tileHeader = new TerrainTileHeader()
        {
          MaximumHeight = grid.MaximumHeight,
          MinimumHeight = grid.MinimumHeight,
          CenterX = grid.CenterX,
          CenterY = grid.CenterY,
          CenterZ = grid.CenterZ,
          BoundingSphereCenterX = grid.BoundingSphereCenterX,
          BoundingSphereCenterY = grid.BoundingSphereCenterY,
          BoundingSphereCenterZ = grid.BoundingSphereCenterZ,
          BoundingSphereRadius = grid.BoundingSphereRadius,
          HorizonOcclusionPointX = grid.HorizonOcclusionPointX,
          HorizonOcclusionPointY = grid.HorizonOcclusionPointY,
          HorizonOcclusionPointZ = grid.HorizonOcclusionPointZ
        };

        // This class constructs a tile from the computed mesh
        var tileBuilder = new TileBuilder();
        QMTile = tileBuilder.MakeTile(vertices, tileHeader, MapUtils.GridSizeToTriangleCount(grid.GridSize), grid.GridSize);
        BuildTileFaultCode = RequestErrorStatus.OK;
      }
      catch (Exception E)
      {
        Log.LogError(E, "BuildQMTile: Exception:");
        return false;
      }
      return true;
    }


  }
}

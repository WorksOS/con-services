using System;
using System.Runtime.InteropServices;
using VSS.TRex.QuantizedMesh.MeshUtils;

namespace VSS.TRex.QuantizedMesh.Models
{
  /// <summary>
  /// Elevation data supplied by a DEM source in single precision including header information
  /// </summary>
  public struct ElevationData
  {
    /// <summary>
    /// Tile elevations for each vertice
    /// </summary>
    public float[] ElevGrid; 
    /// <summary>
    /// Earth Centered Earth Fixed coordinates used in tile header calculation
    /// </summary>
    public Vector3[] EcefPoints; 
    /// <summary>
    /// Square gridsize size to get number of vertices in a tile
    /// </summary>
    public int GridSize;
    /// <summary>
    /// The center of the tile in Earth-centered Fixed coordinates. X coordinate 
    /// </summary>
    public double CenterX;
    /// <summary>
    /// The center of the tile in Earth-centered Fixed coordinates. Y coordinate 
    /// </summary>
    public double CenterY;
    /// <summary>
    /// The center of the tile in Earth-centered Fixed coordinates. Z coordinate 
    /// </summary>
    public double CenterZ;

    // The minimum and maximum heights in the area covered by this tile.
    // The minimum may be lower and the maximum may be higher than
    // the height of any vertex in this tile in the case that the min/max vertex
    // was removed during mesh simplification, but these are the appropriate
    // values to use for analysis or visualization.
    public float MinimumHeight;
    public float MaximumHeight;

    // The tile’s bounding sphere.  The X,Y,Z coordinates are again expressed
    // in Earth-centered Fixed coordinates, and the radius is in meters.
    public double BoundingSphereCenterX;
    public double BoundingSphereCenterY;
    public double BoundingSphereCenterZ;
    public double BoundingSphereRadius;

    // The horizon occlusion point, expressed in the ellipsoid-scaled Earth-centered Fixed frame.
    // If this point is below the horizon, the entire tile is below the horizon.
    // See http://cesiumjs.org/2013/04/25/Horizon-culling/ for more information.
    public double HorizonOcclusionPointX;
    public double HorizonOcclusionPointY;
    public double HorizonOcclusionPointZ;

    public double West; // minX
    public double South;// minY
    public double East; // maxX
    public double North;// maxY

    public float LowestElevation;

    public bool HasData;


    public ElevationData(float lowestElevation, int gridSize)
    {
      ElevGrid = new float[gridSize * gridSize];
      EcefPoints = new Vector3[gridSize * gridSize];
      GridSize = gridSize;
      CenterX = 0;
      CenterY = 0;
      CenterZ = 0;
      BoundingSphereCenterX = 0;
      BoundingSphereCenterY = 0;
      BoundingSphereCenterZ = 0;
      BoundingSphereRadius = 0;
      HorizonOcclusionPointX = 0;
      HorizonOcclusionPointZ = 0;
      HorizonOcclusionPointY = 0;
      MaximumHeight = float.NegativeInfinity;
      MinimumHeight = float.PositiveInfinity;
      HasData = false;
      East = 0;
      West = 0;
      North = 0;
      South = 0;
      LowestElevation = lowestElevation;
    }


    public void Clear()
    {
      for (int i = 0; i < ElevGrid.Length; i++)
        ElevGrid[i] = LowestElevation;//QMConstants.SealLevelElev;
      for (int i = 0; i < EcefPoints.Length; i++)
        EcefPoints[i] = new Vector3();
    }

    public void MakeEmptyTile(LLBoundingBox boundary)
    {
      if (GridSize != QMConstants.FlatResolutionGridSize)
      {
        GridSize = QMConstants.FlatResolutionGridSize;
        Array.Resize(ref ElevGrid, GridSize * GridSize);
        Array.Resize(ref EcefPoints, GridSize * GridSize);
      }
      Clear();
      HasData = false;
      East = boundary.East;
      West = boundary.West;
      North = boundary.North;
      South = boundary.South;
      MaximumHeight = LowestElevation;//QMConstants.SealLevelElev;
      MinimumHeight = LowestElevation;//QMConstants.SealLevelElev;
      EcefPoints[0] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(East), Y = MapUtils.Deg2Rad(South), Z = LowestElevation });
      EcefPoints[1] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(West), Y = MapUtils.Deg2Rad(South), Z = LowestElevation });
      EcefPoints[2] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(East), Y = MapUtils.Deg2Rad(North), Z = LowestElevation });
      EcefPoints[3] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(West), Y = MapUtils.Deg2Rad(North), Z = LowestElevation });

    }

    public void MakeDemoTile(LLBoundingBox boundary)
    {
      MaximumHeight = float.NegativeInfinity;
      MinimumHeight = float.PositiveInfinity;
      if (GridSize != QMConstants.DemoResolutionGridSize)
      {
        GridSize = QMConstants.DemoResolutionGridSize;
        Array.Resize(ref ElevGrid, GridSize * GridSize);
        Array.Resize(ref EcefPoints, GridSize * GridSize);
      }


      var step = 20.0F / (GridSize - 1);

      var XStep = (boundary.East - boundary.West) / (GridSize - 1);
      var YStep = (boundary.North - boundary.South) / (GridSize - 1);
      float hgt;
      int pos = 0;
      double XPos;
      double YPos = boundary.South;

      for (int y = 0; y < GridSize; y++)
      {
        hgt = QMConstants.DemoBaseHgt;
        XPos = boundary.West;
        for (int x = 0; x < GridSize; x++)
        {
          // height 
          ElevGrid[pos] = hgt;

          if (hgt < MinimumHeight)
            MinimumHeight = hgt;
          if (hgt > MaximumHeight)
            MaximumHeight = hgt;

          hgt = hgt + step;

          // ECEF
          EcefPoints[pos] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(XPos), Y = MapUtils.Deg2Rad(YPos), Z = hgt});
          XPos = XPos + XStep;
          pos++;
        }

        YPos = YPos + YStep; // increase y
      }


      HasData = true;
      East = boundary.East;
      West = boundary.West;
      North = boundary.North;
      South = boundary.South;
    }

  }
}

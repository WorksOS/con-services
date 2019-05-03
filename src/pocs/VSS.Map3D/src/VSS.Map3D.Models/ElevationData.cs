using System;
using VSS.Map3D.Models.QMTile;

namespace VSS.Map3D.Models
{
  /// <summary>
  /// Elevation data supplied by a DEM source in single precision
  /// </summary>
  public struct ElevationData
  {
    public float[] Elev;
    /// <summary>
    /// Set for a square tile size
    /// </summary>
    public int GridSize;
    /// <summary>
    /// Grid width.Some grids may not be square
    /// </summary>
    //public int GridWidth;
    /// <summary>
    /// Grid height. some grids may not be square
    /// </summary>
    //public int GridHeight;
 //   public float MinElevation;
 //   public float MaxElevation;

    // The center of the tile in Earth-centered Fixed coordinates.
    public double CenterX;
    public double CenterY;
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

  //  public Boolean HasTerrain; // Lets caller know if data is available

    
    public ElevationData(int gridSize)
    {
      Elev = new float[gridSize * gridSize];
      GridSize = gridSize;
     // GridWidth = width;
     // GridHeight = height;
    //  MinElevation = min;
    //  MaxElevation = max;
      // todo To compute
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

    }
  }
}

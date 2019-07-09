using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Common.RequestStatistics;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.QuantizedMesh.Abstractions;

/// <summary>
/// Todo Ignore for checkin as will change in part two
/// </summary>
namespace VSS.TRex.QuantizedMesh.Executors
{
  /// <summary>
  /// Renders a quantized mesh tile for a location in the project
  /// </summary>
  public class RenderQMTile
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<RenderQMTile>();

    /// <summary>
    /// Details the error status of the tile result returned by the renderer
    /// </summary>
    public RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; set; }

    private Guid DataModelID;

    private bool CoordsAreGrid;

    private XYZ BLPoint; // : TWGS84Point;
    private XYZ TRPoint; // : TWGS84Point;

    private XYZ[] NEECoords;
    private XYZ[] LLHCoords;

    private double WorldTileWidth, WorldTileHeight;

    private IFilterSet Filters;

    private double TileRotation;
    private BoundingWorldExtent3D RotatedTileBoundingExtents = BoundingWorldExtent3D.Inverted();

    /// <summary>
    /// Constructor for the renderer
    /// </summary>
    /// <param name="AMode"></param>
    /// <param name="ANPixelsX"></param>
    /// <param name="ANPixelsY"></param>
    /// <param name="ACutFillDesign"></param>
    /// <param name="ARepresentColor"></param>
    public RenderQMTile(Guid ADataModelID,
      XYZ ABLPoint, // : TWGS84Point;
      XYZ ATRPoint, // : TWGS84Point;
      bool ACoordsAreGrid,
      IFilterSet filters,
      string requestingTRexNodeId
    )
    {
      DataModelID = ADataModelID;
      BLPoint = ABLPoint;
      TRPoint = ATRPoint;
      CoordsAreGrid = ACoordsAreGrid;
      Filters = filters;
      RequestingTRexNodeID = requestingTRexNodeId;
    }


    /// <summary>
    /// Executor that implements requesting and rendering subgrid information to create the rendered tile
    /// </summary>
    public IQuantizedMeshTile Execute()
    {

      // todo . Most likely to change in part two to surface export pattern. Currently based on TileRender pattern

      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}");

      ApplicationServiceRequestStatistics.Instance.NumQMTileRequests.Increment();

      Guid RequestDescriptor = Guid.NewGuid();

      // Determine the grid (NEE) coordinates of the bottom/left, top/right WGS-84 positions
      // given the project's coordinate system. If there is no coordinate system then exit.

      ISiteModel SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);
      Log.LogInformation($"Got Site model {DataModelID}, extents are {SiteModel.SiteModelExtent}");

      // We now need to work in grid coordinates. QM tile requests will always be in LL coordinates. Keeping logic here though in case this changes one day
      LLHCoords = new[]
      {
        new XYZ(BLPoint.X, BLPoint.Y),
        new XYZ(TRPoint.X, TRPoint.Y),
        new XYZ(BLPoint.X, TRPoint.Y),
        new XYZ(TRPoint.X, BLPoint.Y)
      };
      Log.LogInformation($"LLHCoords for tile request {string.Concat(LLHCoords)}, CoordsAreGrid {CoordsAreGrid}");

      if (CoordsAreGrid)
      {
        NEECoords = LLHCoords;
      }
      else
      {
        var conversionResult = DIContext.Obtain<IConvertCoordinates>().LLHToNEE(SiteModel.CSIB(), LLHCoords);

        if (conversionResult.ErrorCode != RequestErrorStatus.OK)
        {
          Log.LogInformation("Quantized tile render failure, could not convert bounding area from WGS to grid coordinates");
          ResultStatus = RequestErrorStatus.FailedToConvertClientWGSCoords;
          return null;
        }

        NEECoords = conversionResult.NEECoordinates;
      }
      Log.LogInformation($"After conversion NEECoords are {string.Concat(NEECoords)}");

      WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
      WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

      double dx = NEECoords[2].X - NEECoords[0].X;
      double dy = NEECoords[2].Y - NEECoords[0].Y;
      TileRotation = (Math.PI / 2) - Math.Atan2(dy, dx);

      RotatedTileBoundingExtents.SetInverted();
      foreach (XYZ xyz in NEECoords)
        RotatedTileBoundingExtents.Include(xyz.X, xyz.Y);


      Log.LogInformation($"Tile render executing across tile: [Rotation:{TileRotation}] " +
        $" [BL:{NEECoords[0].X}, {NEECoords[0].Y}, TL:{NEECoords[2].X},{NEECoords[2].Y}, " +
        $"TR:{NEECoords[1].X}, {NEECoords[1].Y}, BR:{NEECoords[3].X}, {NEECoords[3].Y}] " +
        $"World Width, Height: {WorldTileWidth}, {WorldTileHeight}");

      // , ,
      // , ,
      // , ,
      // , ,
      // NEECoords[1].X - NEECoords[0].X, NEECoords[1].Y - NEECoords[0].Y,
      //);


      // Construct the renderer, configure it, and set it on its way
      //  WorkingColourPalette = Nil;

//      PlanViewTileRenderer Renderer = new PlanViewTileRenderer();
      try
      {
        if (SiteModel == null)
          return null;

        // Intersect the site model extents with the extents requested by the caller
        Log.LogInformation($"Calculating intersection of bbox and site model {DataModelID}:{SiteModel.SiteModelExtent}");
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred");
        ResultStatus = RequestErrorStatus.Exception;
      }

      return null;
    }




  }
}

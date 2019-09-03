﻿using System;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.QuantizedMesh.GridFabric;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.QuantizedMesh.MeshUtils;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.SubGridTrees;
using VSS.TRex.QuantizedMesh.GridFabric.Responses;
using VSS.TRex.QuantizedMesh.Executors.Tasks;
using System.Threading.Tasks;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.QuantizedMesh.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Common;
using System.Collections.Generic;
using System.Diagnostics;

namespace VSS.TRex.QuantizedMesh.Executors
{
  public class QMTileExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<QMTileExecutor>();
    private string RequestingTRexNodeID { get; set; }
    private int TileGridSize;
    private Guid DataModelUid;
    private IFilterSet Filters;
    private int TileX;
    private int TileY;
    private int TileZ;
    private XYZ[] NEECoords;
    private XYZ[] LLHCoords;
    private BoundingWorldExtent3D RotatedTileBoundingExtents = BoundingWorldExtent3D.Inverted();
    private double GridIntervalX;
    private double GridIntervalY;
    private ElevationData ElevData;
    private LLBoundingBox TileBoundaryLL;
    private bool GridReductionRequired;
    // This will eventually be removed
    private static string DIMENSIONS_2012_DC_CSIB = "QM0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS05KA6TXFY6ZE4H6NQX8J3VAX79TTF82VPSV1KVR8W9V7BM1N3MEY5QHACSFNCK7VWPNY52RXGC1G9BPBS1QWA7ZVM6T2E0WMDY7P6CXJ68RB4CHJCDSVR6000047S29YVT08000";
    private float LowestElevation = 0.0F;
    private IPipelineProcessor processor;
    private QuantizedMeshTask task;
    private int DisplayMode = 0;
    private ISiteModel SiteModel;
    // The rotation of tile in the grid coordinate space due to any defined rotation on the coordinate system.
    public double TileRotation { get; set; }
    public QuantizedMeshResponse QMTileResponse { get; } = new QuantizedMeshResponse();
    public ElevationGridResponse GriddedElevationsResponse { get; } = new ElevationGridResponse();
    public GriddedElevDataRow[,] GriddedElevDataArray;
    private GriddedElevDataRow[,] ReducedElevDataArray;
    public RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;
    public GridReportOption GridReportOption { get; set; }
    public double StartNorthing { get; set; }
    public double StartEasting { get; set; }
    public double EndNorthing { get; set; }
    public double EndEasting { get; set; }
    public double Azimuth { get; set; }
    public int OverrideGridSize = 0;
    private bool Rotating;
    private double CosOfRotation = 1.0;
    private double SinOfRotation;
    private double CenterX = Consts.NullDouble;
    private double CenterY = Consts.NullDouble;
    public ILiftParameters LiftParams { get; set; } = new LiftParameters(); // Stage three todo

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public QMTileExecutor(Guid dataModelUid,
      IFilterSet filters,
      int x,
      int y,
      int z,
      int displayMode,
      string requestingTRexNodeId
    )
    {
      DataModelUid = dataModelUid;
      Filters = filters;
      TileX = x;
      TileY = y;
      TileZ = z;
      DisplayMode = displayMode;
      RequestingTRexNodeID = requestingTRexNodeId;
    }

    /// <summary>
    /// Convert 2 dimensional result array to a one dimensional DEM array for tilebuilder
    /// </summary>
    /// <param name="minElev"></param>
    /// <param name="maxElev"></param>
    /// <returns></returns>
    private bool ConvertGridToDEM(float minElev, float maxElev)
    {
      try
      {
        Log.LogDebug($"Tile.({TileX},{TileY}) ConvertGridToDEM, MinElev:{minElev}, MaxElev:{maxElev}, GridSize:{TileGridSize}, FirstPos:{GriddedElevDataArray[0, 0].Easting},{GriddedElevDataArray[0, 0].Northing},{GriddedElevDataArray[0, 0].Elevation}");
        ElevData.MaximumHeight = maxElev;
        ElevData.MinimumHeight = minElev;
        var defaultElev = LowestElevation;
        var yRange = TileBoundaryLL.North - TileBoundaryLL.South;
        var xRange = TileBoundaryLL.East - TileBoundaryLL.West;
        var xStep = xRange / (TileGridSize - 1);
        var yStep = yRange / (TileGridSize - 1);
        var k = 0;
        for (int y = 0; y < TileGridSize; y++)
          for (int x = 0; x < TileGridSize; x++)
          {
            // calculate LL position
            var lat = TileBoundaryLL.South + (y * yStep);
            var lon = TileBoundaryLL.West + (x * xStep);
            var elev = GriddedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight ? defaultElev : GriddedElevDataArray[x, y].Elevation;
            if (elev < ElevData.MinimumHeight)
              ElevData.MinimumHeight = elev; // reset to base
            ElevData.EcefPoints[k] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(lon), Y = MapUtils.Deg2Rad(lat), Z = elev });
            if (ElevData.ElevGrid[k] != elev)
              ElevData.ElevGrid[k] = elev; // missing data set to lowest
            k++;
          }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, $"Tile.({TileX},{TileY}). Exception in ConvertGridToDEM");
        return false;
      }
      return true;
    }

    /// <summary>
    /// No data so return an empty tile
    /// </summary>
    /// <returns></returns>
    private bool BuildEmptyTile()
    {
      Log.LogDebug($"#Tile#.({TileX},{TileY}) Execute End. Returning empty tile. Zoom:{TileZ}, GridSize{QMConstants.FlatResolutionGridSize}");
      // Even empty tiles must have header info correctly calculated 
      if (ElevData.GridSize == QMConstants.NoGridSize)
        ElevData = new ElevationData(LowestElevation, QMConstants.FlatResolutionGridSize); // elevation grid

      ElevData.MakeEmptyTile(TileBoundaryLL);
      QMTileBuilder tileBuilder = new QMTileBuilder()
      {
        TileData = ElevData,
        GridSize = ElevData.GridSize
      };

      if (!tileBuilder.BuildQuantizedMeshTile())
      {
        Log.LogError($"Tile.({TileX},{TileY}) failed to build empty tile. Error code: {tileBuilder.BuildTileFaultCode}");
        return false;
      }

      QMTileResponse.ResultStatus = RequestErrorStatus.OK;
      QMTileResponse.data = tileBuilder.QuantizedMeshTile; // return QM tile in response
      ResultStatus = RequestErrorStatus.OK;
      return true;
    }

    /// <summary>
    /// These root tiles are static
    /// </summary>
    /// <returns></returns>
    private void MakeRootTile()
    {
      Log.LogDebug($"#Tile.({TileX},{TileY}) Returning root tile");
      QMTileResponse.ResultStatus = RequestErrorStatus.OK;
      if (TileY == 0)
        QMTileResponse.data = QMConstants.Terrain0;
      else
        QMTileResponse.data = QMConstants.Terrain1;
      ResultStatus = RequestErrorStatus.OK;
    }

    /// <summary>
    /// Creates a demo tile. Useful for development
    /// </summary>
    /// <returns></returns>
    private bool BuildDemoTile()
    {
      Log.LogDebug($"#Tile.({TileX},{TileY}) Returning demo tile. (X:{TileX}, Y:{TileX},{TileY}, Z:{TileZ}), GridSize{QMConstants.DemoResolutionGridSize}");
      // Even empty tiles must have header info correctly calculated 
      if (ElevData.GridSize == QMConstants.NoGridSize)
        ElevData = new ElevationData(LowestElevation, QMConstants.DemoResolutionGridSize); // elevation grid

      ElevData.MakeDemoTile(TileBoundaryLL);

      QMTileBuilder tileBuilder = new QMTileBuilder()
      {
        TileData = ElevData,
        GridSize = ElevData.GridSize
      };

      if (!tileBuilder.BuildQuantizedMeshTile())
      {
        Log.LogError($"Tile.({TileX},{TileY}) failed to build demo tile. Error code: {tileBuilder.BuildTileFaultCode}");
        return false;
      }

      QMTileResponse.ResultStatus = RequestErrorStatus.OK;
      QMTileResponse.data = tileBuilder.QuantizedMeshTile; // return QM tile in response
      ResultStatus = RequestErrorStatus.OK;
      return true;
    }

    /// <summary>
    /// Setup values for rotation
    /// </summary>
    /// <param name="rotation"></param>
    private void SetRotation(double rotation)
    {
      SinOfRotation = Math.Sin(rotation);
      CosOfRotation = Math.Cos(rotation);
      Rotating = rotation != 0.0;
    }

    /// <summary>
    /// Rotate point around projects grid rotation
    /// </summary>
    /// <param name="fromX"></param>
    /// <param name="fromY"></param>
    /// <param name="toX"></param>
    /// <param name="toY"></param>
    private void Rotate_point(double fromX, double fromY, out double toX, out double toY)
    {
      toX = CenterX + (fromX - CenterX) * CosOfRotation - (fromY - CenterY) * SinOfRotation;
      toY = CenterY + (fromY - CenterY) * CosOfRotation + (fromX - CenterX) * SinOfRotation;
    }


    /// <summary>
    /// Setup GridSize for tile
    /// </summary>
    private void Initialise()
    {
      QMTileResponse.ResultStatus = RequestErrorStatus.FailedToBuildQuantizedMeshTile;
      ResultStatus = RequestErrorStatus.FailedToBuildQuantizedMeshTile;

      // Determine the QM tile resolution by the zoom level
      if (OverrideGridSize != QMConstants.NoGridSize)
        TileGridSize = OverrideGridSize;
      else
      {
        if (TileZ >= QMConstants.HighResolutionLevel)
          TileGridSize = QMConstants.HighResolutionGridSize;
        else if (TileZ >= QMConstants.MidResolutionLevel)
          TileGridSize = QMConstants.MidResolutionGridSize;
        else
          TileGridSize = QMConstants.FlatResolutionGridSize;
      }

      GridReductionRequired = TileGridSize == QMConstants.MidResolutionGridSize;

      // Setup for return. In most cases you want to at least return an empty tile
      ElevData = new ElevationData(LowestElevation, TileGridSize); // elevation grid
    }

    /// <summary>
    /// Setup pipeline for tile request
    /// </summary>
    /// <param name="siteModelExtent">Site Model Extent</param>
    /// <param name="cellSize">Cell Size</param>
    /// <returns></returns>
    private async Task<bool> SetupPipelineTask(BoundingWorldExtent3D siteModelExtent, double cellSize)
    {

      var requestDescriptor = Guid.NewGuid();
      if (DisplayMode == QMConstants.DisplayModeStandard)
      {
        // Note coords are always supplied lat long
        if (SiteModel.CSIB() == string.Empty)
        {
          ResultStatus = RequestErrorStatus.EmptyCoordinateSystem;
          Log.LogError($"Failed to obtain site model coordinate system CSIB file for Project:{DataModelUid}");
          return false;
        }
      }

      LLHCoords = new[] {new XYZ(MapUtils.Deg2Rad(TileBoundaryLL.West), MapUtils.Deg2Rad(TileBoundaryLL.South), 0),
          new XYZ(MapUtils.Deg2Rad(TileBoundaryLL.East), MapUtils.Deg2Rad(TileBoundaryLL.North), 0),
          new XYZ(MapUtils.Deg2Rad(TileBoundaryLL.West), MapUtils.Deg2Rad(TileBoundaryLL.North), 0),
          new XYZ(MapUtils.Deg2Rad(TileBoundaryLL.East), MapUtils.Deg2Rad(TileBoundaryLL.South), 0)};

      // This will change in Part3 once development is complete 
      var strCSIB = DisplayMode == QMConstants.DisplayModeStandard ? SiteModel.CSIB() : DIMENSIONS_2012_DC_CSIB;
      var conversionResult = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(strCSIB, LLHCoords);
      if (conversionResult.ErrorCode != RequestErrorStatus.OK)
      {
        Log.LogInformation($"Tile.({TileX},{TileY}) Tile render failure, could not convert bounding area from WGS to grid coordinates");
        ResultStatus = RequestErrorStatus.FailedToConvertClientWGSCoords;
        return false;
      }

      NEECoords = conversionResult.NEECoordinates;
      GridIntervalX = (NEECoords[1].X - NEECoords[0].X) / (TileGridSize - 1);
      GridIntervalY = (NEECoords[1].Y - NEECoords[0].Y) / (TileGridSize - 1);

      Log.LogDebug($"#Tile#.({TileX},{TileY}) TileInfo: Zoom:{TileZ}, TileSizeXY:{Math.Round(NEECoords[1].X - NEECoords[0].X, 3)}m x {Math.Round(NEECoords[2].Y - NEECoords[0].Y, 3)}m, GridInterval(m) X:{Math.Round(GridIntervalX, 3)}, Y:{Math.Round(GridIntervalY, 3)}, GridSize:{TileGridSize}, GridReduction:{GridReductionRequired}");

      var WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
      var WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

      double dx = NEECoords[2].X - NEECoords[0].X;
      CenterX = NEECoords[2].X + dx / 2;
      double dy = NEECoords[2].Y - NEECoords[0].Y;
      CenterY = NEECoords[0].Y + dy / 2;
      TileRotation = Math.PI / 2 - Math.Atan2(dy, dx);
      SetRotation(TileRotation);

      Log.LogDebug($"QMTile render executing across tile: [Rotation:{ MathUtilities.RadiansToDegrees(TileRotation)}] " +
          $" [BL:{NEECoords[0].X}, {NEECoords[0].Y}, TL:{NEECoords[2].X},{NEECoords[2].Y}, " +
          $"TR:{NEECoords[1].X}, {NEECoords[1].Y}, BR:{NEECoords[3].X}, {NEECoords[3].Y}] " +
          $"World Width, Height: {WorldTileWidth}, {WorldTileHeight}");

      RotatedTileBoundingExtents.SetInverted();
      foreach (var xyz in NEECoords)
        RotatedTileBoundingExtents.Include(xyz.X, xyz.Y);

      // Intersect the site model extents with the extents requested by the caller
      Log.LogDebug($"Tile.({TileX},{TileY}) Calculating intersection of bounding box and site model {DataModelUid}:{siteModelExtent}");
      RotatedTileBoundingExtents.Intersect(siteModelExtent);
      if (!RotatedTileBoundingExtents.IsValidPlanExtent)
      {
        ResultStatus = RequestErrorStatus.InvalidCoordinateRange;
        Log.LogInformation($"Tile.({TileX},{TileY}) Site model extents {siteModelExtent}, do not intersect RotatedTileBoundingExtents {RotatedTileBoundingExtents}");
        return false;
      }

      // Compute the override cell boundary to be used when processing cells in the sub grids
      // selected as a part of this pipeline
      // Increase cell boundary by one cell to allow for cells on the boundary that cross the boundary
      SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MinX,
        RotatedTileBoundingExtents.MinY, cellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
        out var CellExtents_MinX, out var CellExtents_MinY);
      SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MaxX,
        RotatedTileBoundingExtents.MaxY, cellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
        out var CellExtents_MaxX, out var CellExtents_MaxY);
      var CellExtents = new BoundingIntegerExtent2D(CellExtents_MinX, CellExtents_MinY, CellExtents_MaxX, CellExtents_MaxY);
      CellExtents.Expand(1);

      // Setup Task
      task = DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.QuantizedMesh) as QuantizedMeshTask;
      processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(requestDescriptor: requestDescriptor,
        dataModelID: DataModelUid,
        gridDataType: GridDataType.Height,
        response: GriddedElevationsResponse,
        filters: Filters,
        cutFillDesign: new DesignOffset(),
        task: task,
        pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
        requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
        requireSurveyedSurfaceInformation: false, //Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(DisplayMode.Height) && Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
        requestRequiresAccessToDesignFileExistenceMap: false,
        overrideSpatialCellRestriction: CellExtents,
        liftParams: LiftParams
      );

      // Set the grid TRexTask parameters for progressive processing
      processor.Task.RequestDescriptor = requestDescriptor;
      processor.Task.TRexNodeID = RequestingTRexNodeID;
      processor.Task.GridDataType = GridDataType.Height;
      // Setup new grid array for results 
      GriddedElevDataArray = new GriddedElevDataRow[TileGridSize, TileGridSize];
      double px1, py1, px2, py2;
      // build up a data sample grid from SW to NE
      for (int y = 0; y < TileGridSize; y++)
        for (int x = 0; x < TileGridSize; x++)
        {
          var x1 = NEECoords[0].X + (GridIntervalX * x);
          var y1 = NEECoords[0].Y + (GridIntervalY * y);
          if (Rotating)
            Rotate_point(x1, y1, out x1, out y1);
          GriddedElevDataArray[x, y].Easting = x1;
          GriddedElevDataArray[x, y].Northing = y1;
          GriddedElevDataArray[x, y].Elevation = CellPassConsts.NullHeight;
        }

      Log.LogDebug($"Tile.({TileX},{TileY}) Boundary grid coords:{string.Concat(NEECoords)}");
      Log.LogDebug($"Tile.({TileX},{TileY}) First Easting:{GriddedElevDataArray[0, 0].Easting} Northing:{GriddedElevDataArray[0, 0].Northing}");
      Log.LogDebug($"Tile.({TileX},{TileY}) Last Easting:{GriddedElevDataArray[TileGridSize - 1, TileGridSize - 1].Easting} Northing:{GriddedElevDataArray[TileGridSize - 1, TileGridSize - 1].Northing}");

      // point to container for results
      task.GriddedElevDataArray = GriddedElevDataArray;
      task.GridIntervalX = GridIntervalX;
      task.GridIntervalY = GridIntervalY;
      task.GridSize = TileGridSize;
      // Tile boundary
      task.TileMinX = NEECoords[0].X;
      task.TileMinY = NEECoords[0].Y;
      task.TileMaxX = NEECoords[1].X;
      task.TileMaxY = NEECoords[1].Y;
      task.LowestElevation = LowestElevation;

      Azimuth = 0;
      StartNorthing = 0;
      StartEasting = 0;

      processor.Pipeline.AreaControlSet =
        new AreaControlSet(false, GridIntervalX, GridIntervalY, StartEasting, StartNorthing, Azimuth);

      if (!await processor.BuildAsync())
      {
        Log.LogError($"Tile.({TileX},{TileY}) Failed to build pipeline processor for request to model {DataModelUid}");
        return false;
      }

      return true;
    }

    /// <summary>
    /// Reduces the size of the grid by half and smooths out the data
    /// </summary>
    private void ReduceAndSmoothGrid(ref float min, ref float max)
    {
      try
      {

        min = float.PositiveInfinity;
        max = float.NegativeInfinity;
        var newGridSize = (TileGridSize - 1) / 2 + 1; // halfing grid size
        ReducedElevDataArray = new GriddedElevDataRow[newGridSize, newGridSize];
        var rdGridIntervalX = GridIntervalX * 2;
        var rdGridIntervalY = GridIntervalY * 2;
        var midX = 0;
        var midY = 0;
        // build up grid coordinates and initialise height
        for (int y = 0; y < newGridSize; y++)
          for (int x = 0; x < newGridSize; x++)
          {
            midX = x * 2;
            midY = y * 2;
            var x1 = NEECoords[0].X + (rdGridIntervalX * x);
            var y1 = NEECoords[0].Y + (rdGridIntervalY * y);
            if (Rotating)
              Rotate_point(x1, y1, out x1, out y1);
            ReducedElevDataArray[x, y].Easting = x1;
            ReducedElevDataArray[x, y].Northing = y1;
            ReducedElevDataArray[x, y].Elevation = CellPassConsts.NullHeight; // default

            // Smooth data. There are 9 possible values surrounding a point except for edges edges
            // 4 points forward down
            if (y < newGridSize && x < newGridSize)
            {
              // Center point
              if (GriddedElevDataArray[midX, midY].Elevation != CellPassConsts.NullHeight)
              {
                if (ReducedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
                  ReducedElevDataArray[x, y].Elevation = GriddedElevDataArray[midX, midY].Elevation;
                else
                  ReducedElevDataArray[x, y].Elevation = (ReducedElevDataArray[x, y].Elevation + GriddedElevDataArray[midX, midY].Elevation) / 2;
              }

              if (y < newGridSize - 1) // if not last row y
              { // center down
                if (GriddedElevDataArray[midX, midY + 1].Elevation != CellPassConsts.NullHeight)
                {
                  if (ReducedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
                    ReducedElevDataArray[x, y].Elevation = GriddedElevDataArray[midX, midY + 1].Elevation;
                  else
                    ReducedElevDataArray[x, y].Elevation = (ReducedElevDataArray[x, y].Elevation + GriddedElevDataArray[midX, midY + 1].Elevation) / 2;
                }
              }

              if (x < newGridSize - 1) // if not end of row x
              { // center across
                if (GriddedElevDataArray[midX + 1, midY].Elevation != CellPassConsts.NullHeight)
                {
                  if (ReducedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
                    ReducedElevDataArray[x, y].Elevation = GriddedElevDataArray[midX + 1, midY].Elevation;
                  else
                    ReducedElevDataArray[x, y].Elevation = (ReducedElevDataArray[x, y].Elevation + GriddedElevDataArray[midX + 1, midY].Elevation) / 2;
                }
                // center down across
                if (y < newGridSize - 1) // if not last row y
                  if (GriddedElevDataArray[midX + 1, midY + 1].Elevation != CellPassConsts.NullHeight)
                  {
                    if (ReducedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
                      ReducedElevDataArray[x, y].Elevation = GriddedElevDataArray[midX + 1, midY + 1].Elevation;
                    else
                      ReducedElevDataArray[x, y].Elevation = (ReducedElevDataArray[x, y].Elevation + GriddedElevDataArray[midX + 1, midY + 1].Elevation) / 2;
                  }
              }
            }

            if (x > 0) // 2 places back on x
            { // center back
              if (GriddedElevDataArray[midX - 1, midY].Elevation != CellPassConsts.NullHeight)
              {
                if (ReducedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
                  ReducedElevDataArray[x, y].Elevation = GriddedElevDataArray[midX - 1, midY].Elevation;
                else
                  ReducedElevDataArray[x, y].Elevation = (ReducedElevDataArray[x, y].Elevation + GriddedElevDataArray[midX - 1, midY].Elevation) / 2;
              } // center back, down
              if (y < newGridSize - 1)
                if (GriddedElevDataArray[midX - 1, midY + 1].Elevation != CellPassConsts.NullHeight)
                {
                  if (ReducedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
                    ReducedElevDataArray[x, y].Elevation = GriddedElevDataArray[midX - 1, midY + 1].Elevation;
                  else
                    ReducedElevDataArray[x, y].Elevation = (ReducedElevDataArray[x, y].Elevation + GriddedElevDataArray[midX - 1, midY + 1].Elevation) / 2;
                }
            }

            if (y > 0)
            { // 3 points back on y
              if (x > 0)
              { // center back x, y
                if (GriddedElevDataArray[midX - 1, midY - 1].Elevation != CellPassConsts.NullHeight)
                {
                  if (ReducedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
                    ReducedElevDataArray[x, y].Elevation = GriddedElevDataArray[midX - 1, midY - 1].Elevation;
                  else
                    ReducedElevDataArray[x, y].Elevation = (ReducedElevDataArray[x, y].Elevation + GriddedElevDataArray[midX - 1, midY - 1].Elevation) / 2;
                }
              }
              // center back y
              if (GriddedElevDataArray[midX, midY - 1].Elevation != CellPassConsts.NullHeight)
              {
                if (ReducedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
                  ReducedElevDataArray[x, y].Elevation = GriddedElevDataArray[midX, midY - 1].Elevation;
                else
                  ReducedElevDataArray[x, y].Elevation = (ReducedElevDataArray[x, y].Elevation + GriddedElevDataArray[midX, midY - 1].Elevation) / 2;
              }
              // center forward x back y
              if (x < newGridSize - 1)
                if (GriddedElevDataArray[midX + 1, midY - 1].Elevation != CellPassConsts.NullHeight)
                {
                  if (ReducedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
                    ReducedElevDataArray[x, y].Elevation = GriddedElevDataArray[midX + 1, midY - 1].Elevation;
                  else
                    ReducedElevDataArray[x, y].Elevation = (ReducedElevDataArray[x, y].Elevation + GriddedElevDataArray[midX + 1, midY - 1].Elevation) / 2;
                }
            }
          }

        // recalculate min max now data has been smoothed 
        for (int y = 0; y < newGridSize; y++)
          for (int x = 0; x < newGridSize; x++)
          {
            if (ReducedElevDataArray[x, y].Elevation != CellPassConsts.NullHeight)
            {
              if (ReducedElevDataArray[x, y].Elevation < min)
                min = ReducedElevDataArray[x, y].Elevation;
              if (ReducedElevDataArray[x, y].Elevation > max)
                max = ReducedElevDataArray[x, y].Elevation;
            }
          }

        Log.LogDebug($"#Tile#.({TileX},{TileY}) Data successfully reduced. GridSize:{newGridSize}, Min:{min}, Max:{max}, FirstElev:{ReducedElevDataArray[0, 0].Elevation}, LastElev:{ReducedElevDataArray[newGridSize - 1, newGridSize - 1].Elevation}, GridInterval(m) X:{Math.Round(rdGridIntervalX, 3)}, Y:{Math.Round(rdGridIntervalY, 3)}");

      }
      catch (Exception ex)
      {
        Log.LogError(ex, $"Tile.({TileX},{TileY}). Unexpected exception in ReduceAndSmoothGrid");
      }

    }

    /// <summary>
    /// Debug function for viewing output
    /// </summary>
    private void OutputDebugTile(string preFix)
    {
      // can only run in ide check
      if (Debugger.IsAttached == false)
        return;

      var pathString = $"c:\\temp\\qmtiles";
      System.IO.Directory.CreateDirectory(pathString);
      var fileName = $"{preFix}Z{TileZ}X{TileX}Y{TileY}.txt";
      pathString = System.IO.Path.Combine(pathString, fileName);
      List<string> lines = new List<string>();
      for (var y = TileGridSize - 1; y >= 0; y--)
      {
        var str = "";
        for (var x = 0; x < TileGridSize; x++)
        {
          if (GriddedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight)
            str += "_";
          else
            str += "X";
        }
        lines.Add(str);
      }

      System.IO.File.WriteAllLines(pathString, lines.ToArray());

    }

    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the grid rows
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {

      // Get the lat lon boundary from xyz tile request
      TileBoundaryLL = MapGeo.TileXYZToRectLL(TileX, TileY, TileZ, out var yFlip);
      Log.LogInformation($"#Tile#.({TileX},{TileY}) Execute Start.  DMode:{DisplayMode}, Zoom:{TileZ} FlipY:{yFlip}. TileBoundary:{TileBoundaryLL.ToDisplay()}, DataModel:{DataModelUid}");

      if (TileZ == 0) // Send back default root tile
      {
        MakeRootTile();
        return true;
      }

      SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelUid);
      if (SiteModel == null)
      {
        ResultStatus = RequestErrorStatus.NoSuchDataModel;
        Log.LogError($"Tile.({TileX},{TileY}) Failed to obtain site model for {DataModelUid}");
        return false;
      }
      Log.LogDebug($"Tile.({TileX},{TileY}) Site model extents are {SiteModel.SiteModelExtent}. TileBoundary:{TileBoundaryLL.ToDisplay()}");
      if (!SiteModel.SiteModelExtent.IsValidPlanExtent) // No data return empty tile
        return BuildEmptyTile();

      // We will draw all missing data just below lowest elevation for site
      LowestElevation = (float)SiteModel.SiteModelExtent.MinZ - 1F;

      Initialise(); // setup tile requirements

      if (TileGridSize == QMConstants.FlatResolutionGridSize) // Too far out to see detail so return empty tile
        return BuildEmptyTile();

      if (DisplayMode == QMConstants.DisplayModeDemo) // development use only
        return BuildDemoTile();

      try
      {

        var setupResult = await SetupPipelineTask(SiteModel.SiteModelExtent, SiteModel.CellSize);
        if (!setupResult)
        {
          if (ResultStatus != RequestErrorStatus.InvalidCoordinateRange)
            Log.LogError($"Tile.({TileX},{TileY}) Unable to setup pipelinetask.");
          return BuildEmptyTile();
        }

        processor.Process();

        if (GriddedElevationsResponse.ResultStatus != RequestErrorStatus.OK)
        {
          Log.LogError($"Tile.({TileX},{TileY}) Unable to obtain data for gridded data. GriddedElevationRequestResponse: {GriddedElevationsResponse.ResultStatus.ToString()}");
          return BuildEmptyTile();
        }

        Log.LogInformation($"#Tile#.({TileX},{TileY}) Data successfully sampled. GridSize:{TileGridSize} Min:{task.MinElevation}, Max:{task.MaxElevation} FirstElev:{GriddedElevDataArray[0, 0].Elevation}, LastElev:{GriddedElevDataArray[TileGridSize - 1, TileGridSize - 1].Elevation}");

        ElevData.HasData = !float.IsPositiveInfinity(task.MinElevation); // check for data

        // Developer Debugging Only
        //if (ElevData.HasData)
        //  OutputDebugTile("Raw");

        if (!ElevData.HasData)
          return BuildEmptyTile();

        var minElev = task.MinElevation;
        var maxElev = task.MaxElevation;
        if (GridReductionRequired)
        {
          ReduceAndSmoothGrid(ref minElev, ref maxElev);
          if (minElev == float.PositiveInfinity || maxElev == float.NegativeInfinity)
          {
            Log.LogError($"Tile.({TileX},{TileY},{TileZ}) Min/Max not set. Unexpected result reducing grid. Empty tile returned");
            return BuildEmptyTile();
          }
          GriddedElevDataArray = ReducedElevDataArray; // point to new grid
          TileGridSize = (TileGridSize - 1) / 2 + 1;
          if (ElevData.GridSize != TileGridSize) // resize
            ElevData = new ElevationData(LowestElevation, TileGridSize);
          ElevData.HasData = !float.IsPositiveInfinity(minElev);

          // Developer Debugging only
          //if (ElevData.HasData)
          // OutputDebugTile("Reduced");

        }

        // Transform gridded data into a format the mesh builder can use
        if (!ConvertGridToDEM(minElev, maxElev))
          return BuildEmptyTile();

        // Build a quantized mesh from sampled elevations
        QMTileBuilder tileBuilder = new QMTileBuilder() { TileData = ElevData, GridSize = TileGridSize };
        Log.LogInformation($"Tile.({TileX},{TileY}) BuildQuantizedMeshTile. GridSize:{TileGridSize} Min:{ElevData.MinimumHeight}, Max:{ElevData.MaximumHeight}");
        if (!tileBuilder.BuildQuantizedMeshTile())
        {
          Log.LogError($"Tile.({TileX},{TileY}) BuildQuantizedMeshTile returned false with error code: {tileBuilder.BuildTileFaultCode}");
          return false;
        }

        QMTileResponse.data = tileBuilder.QuantizedMeshTile; // Make tile from mesh
        ResultStatus = RequestErrorStatus.OK;
        QMTileResponse.ResultStatus = ResultStatus;
        Log.LogDebug($"#Tile#.({TileX},{TileY}) Execute End. Returning production tile. CesiumY:{yFlip}, Zoom:{TileZ}, GridSize:{TileGridSize}");

        // Debveloper debugging only
        /*
         if (Debugger.IsAttached) 
           if (DisplayMode == QMConstants.DisplayModeDev)
             File.WriteAllBytes("c://temp//0.terrain", tileBuilder.QuantizedMeshTile);
        */

        return true;
      }
      catch (Exception ex)
      {
        Log.LogError(ex, $"#Tile#.({TileX},{TileY}). Exception building QuantizedMesh tile: ");
        return false;
      }
    }
  }
}

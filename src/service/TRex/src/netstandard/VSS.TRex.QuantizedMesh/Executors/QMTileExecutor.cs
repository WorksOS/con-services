using System;
using System.Reflection;
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
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.QuantizedMesh.MeshUtils;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.SubGridTrees;
using VSS.TRex.QuantizedMesh.GridFabric.Responses;
using VSS.TRex.Common.RequestStatistics;
using VSS.TRex.QuantizedMesh.Executors.Tasks;
using VSS.TRex.SubGridTrees.Client;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Utilities;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.QuantizedMesh.Models;
using System.IO;

namespace VSS.TRex.QuantizedMesh.Executors
{
  public class QMTileExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public QuantizedMeshResponse QMTileResponse { get; } = new QuantizedMeshResponse();
    public ElevationGridResponse GriddedElevationsResponse { get; } = new ElevationGridResponse();

    public GriddedElevDataRow[,] GriddedElevDataArray;

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; set; }
    public RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;
    private int TileGridSize;
    private Guid DataModelUid;
    private IFilterSet Filters;
    private double Tolerance;
    private int TileX;
    private int TileY;
    private int TileZ;
  //  private readonly XYZ BLPoint;
  //  private readonly XYZ TRPoint;
    private XYZ[] NEECoords;
   // private XYZ[] NEECoords2;
  //  private XYZ[] NEECoords3;

    private XYZ[] LLHCoords;
    private XYZ[] LLHCoords2;
    private BoundingWorldExtent3D RotatedTileBoundingExtents = BoundingWorldExtent3D.Inverted();
    private double GridIntervalX;
    private double GridIntervalY;
    private ElevationData ElevData;
    private LLBoundingBox TileBoundaryLL;

    /// <summary>
    /// Grid report option. Whether it is defined automatically or by user specified parameters.
    /// </summary>
    /// 
    public GridReportOption GridReportOption { get; set; }

    /// <summary>
    /// The Northing ordinate of the location to start gridding from
    /// </summary>
    public double StartNorthing { get; set; }

    /// <summary>
    /// The Easting ordinate of the location to start gridding from
    /// </summary>
    public double StartEasting { get; set; }

    /// <summary>
    /// The Northing ordinate of the location to end gridding at
    /// </summary>
    public double EndNorthing { get; set; }

    /// <summary>
    /// The Easting ordinate of the location to end gridding at
    /// </summary>
    public double EndEasting { get; set; }

    /// <summary>
    /// The orientation of the grid, expressed in radians
    /// </summary>
    public double Azimuth { get; set; }

    public int OverrideGridSize = 0;

    // Temp for debugging
    public static string DIMENSIONS_2012_DC_CSIB = "QM0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS05KA6TXFY6ZE4H6NQX8J3VAX79TTF82VPSV1KVR8W9V7BM1N3MEY5QHACSFNCK7VWPNY52RXGC1G9BPBS1QWA7ZVM6T2E0WMDY7P6CXJ68RB4CHJCDSVR6000047S29YVT08000";

    private float LowestElevation = 0.0F;
    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    private QuantizedMeshTask task;

    public bool DemoMode = false;
    public bool SaveOutputToDisk = false;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    /// <param name="dataModelUid"></param>
    /// <param name="filters"></param>
    /// <param name="requestingTRexNodeId"></param>
    public QMTileExecutor(Guid dataModelUid,
      IFilterSet filters,
      int X,
      int Y,
      int Z,
      string requestingTRexNodeId
    )
    {
      DataModelUid = dataModelUid;
      Filters = filters;
      TileX = X;
      TileY = Y;
      TileZ = Z;
      RequestingTRexNodeID = requestingTRexNodeId;
    }

    /// <summary>
    /// Convert 2 dimensional result array to a one dimensional DEM array for tilebuilder
    /// </summary>
    /// <param name="minElev"></param>
    /// <param name="maxElev"></param>
    /// <returns></returns>
    private async Task<bool> ConvertGridToDEM(float minElev, float maxElev)
    {
      Log.LogDebug($"Tile.({TileY}) ConvertGridToDEM, MinElev:{minElev}, MaxElev:{maxElev}, FirstPos:{GriddedElevDataArray[0,0].Easting},{GriddedElevDataArray[0, 0].Northing},{GriddedElevDataArray[0, 0].Elevation}");
      ElevData.MaximumHeight = maxElev;
      ElevData.MinimumHeight = minElev;


      // Run Test
      /*
      var NEECoordsX = new XYZ[2];
      NEECoordsX[0] = new XYZ(2882.177879, 1148.867816, 0);
      NEECoordsX[1] = new XYZ(1186.969147, 1186.969147, 0);
      var conversionResultX = await DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIMENSIONS_2012_DC_CSIB, NEECoordsX);
      if (conversionResultX.ErrorCode == RequestErrorStatus.OK)
      {
        var LLHCoordsX = new XYZ[conversionResultX.LLHCoordinates.Length];
        for (var i = 0; i < conversionResultX.LLHCoordinates.Length; i++)
        {
          LLHCoordsX[i].X = conversionResultX.LLHCoordinates[i].X;
          LLHCoordsX[i].Y = conversionResultX.LLHCoordinates[i].Y;
          LLHCoordsX[i].Z = conversionResultX.LLHCoordinates[i].Z;
        }
        Log.LogDebug($"Tile.({TileY}) After conversion LLCoordsX are {string.Concat(LLHCoordsX)}");
        var LLHCoordsX2 = conversionResultX.LLHCoordinates;
        Log.LogDebug($"Tile.({TileY}) After conversion LLCoordsX2 are {string.Concat(LLHCoordsX2)}");
      }
      */

      var neeCoords = new XYZ[TileGridSize * TileGridSize];
      int d = 0;
      // build up a results grid from SW to NE
      for (int y = 0; y < TileGridSize; y++)
        for (int x = 0; x < TileGridSize; x++)
        {
          neeCoords[d] = new XYZ(GriddedElevDataArray[x, y].Easting, GriddedElevDataArray[x, y].Northing, 0);
          d++;
        }

      //Log.LogDebug($"Tile.({TileY}) After conversion NEECoords3 are {string.Concat(neeCoords)}");
      


      // todo      (var errorCode, XYZ[] LLHCoords2) = await DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelUid).CSIB(), NEECoords2);
      //    (var errorCode, XYZ[] LLHCoords2) = await DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIMENSIONS_2012_DC_CSIB, NEECoords2);

      // var conversionResult = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(DIMENSIONS_2012_DC_CSIB, LLHCoords);

      int k = 0;
      var conversionResult = await DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIMENSIONS_2012_DC_CSIB, neeCoords);
      if (conversionResult.ErrorCode == RequestErrorStatus.OK)
      {
        var llCoords = conversionResult.LLHCoordinates;
        Log.LogDebug($"Tile.({TileY}) ConvertGridToDEM, NEEToLL First LL position. {llCoords[0].X},{llCoords[0].Y}");
       // Log.LogDebug($"Tile.({TileY}) ConvertGridToDEM, NEEToLL All. {string.Concat(llCoords)}");

        // use min hgt for default if present
//        var defaultElev = float.IsPositiveInfinity(ElevData.MinimumHeight) ? QMConstants.SealLevelElev : ElevData.MinimumHeight;
        var defaultElev = LowestElevation;

        //  if (errorCode == RequestErrorStatus.OK)

        for (int y = 0; y < TileGridSize; y++)
          for (int x = 0; x < TileGridSize; x++)
          {
            // todo eventually replace when SS part three implemented. There must be a value for now
            var elev = GriddedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight ? defaultElev : GriddedElevDataArray[x, y].Elevation;
            if (elev < ElevData.MinimumHeight)
              ElevData.MinimumHeight = elev; // reset to base
            // list of ecef points used in header calculations
            ElevData.EcefPoints[k] = CoordinateUtils.geo_to_ecef(new Vector3() { X = MapUtils.Deg2Rad(llCoords[k].X), Y = MapUtils.Deg2Rad(llCoords[k].Y), Z = elev});
            // tile elevation data
            ElevData.ElevGrid[k] = elev;
            k++;
          }

      }
      else
      {
        Log.LogError($"Tile.({TileY}) failure, could not convert bounding area from grid to WGS coordinates");
        // todo response.ResponseCode = SubGridRequestsResponseResult.Failure;
        return false;
      }

      return true;
    }

    private bool BuildEmptyTile()
    {
      Log.LogDebug($"Tile.({TileY}) Returning empty tile. (X:{TileX}, Y:{TileY}, Z:{TileZ}), GridSize{QMConstants.FlatResolutionGridSize}");
      // Even empty tiles must have header info correctly calculated 
      if (ElevData.GridSize == QMConstants.NoGridSize)
        ElevData = new ElevationData(LowestElevation,QMConstants.FlatResolutionGridSize); // elevation grid

      ElevData.MakeEmptyTile(TileBoundaryLL);

      QMTileBuilder tileBuilder = new QMTileBuilder()
      {
        TileData = ElevData,
        GridSize = ElevData.GridSize
      };

      if (!tileBuilder.BuildQuantizedMeshTile())
      {
        Log.LogError($"Tile.({TileY}) failed to build empty tile. Error code: {tileBuilder.BuildTileFaultCode}");
        return false;
      }
      QMTileResponse.ResultStatus = RequestErrorStatus.OK;
      QMTileResponse.data = tileBuilder.QuantizedMeshTile; // return QM tile in response
      ResultStatus = RequestErrorStatus.OK;
      return true;
    }

    private bool BuildDemoTile()
    {
      Log.LogDebug($"#Tile.({TileY}) Returning demo tile. (X:{TileX}, Y:{TileY}, Z:{TileZ}), GridSize{QMConstants.DemoResolutionGridSize}");
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
        Log.LogError($"Tile.({TileY}) failed to build demo tile. Error code: {tileBuilder.BuildTileFaultCode}");
        return false;
      }

      QMTileResponse.ResultStatus = RequestErrorStatus.OK;
      QMTileResponse.data = tileBuilder.QuantizedMeshTile; // return QM tile in response
      ResultStatus = RequestErrorStatus.OK;
      return true;
    }

    private void Intialise()
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

      // Setup for return. In most cases you want to at least return an empty tile
      ElevData = new ElevationData(LowestElevation, TileGridSize); // elevation grid
 
    }

    private async Task<bool> SetupPipelineTask(BoundingWorldExtent3D siteModelExtent, double cellSize)
    {

      //      Log.LogDebug($"QMTileExecutor Details. GridSize:{TileGridSize}, Y:{TileY}, LLBoundary:{TileBoundaryLL.ToDisplay()}");
      var requestDescriptor = Guid.NewGuid();


      //    Log.LogDebug($"QMTileExecutor.({TileY}) LLHCoords for tile request {string.Concat(LLHCoords)}");


      // Note coords are always supplied lat long
      /*      if (SiteModel.CSIB() == string.Empty)
            {
              ResultStatus = RequestErrorStatus.EmptyCoordinateSystem;
              Log.LogError($"Failed to obtain site model coordinate system CSIB file for Project:{DataModelUid}");
              return false;
            }

             todo      var conversionResult = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(SiteModel.CSIB(), LLHCoords);
            */

      LLHCoords = new[] {new XYZ(MapUtils.Deg2Rad(TileBoundaryLL.West), MapUtils.Deg2Rad(TileBoundaryLL.South), 0),
          new XYZ(MapUtils.Deg2Rad(TileBoundaryLL.East), MapUtils.Deg2Rad(TileBoundaryLL.North), 0),
          new XYZ(MapUtils.Deg2Rad(TileBoundaryLL.West), MapUtils.Deg2Rad(TileBoundaryLL.North), 0),
          new XYZ(MapUtils.Deg2Rad(TileBoundaryLL.East), MapUtils.Deg2Rad(TileBoundaryLL.South), 0)};


      Log.LogDebug($"Tile.({TileY}) Alan before LLHToNEE conversion LLHCoords are {string.Concat(LLHCoords)}");

      //        var conversionResult = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(DIMENSIONS_2012_DC_CSIB, LLHCoords, false);

      var conversionResult = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(DIMENSIONS_2012_DC_CSIB, LLHCoords);
      if (conversionResult.ErrorCode != RequestErrorStatus.OK)
      {
        Log.LogInformation($"Tile.({TileY}) Tile render failure, could not convert bounding area from WGS to grid coordinates");
        ResultStatus = RequestErrorStatus.FailedToConvertClientWGSCoords;
        return false;
      }

      NEECoords = conversionResult.NEECoordinates;
      Log.LogDebug($"Tile.({TileY}) Alan After conversion NEECoords are {string.Concat(NEECoords)}");

//      NEECoords[0].Z = 0;
//      NEECoords[1].Z = 0;
//      NEECoords[2].Z = 0;
//      NEECoords[3].Z = 0;

//      var conversionResultB = DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIMENSIONS_2012_DC_CSIB, NEECoords);
//      if (conversionResultB.Result.ErrorCode == RequestErrorStatus.OK)
 //     {
  //      var llCoords = conversionResultB.Result.LLHCoordinates;
   //     Log.LogDebug($"Tile.({TileY}) Alan After NEEToLLH conversion llCoords are {string.Concat(llCoords)}");
    //  }


      GridIntervalX = (NEECoords[1].X - NEECoords[0].X) / (TileGridSize - 1);
      GridIntervalY = (NEECoords[2].Y - NEECoords[0].Y) / (TileGridSize - 1);

      Log.LogDebug($"Tile.({TileY}) GridInterval(m) X:{GridIntervalX}, Y:{GridIntervalY}");

        //  WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
      //   WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

      //  double dx = NEECoords[2].X - NEECoords[0].X;
      //  double dy = NEECoords[2].Y - NEECoords[0].Y;
      //   TileRotation = Math.PI / 2 - Math.Atan2(dy, dx);

      RotatedTileBoundingExtents.SetInverted();
      foreach (var xyz in NEECoords)
        RotatedTileBoundingExtents.Include(xyz.X, xyz.Y);
      // Todo check with Raymond 

      // Intersect the site model extents with the extents requested by the caller
      Log.LogInformation($"Tile.({TileY}) Calculating intersection of bounding box and site model {DataModelUid}:{siteModelExtent}");
      RotatedTileBoundingExtents.Intersect(siteModelExtent);
      if (!RotatedTileBoundingExtents.IsValidPlanExtent)
      {
        ResultStatus = RequestErrorStatus.InvalidCoordinateRange;
        Log.LogInformation($"Tile.({TileY}) Site model extents {siteModelExtent}, do not intersect RotatedTileBoundingExtents {RotatedTileBoundingExtents}");
        return false; //  BuildEmptyTile(); // No data to display
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
        overrideSpatialCellRestriction: CellExtents
      );

      // Set the grid TRexTask parameters for progressive processing
      processor.Task.RequestDescriptor = requestDescriptor;
      processor.Task.TRexNodeID = RequestingTRexNodeID;
      processor.Task.GridDataType = GridDataType.Height;

      // Setup new data containers
      GriddedElevDataArray = new GriddedElevDataRow[TileGridSize, TileGridSize];
      int k = 0;

      // build up a data sample grid from SW to NE
      // this could be done without corex
      for (int y = 0; y < TileGridSize; y++)
        for (int x = 0; x < TileGridSize; x++)
        {
          GriddedElevDataArray[x, y].Easting = NEECoords[0].X + (GridIntervalX * x);
          GriddedElevDataArray[x, y].Northing = NEECoords[0].Y + (GridIntervalY * y);
          GriddedElevDataArray[x, y].Elevation = CellPassConsts.NullHeight;
          k++;
        }


      Log.LogDebug($"Tile.({TileY}) Boundary grid coords:{string.Concat(NEECoords)}");
      Log.LogDebug($"Tile.({TileY}) First Easting:{GriddedElevDataArray[0, 0].Easting} Northing:{GriddedElevDataArray[0, 0].Northing}");
      Log.LogDebug($"Tile.({TileY}) Last Easting:{GriddedElevDataArray[TileGridSize - 1, TileGridSize - 1].Easting} Northing:{GriddedElevDataArray[TileGridSize - 1, TileGridSize - 1].Northing}");

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
        Log.LogError($"Tile.({TileY}) Failed to build pipeline processor for request to model {DataModelUid}");
        return false;
      }

      return true;
    }


    private double CalculateGridStats()
    {
      var cnt = 0.0;
      for (int y = 0; y < TileGridSize; y++)
        for (int x = 0; x < TileGridSize; x++)
        {
          if (GriddedElevDataArray[x, y].Elevation != CellPassConsts.NullHeight)
            cnt++;
        }
      return (cnt /  GriddedElevDataArray.Length) * 100;
    }

    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the grid rows
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {

      // Get the lat lon boundary from xyz tile
      TileBoundaryLL = MapGeo.TileXYZToRectLL(TileX, TileY, TileZ);
      Log.LogInformation($"#Tile.({TileY}) Execute. (X:{TileX}, Y:{TileY}, Z:{TileZ}). TileBoundary:{TileBoundaryLL.ToDisplay()}. DataModel:{DataModelUid}");

      // get sitemodel
      var SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelUid);
      if (SiteModel == null)
      {
        ResultStatus = RequestErrorStatus.NoSuchDataModel;
        Log.LogError($"Tile.({TileY}) Failed to obtain site model for {DataModelUid}");
        return false;
      }

      Log.LogDebug($"Tile.({TileY}) Site model extents are {SiteModel.SiteModelExtent}. TileBoundary:{TileBoundaryLL.ToDisplay()}");

      // We will draw all missing data just below lowest elevation for site
      LowestElevation = (float)SiteModel.SiteModelExtent.MinZ - 1F;

      Intialise(); // setup tile requirements

      if (DemoMode)
        return BuildDemoTile();

      try
      {


        if (TileGridSize == QMConstants.FlatResolutionGridSize) // Not worth the effort. Too far out to see detail
          return BuildEmptyTile();

        var setupResult = await SetupPipelineTask(SiteModel.SiteModelExtent, SiteModel.CellSize);
        if (!setupResult)
          return BuildEmptyTile();

        processor.Process();
        if (GriddedElevationsResponse.ResultStatus != RequestErrorStatus.OK)
        {
          // throw new ArgumentException($"#Tile.({TileY}) Unable to obtain data for gridded data. GriddedElevationRequestResponse: {GriddedElevationsResponse.ResultStatus.ToString()}.");
          Log.LogError($"Tile.({TileY}) Unable to obtain data for gridded data. GriddedElevationRequestResponse: {GriddedElevationsResponse.ResultStatus.ToString()}");
          return BuildEmptyTile(); 
        }

        var percFull = CalculateGridStats();

        Log.LogError($"Tile.({TileY}) TotSampled{task.TotalSampled}, Used:{task.TotalUsed}, GridResults: PercentFull:{percFull}, MinElev:{task.MinElevation}, MaxElev:{task.MaxElevation}, FirstPos:{GriddedElevDataArray[0, 0].Easting},{GriddedElevDataArray[0, 0].Northing},{GriddedElevDataArray[0, 0].Elevation}");

        ElevData.HasData = !float.IsPositiveInfinity(task.MinElevation); // check for data
        if (!ElevData.HasData)
          return BuildEmptyTile();

        Log.LogDebug($"Tile.({TileY}) Converting sample grid to QM DEM");
        if (!await ConvertGridToDEM(task.MinElevation, task.MaxElevation))
        {
          return BuildEmptyTile(); 
        }


        // Build a quantized mesh tile from fetched elevations
        QMTileBuilder tileBuilder = new QMTileBuilder() {TileData = ElevData, GridSize = TileGridSize};

        if (!tileBuilder.BuildQuantizedMeshTile())
        {
          Log.LogError($"Tile.({TileY}) returned false with error code: {tileBuilder.BuildTileFaultCode}");
          return false;
        }

        QMTileResponse.data = tileBuilder.QuantizedMeshTile; // return QM tile in response
        ResultStatus = RequestErrorStatus.OK;
        QMTileResponse.ResultStatus = ResultStatus;
        Log.LogDebug($"Tile.({TileY}) Returning production tile. (X:{TileX}, Y:{TileY}, Z:{TileZ}), GridSize{TileGridSize}");
        if (SaveOutputToDisk)
          File.WriteAllBytes("c://temp//0.terrain", tileBuilder.QuantizedMeshTile);
        return true;
      }
      catch (Exception ex)
      {
        Log.LogError(ex, $"Tile.({TileY}). Exception building Quantized Mesh Tile: ");
        return false;
      }
    }

  }
}

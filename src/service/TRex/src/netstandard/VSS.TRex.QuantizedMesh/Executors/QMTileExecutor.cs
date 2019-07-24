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
    private const int NoGridSize = 0;
    private const int EmptyTileSize = 2;
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
    private XYZ[] NEECoords2;
    private XYZ[] NEECoords3;

    private XYZ[] LLHCoords;
    private XYZ[] LLHCoords2;
    private BoundingWorldExtent3D RotatedTileBoundingExtents = BoundingWorldExtent3D.Inverted();
    private double GridIntervalX;
    private double GridIntervalY;
    private ElevationData ElevData;
    private Vector3[] EcefPoints;
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


    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

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


    private async Task<bool> ConvertGridToDEM(float minElev, float maxElev)
    {
      ElevData = new ElevationData(TileGridSize); // elevation grid
      ElevData.MaximumHeight = maxElev;
      ElevData.MinimumHeight = minElev;
      EcefPoints = new Vector3[TileGridSize * TileGridSize]; // ecef grid
      int k = 0;

      // todo      (var errorCode, XYZ[] LLHCoords2) = await DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelUid).CSIB(), NEECoords2);
      //    (var errorCode, XYZ[] LLHCoords2) = await DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIMENSIONS_2012_DC_CSIB, NEECoords2);

      // var conversionResult = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(DIMENSIONS_2012_DC_CSIB, LLHCoords);

      NEECoords3 = new XYZ[TileGridSize * TileGridSize];
      int d = 0;
      // build up a results grid from SW to NE
      for (int y = 0; y < TileGridSize; y++)
      for (int x = 0; x < TileGridSize; x++)
      {
        NEECoords3[k] = new XYZ(d+1, d+1, 0);
        d++;
      }


      var conversionResult = await DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIMENSIONS_2012_DC_CSIB, NEECoords3);
      if (conversionResult.ErrorCode == RequestErrorStatus.OK)
      {
        LLHCoords2 = conversionResult.LLHCoordinates;
      

      //  if (errorCode == RequestErrorStatus.OK)
      
        for (int y = 0; y < TileGridSize; y++)
          for (int x = 0; x < TileGridSize; x++)
          {
            // todo eventually replace when SS part three implemented. There must be a value for now
            var elev = GriddedElevDataArray[x, y].Elevation == CellPassConsts.NullHeight ? ElevData.MinimumHeight : GriddedElevDataArray[x, y].Elevation;
            // list of ecef points used in header calculations
            EcefPoints[k] = CoordinateUtils.geo_to_ecef(new Vector3() { X = LLHCoords2[k].X, Y = LLHCoords2[k].Y, Z = elev});
            // tile elevation data
            ElevData.Elev[k] = elev;
            k++;
          }
      }
      else
      {
        Log.LogError("QMTileExecutor failure, could not convert bounding area from grid to WGS coordinates");
        // todo response.ResponseCode = SubGridRequestsResponseResult.Failure;
        return false;
      }

      return true;
    }

    private bool BuildEmptyTile()
    {
      ElevData = new ElevationData(EmptyTileSize); // elevation grid
      ElevData.MakeEmptyTile(TileBoundaryLL);
      EcefPoints = new Vector3[TileGridSize * TileGridSize];
      CoordinateUtils.geo_to_ecef(new Vector3() { X = ElevData.East, Y = ElevData.South, Z = 0 });
      CoordinateUtils.geo_to_ecef(new Vector3() { X = ElevData.West, Y = ElevData.South, Z = 0 });
      CoordinateUtils.geo_to_ecef(new Vector3() { X = ElevData.East, Y = ElevData.North, Z = 0 });
      CoordinateUtils.geo_to_ecef(new Vector3() { X = ElevData.West, Y = ElevData.North, Z = 0 });
      QMTileBuilder tileBuilder = new QMTileBuilder()
      {
        TileData = ElevData,
        TileEcefPoints = EcefPoints,
        GridSize = EmptyTileSize
      };

      if (!tileBuilder.BuildQuantizedMeshTile())
      {
        Log.LogError($"QMTileBuilder failed to build empty tile. Error code: {tileBuilder.BuildTileFaultCode}");
        return false;
      }
      return true;
    }


    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the grid rows
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      Log.LogInformation($"QMTileExecutor performing Execute for DataModel:{DataModelUid} X:{TileX}, Y:{TileX}, Z:{TileX}");

      // Get the lat lon boundary from xyz tile
      TileBoundaryLL = MapGeo.TileXYZToRectLL(TileX, TileY, TileZ);

      if (TileZ < 10) // Not worth the effort. Too far out
        return BuildEmptyTile();

      Guid requestDescriptor = Guid.NewGuid();

      var SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelUid);
      if (SiteModel == null)
      {
        ResultStatus = RequestErrorStatus.NoSuchDataModel;
        Log.LogError($"Failed to obtain site model for {DataModelUid}");
        return false;
      }


      Log.LogDebug($"Got Site model {DataModelUid}, extents are {SiteModel.SiteModelExtent}. TileBoundary:{TileBoundaryLL.ToDisplay()}");
      LLHCoords = new[]
      {
          new XYZ(TileBoundaryLL.West,TileBoundaryLL.South,0),
          new XYZ(TileBoundaryLL.East,TileBoundaryLL.North,0),
          new XYZ(TileBoundaryLL.West,TileBoundaryLL.North,0),
          new XYZ(TileBoundaryLL.East,TileBoundaryLL.South,0)
        };

      Log.LogDebug($"LLHCoords for tile request {string.Concat(LLHCoords)}");


      // Note coords are always supplied lat long
/*      if (SiteModel.CSIB() == string.Empty)
      {
        ResultStatus = RequestErrorStatus.EmptyCoordinateSystem;
        Log.LogError($"Failed to obtain site model coordinate system CSIB file for Project:{DataModelUid}");
        return false;
      }

       todo      var conversionResult = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(SiteModel.CSIB(), LLHCoords);
      */

      var conversionResult = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(DIMENSIONS_2012_DC_CSIB, LLHCoords);

      if (conversionResult.ErrorCode != RequestErrorStatus.OK)
      {
        Log.LogInformation("Tile render failure, could not convert bounding area from WGS to grid coordinates");
        ResultStatus = RequestErrorStatus.FailedToConvertClientWGSCoords;
        return false;
      }

      NEECoords = conversionResult.NEECoordinates;
      Log.LogDebug($"After conversion NEECoords are {string.Concat(NEECoords)}");

      // Determine the QM tile resolution by the zoom level
      if (OverrideGridSize != NoGridSize)
        TileGridSize = OverrideGridSize;
      else
      {
        if (TileZ >= QMConstants.HighResolutionLevel)
          TileGridSize = QMConstants.HighResolutionTile;
        else if (TileZ >= QMConstants.MidResolutionLevel)
          TileGridSize = QMConstants.MidResolutionTile;
        else
          TileGridSize = QMConstants.LowResolutionTile;
      }

      GridIntervalX = (NEECoords[1].X - NEECoords[0].X) / (TileGridSize - 1);
      GridIntervalY = (NEECoords[2].Y - NEECoords[0].Y) / (TileGridSize - 1);

      //    WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
      //   WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

    //  double dx = NEECoords[2].X - NEECoords[0].X;
    //  double dy = NEECoords[2].Y - NEECoords[0].Y;
    //   TileRotation = Math.PI / 2 - Math.Atan2(dy, dx);

      RotatedTileBoundingExtents.SetInverted();
      foreach (var xyz in NEECoords)
        RotatedTileBoundingExtents.Include(xyz.X, xyz.Y);
      // Todo check with Raymond 

      // Intersect the site model extents with the extents requested by the caller
      Log.LogInformation($"Calculating intersection of bounding box and site model {DataModelUid}:{SiteModel.SiteModelExtent}");
      RotatedTileBoundingExtents.Intersect(SiteModel.SiteModelExtent);
      if (!RotatedTileBoundingExtents.IsValidPlanExtent)
      {
        ResultStatus = RequestErrorStatus.InvalidCoordinateRange;
        Log.LogInformation($"Site model extents {SiteModel.SiteModelExtent}, do not intersect RotatedTileBoundingExtents {RotatedTileBoundingExtents}");
        return false;
      }

      // Compute the override cell boundary to be used when processing cells in the sub grids
      // selected as a part of this pipeline
      // Increase cell boundary by one cell to allow for cells on the boundary that cross the boundary

      SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MinX,
        RotatedTileBoundingExtents.MinY, SiteModel.CellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
        out var CellExtents_MinX, out var CellExtents_MinY);
      SubGridTree.CalculateIndexOfCellContainingPosition(RotatedTileBoundingExtents.MaxX,
        RotatedTileBoundingExtents.MaxY, SiteModel.CellSize, SubGridTreeConsts.DefaultIndexOriginOffset,
        out var CellExtents_MaxX, out var CellExtents_MaxY);

      var CellExtents = new BoundingIntegerExtent2D(CellExtents_MinX, CellExtents_MinY, CellExtents_MaxX, CellExtents_MaxY);
      CellExtents.Expand(1);

      // Setup Task
      var task = DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.QuantizedMesh) as QuantizedMeshTask;
      processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(requestDescriptor: requestDescriptor,
        dataModelID: DataModelUid,
        gridDataType: GridDataType.Height,
        response: GriddedElevationsResponse, 
        filters: Filters,
        cutFillDesign: new DesignOffset(),
        task: task,
        pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
        requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
        requireSurveyedSurfaceInformation: false,//Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(DisplayMode.Height) && Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
        requestRequiresAccessToDesignFileExistenceMap: false,
        overrideSpatialCellRestriction: CellExtents
      );

      // Set the grid TRexTask parameters for progressive processing
      processor.Task.RequestDescriptor = requestDescriptor;
      processor.Task.TRexNodeID = RequestingTRexNodeID; 
      processor.Task.GridDataType = GridDataType.Height;

      // Setup new data containers
      GriddedElevDataArray = new GriddedElevDataRow[TileGridSize, TileGridSize];
      NEECoords2 = new XYZ[TileGridSize * TileGridSize];
      int k = 0;
      // build up a results grid from SW to NE
      for (int y = 0; y < TileGridSize; y++)
      for (int x = 0; x < TileGridSize; x++)
      {
        GriddedElevDataArray[x, y].Easting   = NEECoords[0].X + (GridIntervalX * x);
        GriddedElevDataArray[x, y].Northing  = NEECoords[0].Y + (GridIntervalY * y);
        GriddedElevDataArray[x, y].Elevation = CellPassConsts.NullHeight;
        NEECoords2[k] = new XYZ(GriddedElevDataArray[x, y].Easting, GriddedElevDataArray[x, y].Northing,0);
        k++;
      }

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

      Azimuth = 0;
      StartNorthing = 0;
      StartEasting = 0;

      processor.Pipeline.AreaControlSet =
        new AreaControlSet(false, GridIntervalX, GridIntervalY,StartEasting, StartNorthing,Azimuth);

      if (!await processor.BuildAsync())
      {
        Log.LogError($"Failed to build pipeline processor for request to model {DataModelUid}");
        return false;
      }


      processor.Process();
      if (GriddedElevationsResponse.ResultStatus != RequestErrorStatus.OK)
      {
        throw new ArgumentException($"Unable to obtain data for gridded data. GriddedElevationRequestResponse: {GriddedElevationsResponse.ResultStatus.ToString()}.");
      }

      // todo make ecef array and pass to builder

      // todo use new GriddedElevDataArray in tilebuilder
      if (! await ConvertGridToDEM(task.MinElevation,task.MaxElevation))
      {
        // todo return code etc
        return false;
      };

      // Build a quantized mesh tile from fetched elevations
      QMTileBuilder tileBuilder = new QMTileBuilder()
      {
        TileData  = ElevData,
        TileEcefPoints = EcefPoints,
        GridSize = TileGridSize 
      };

      if (!tileBuilder.BuildQuantizedMeshTile())
      {
        Log.LogError($"QMTileBuilder returned false with error code: {tileBuilder.BuildTileFaultCode}");
        return false;
      }

 
      QMTileResponse.ResultStatus = RequestErrorStatus.OK;
      QMTileResponse.data = tileBuilder.QuantizedMeshTile; // return QM tile in response
      ResultStatus = RequestErrorStatus.OK;

      return true;
    }



  }
}

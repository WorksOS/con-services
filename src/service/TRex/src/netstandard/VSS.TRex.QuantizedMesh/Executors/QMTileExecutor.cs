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
    private XYZ[] LLHCoords;
    private BoundingWorldExtent3D RotatedTileBoundingExtents = BoundingWorldExtent3D.Inverted();
    private double GridIntervalX;
    private double GridIntervalY;
    private ElevationData ElevData;
    private Vector3[] EcefPoints;

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

    /*
    /// <summary>
    /// Computes a tight bounding extent around the elevation values stored in the sub grid tree
    /// </summary>
    /// <param name="dataStore"></param>
    /// <returns></returns>
    private BoundingWorldExtent3D DataStoreExtents(GenericSubGridTree_Float dataStore)
    {
      BoundingWorldExtent3D ComputedGridExtent = BoundingWorldExtent3D.Inverted();

      dataStore.ScanAllSubGrids(subGrid =>
      {
        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          var elev = ((GenericLeafSubGrid_Float)subGrid).Items[x, y];
          if (elev != Common.Consts.NullHeight)
            ComputedGridExtent.Include((int)(subGrid.OriginX + x), (int)(subGrid.OriginY + y), elev);
        });

        return true;
      });

      if (ComputedGridExtent.IsValidPlanExtent)
        ComputedGridExtent.Offset(-(int)SubGridTreeConsts.DefaultIndexOriginOffset, -(int)SubGridTreeConsts.DefaultIndexOriginOffset);

      // Convert the grid rectangle to a world rectangle, padding out the 3D bound by a small margin to avoid edge effects in calculations
      BoundingWorldExtent3D ComputedWorldExtent = new BoundingWorldExtent3D
       ((ComputedGridExtent.MinX - 1.01) * dataStore.CellSize,
        (ComputedGridExtent.MinY - 1.01) * dataStore.CellSize,
        (ComputedGridExtent.MaxX + 1.01) * dataStore.CellSize,
        (ComputedGridExtent.MaxY + 1.01) * dataStore.CellSize,
        ComputedGridExtent.MinZ - 0.01, ComputedGridExtent.MaxZ + 0.01);

      return ComputedWorldExtent;
    }

*/


    private bool ConvertGridToDEM(float minElev, float maxElev)
    {
      ElevData = new ElevationData(TileGridSize); // elevation grid
      ElevData.MaximumHeight = maxElev;
      ElevData.MinimumHeight = minElev;
      EcefPoints = new Vector3[TileGridSize * TileGridSize]; // ecef grid
      int k = 0;

      (var errorCode, XYZ[] LLHCoords2) = DIContext.Obtain<IConvertCoordinates>().NEEToLLH(DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelUid).CSIB(), NEECoords2);
      if (errorCode == RequestErrorStatus.OK)
      {
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


    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the grid rows
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Log.LogInformation($"QMTileExecutor performing Execute for DataModel:{DataModelUid} X:{TileX}, Y:{TileX}, Z:{TileX}");

      ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();
      Guid requestDescriptor = Guid.NewGuid();

      var SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelUid);
      if (SiteModel == null)
      {
        ResultStatus = RequestErrorStatus.NoSuchDataModel;
        Log.LogError($"Failed to obtain site model for {DataModelUid}");
        return false;
      }

      // Get the lat lon boundary from xyz tile
      var rect = MapGeo.TileXYZToRectLL(TileX, TileY, TileZ);
      Log.LogDebug($"Got Site model {DataModelUid}, extents are {SiteModel.SiteModelExtent}. TileBoundary:{rect.ToDisplay()}");
      LLHCoords = new[]
      {
          new XYZ(rect.West,rect.South),
          new XYZ(rect.East,rect.North),
          new XYZ(rect.West,rect.North),
          new XYZ(rect.East,rect.South)
        };

      Log.LogDebug($"LLHCoords for tile request {string.Concat(LLHCoords)}");
      // Note coords are always supplied lat long
      var conversionResult = DIContext.Obtain<IConvertCoordinates>().LLHToNEE(SiteModel.CSIB(), LLHCoords);
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
        NEECoords2[k] = new XYZ(GriddedElevDataArray[x, y].Easting, GriddedElevDataArray[x, y].Northing);
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

      if (!processor.Build())
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
      if (!ConvertGridToDEM(task.MinElevation,task.MaxElevation))
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

    /*
    /// <summary>
    /// Executor that implements creation of the QM Tile
    /// </summary>
    /// <returns></returns>
    public bool ExecuteOld()
    {
      Log.LogInformation($"QMTileExecutor performing Execute for DataModel:{DataModelUid} X:{X}, Y:{Y}, Z:{Z}");

      try
      {
        Guid RequestDescriptor = Guid.NewGuid();

        var SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelUid);
        if (SiteModel == null)
        {
          ResultStatus = RequestErrorStatus.NoSuchDataModel;
          Log.LogError($"Failed to obtain site model for {DataModelUid}");
          return false;
        }

        Log.LogDebug($"Got Site model {DataModelUid}, extents are {SiteModel.SiteModelExtent}");

        // Get the lat lon boundary from xyz tile
        var rect = MapGeo.TileXYZToRectLL(X, Y, Z);
        LLHCoords = new[]
        {
          new XYZ(rect.West,rect.South),
          new XYZ(rect.East,rect.North),
          new XYZ(rect.West,rect.North),
          new XYZ(rect.East,rect.South)

//          new XYZ(BLPoint.X, BLPoint.Y),
//          new XYZ(TRPoint.X, TRPoint.Y),
//          new XYZ(BLPoint.X, TRPoint.Y),
//          new XYZ(TRPoint.X, BLPoint.Y)
        };

        Log.LogDebug($"LLHCoords for tile request {string.Concat(LLHCoords)}");
        // Note coords are always lat long
        var conversionResult = DIContext.Obtain<IConvertCoordinates>().LLHToNEE(SiteModel.CSIB(), LLHCoords);

        if (conversionResult.ErrorCode != RequestErrorStatus.OK)
        {
          Log.LogInformation("Tile render failure, could not convert bounding area from WGS to grid coordinates");
          ResultStatus = RequestErrorStatus.FailedToConvertClientWGSCoords;

          return false;
        }

        NEECoords = conversionResult.NEECoordinates;
        Log.LogDebug($"After conversion NEECoords are {string.Concat(NEECoords)}");





    //    WorldTileHeight = MathUtilities.Hypot(NEECoords[0].X - NEECoords[2].X, NEECoords[0].Y - NEECoords[2].Y);
     //   WorldTileWidth = MathUtilities.Hypot(NEECoords[0].X - NEECoords[3].X, NEECoords[0].Y - NEECoords[3].Y);

        double dx = NEECoords[2].X - NEECoords[0].X;
        double dy = NEECoords[2].Y - NEECoords[0].Y;
       // TileRotation = Math.PI / 2 - Math.Atan2(dy, dx);

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



        // Provide the processor with a customised request analyser configured to return a set of sub grids. These sub grids
        // are the feed stock for the generated TIN surface
        processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(
          requestDescriptor: RequestDescriptor,
          dataModelID: DataModelUid,
          gridDataType: GridDataFromModeConverter.Convert(DisplayMode.Height),
          response: QMTileResponse, // todo
          filters: Filters,
          cutFillDesign: new DesignOffset(),
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.QuantizedMesh),
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requireSurveyedSurfaceInformation: Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(DisplayMode.Height) && Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
          requestRequiresAccessToDesignFileExistenceMap: false, 
          overrideSpatialCellRestriction: CellExtents);

        // Set the surface TRexTask parameters for progressive processing
        processor.Task.RequestDescriptor = RequestDescriptor;
        processor.Task.TRexNodeID = RequestingTRexNodeID;
        processor.Task.GridDataType = GridDataFromModeConverter.Convert(DisplayMode.Height);

        // Set the spatial extents of the tile boundary rotated into the north reference frame of the cell coordinate system to act as
        // a final restriction of the spatial extent used to govern data requests
        processor.OverrideSpatialExtents = RotatedTileBoundingExtents;

        if (!processor.Build())
        {
          ResultStatus = RequestErrorStatus.FailedToConfigureInternalPipeline;
          Log.LogError($"Failed to build pipeline processor for request to model {DataModelUid}");
          return false;
        }

        processor.Process();

        if (QMTileResponse.ResultStatus != RequestErrorStatus.OK)
        {
          Log.LogError($"Sub grids response status not OK: {QMTileResponse.ResultStatus}");
          return false;
        }

     //   ISiteModel SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);


        // Create the TIN decimator and populate it with the retrieve sub grids
        GenericSubGridTree_Float datastore = new GenericSubGridTree_Float(SiteModel.Grid.NumLevels, SiteModel.CellSize);
        foreach (var subGrid in ((SurfaceTask)processor.Task).SurfaceSubgrids)
        {
          INodeSubGrid newGridNode = datastore.ConstructPathToCell(subGrid.OriginX, subGrid.OriginY, SubGridPathConstructionType.CreatePathToLeaf) as INodeSubGrid;

          if (newGridNode == null)
          {
            Log.LogError($"Result from data store ConstructPathToCell({subGrid.OriginX}, {subGrid.OriginY}) was null. Aborting...");
            return false;
          }

          subGrid.Owner = datastore;
          newGridNode.GetSubGridCellIndex(subGrid.OriginX, subGrid.OriginY, out byte subGridIndexX, out byte subGridIndexY);
          newGridNode.SetSubGrid(subGridIndexX, subGridIndexY, subGrid);
        }

        // Decimate the elevations into a quantized tile
        QMTileBuilder tileGen = new QMTileBuilder(datastore)
        {
          Tolerance = Tolerance, // todo may not need this property
          GridSize = 10 // // todo
        };

        tileGen.SetDecimationExtents(DataStoreExtents(datastore));

        if (!tileGen.BuildQMTile(rect))
        {
          Log.LogError($"QMTileBuilder returned false with error code: {tileGen.BuildTileFaultCode}");
          return false;
        }

        QMTileResponse.ResultStatus = RequestErrorStatus.OK;
        QMTileResponse.data = tileGen.QMTile; // return QM tile in response

      }
      catch (Exception E)
      {
        QMTileResponse.ResultStatus = RequestErrorStatus.Unknown;
        Log.LogError(E, "QMTileExecutor raised Exception:");
        return false;
      }

      return true;
    }
    */


  }
}

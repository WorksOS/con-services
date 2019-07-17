using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.QuantizedMesh.GridFabric;
using VSS.TRex.Exports.Surfaces.Executors.Tasks;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Types;
using VSS.TRex.QuantizedMesh.MeshUtils;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.SubGridTrees;
using VSS.TRex.QuantizedMesh.GridFabric.Responses;

namespace VSS.TRex.QuantizedMesh.Executors
{
  public class QMTileExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public QuantizedMeshResponse QMTileResponse { get; } = new QuantizedMeshResponse();

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; set; }
    public RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

    private Guid DataModelID;
    private IFilterSet Filters;
    private double Tolerance;
    private int X;
    private int Y;
    private int Z;
  //  private readonly XYZ BLPoint;
  //  private readonly XYZ TRPoint;
    private XYZ[] NEECoords;
    private XYZ[] LLHCoords;
    private BoundingWorldExtent3D RotatedTileBoundingExtents = BoundingWorldExtent3D.Inverted();

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="filters"></param>
    /// <param name="requestingTRexNodeId"></param>
    public QMTileExecutor(Guid dataModelID,
      IFilterSet filters,
      int X,
      int Y,
      int Z,
      string requestingTRexNodeId
    )
    {
      DataModelID = dataModelID;
      Filters = filters;
      this.X = X;
      this.X = Y;
      this.X = Z;
      RequestingTRexNodeID = requestingTRexNodeId;
    }

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

    /// <summary>
    /// Executor that implements creation of the QM Tile
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Log.LogInformation($"QMTileExecutor performing Execute for DataModel:{DataModelID} X:{X}, Y:{Y}, Z:{Z}");

      try
      {
        Guid RequestDescriptor = Guid.NewGuid();

        var SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);
        if (SiteModel == null)
        {
          ResultStatus = RequestErrorStatus.NoSuchDataModel;
          Log.LogError($"Failed to obtain site model for {DataModelID}");
          return false;
        }

        Log.LogDebug($"Got Site model {DataModelID}, extents are {SiteModel.SiteModelExtent}");

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

        // Todo check with Raymond 

        // Intersect the site model extents with the extents requested by the caller
        Log.LogInformation($"Calculating intersection of bounding box and site model {DataModelID}:{SiteModel.SiteModelExtent}");
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
          dataModelID: DataModelID,
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
          Log.LogError($"Failed to build pipeline processor for request to model {DataModelID}");
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
        QMTileGenerator tileGen = new QMTileGenerator(datastore)
        {
          Tolerance = Tolerance, // todo may not need this property
          GridSize = 10 // // todo
        };

        tileGen.SetDecimationExtents(DataStoreExtents(datastore));

        if (!tileGen.BuildQMTile(rect))
        {
          Log.LogError($"QMTileGenerator returned false with error code: {tileGen.BuildTileFaultCode}");
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



  }
}

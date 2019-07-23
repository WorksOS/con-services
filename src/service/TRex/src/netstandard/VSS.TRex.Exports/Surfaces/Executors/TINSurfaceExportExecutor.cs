using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exports.Surfaces.Executors.Tasks;
using VSS.TRex.Exports.Surfaces.GridDecimator;
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

namespace VSS.TRex.Exports.Surfaces.Executors
{
  /// <summary>
  /// Generates a decimated TIN surface
  /// </summary>
  public class TINSurfaceExportExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public TINSurfaceRequestResponse SurfaceSubGridsResponse { get; } = new TINSurfaceRequestResponse();

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; set; }

    private Guid DataModelID;
    private IFilterSet Filters;
    private double Tolerance;

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="filters"></param>
    /// <param name="tolerance"></param>
    /// <param name="requestingTRexNodeId"></param>
    public TINSurfaceExportExecutor(Guid dataModelID,
      IFilterSet filters,
      double tolerance,
      string requestingTRexNodeId
    )
    {
      DataModelID = dataModelID;
      Filters = filters;
      Tolerance = tolerance;
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
    /// Executor that implements creation of the TIN surface
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}");

      try
      {
        Guid RequestDescriptor = Guid.NewGuid();

        // Provide the processor with a customised request analyser configured to return a set of sub grids. These sub grids
        // are the feed stock for the generated TIN surface
        processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(
          requestDescriptor: RequestDescriptor,
          dataModelID: DataModelID,
          gridDataType: GridDataFromModeConverter.Convert(DisplayMode.Height),
          response: SurfaceSubGridsResponse,
          filters: Filters,
          cutFillDesign: new DesignOffset(), 
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.SurfaceExport),
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requireSurveyedSurfaceInformation: Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(DisplayMode.Height) && Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
          requestRequiresAccessToDesignFileExistenceMap: false, //Rendering.Utilities.RequestRequiresAccessToDesignFileExistenceMap(DisplayMode.Height),
          overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted());

        // Set the surface TRexTask parameters for progressive processing
        processor.Task.RequestDescriptor = RequestDescriptor;
        processor.Task.TRexNodeID = RequestingTRexNodeID;
        processor.Task.GridDataType = GridDataFromModeConverter.Convert(DisplayMode.Height);

        if (!await processor.BuildAsync())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {DataModelID}");
          return false;
        }

        processor.Process();

        if (SurfaceSubGridsResponse.ResultStatus != RequestErrorStatus.OK)
        {
          Log.LogError($"Sub grids response status not OK: {SurfaceSubGridsResponse.ResultStatus}");
          return false;
        }

        ISiteModel SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DataModelID);

        if (SiteModel == null)
        {
          Log.LogError($"Failed to obtain site model for {DataModelID}");
          return false;
        }

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

        // Decimate the elevations into a grid
        GridToTINDecimator decimator = new GridToTINDecimator(datastore)
        {
          Tolerance = Tolerance
        };
        decimator.SetDecimationExtents(DataStoreExtents(datastore));

        if (!decimator.BuildMesh())
        {
          Log.LogError($"Decimator returned false with error code: {decimator.BuildMeshFaultCode}");
          return false;
        }

        // A decimated TIN has been successfully constructed...  Return it!
        SurfaceSubGridsResponse.TIN = decimator.GetTIN();
      }
      catch (Exception E)
      {
        Log.LogError(E, "ExecutePipeline raised Exception:");
        return false;
      }

      return true;
    }
  }
}

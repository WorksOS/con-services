using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Exports.Surfaces.Executors.Tasks;
using VSS.TRex.Exports.Surfaces.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.SubGridTrees.Utilities;
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
    public TINSurfaceRequestResponse SurfaceSubGridsResponse { get; set; } = new TINSurfaceRequestResponse();

    // FExternalDescriptor :TASNodeRequestDescriptor;

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; set; }

    private Guid DataModelID;
    private FilterSet Filters;
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
      //AExternalDescriptor :TASNodeRequestDescriptor;
      FilterSet filters,
      double tolerance,
      string requestingTRexNodeId
    )
    {
      DataModelID = dataModelID;
      // ExternalDescriptor = AExternalDescriptor
      Filters = filters;
      Tolerance = tolerance;
      RequestingTRexNodeID = requestingTRexNodeId;
    }

    /// <summary>
    /// Computes a tight bounding extent around the elevation values stored in the subgrid tree
    /// </summary>
    /// <param name="dataStore"></param>
    /// <returns></returns>
    private BoundingWorldExtent3D DataStoreExtents(GenericSubGridTree<float> dataStore)
    {
      BoundingWorldExtent3D ComputedGridExtent = BoundingWorldExtent3D.Inverted();

      dataStore.ScanAllSubGrids(subGrid =>
      {
        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          float elev = ((ClientHeightLeafSubGrid)subGrid).Cells[x, y];
          if (elev != Common.Consts.NullHeight)
            ComputedGridExtent.Include((int)(subGrid.OriginX + x), (int)(subGrid.OriginY + y), elev);
        });

        return true;
      });

      if (ComputedGridExtent.IsValidPlanExtent)
        ComputedGridExtent.Offset(-(int)SubGridTree.DefaultIndexOriginOffset, -(int)SubGridTree.DefaultIndexOriginOffset);

      // Convert the grid rectangle to a world rectangle, padding out the 3D bound by a small margin to avoid edge effects in calcualations
      BoundingWorldExtent3D ComputedWorldExtent = new BoundingWorldExtent3D
       (ComputedGridExtent.MinX - 0.01 * dataStore.CellSize,
        ComputedGridExtent.MinY - 0.01 * dataStore.CellSize,
        (ComputedGridExtent.MaxX + 1 + 0.01) * dataStore.CellSize,
        (ComputedGridExtent.MaxY + 1 + 0.01) * dataStore.CellSize,
        ComputedGridExtent.MinZ - 0.01, ComputedGridExtent.MaxZ + 0.01);

      return ComputedWorldExtent;
    }

    /// <summary>
    /// Executor that implements creation of the TIN surface
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}");

      try
      {
//        ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

        Guid RequestDescriptor = Guid.NewGuid();

        // Provide the processor with a customised request analyser configured to return a set of subgrids. These subgrids
        // are the feed stock for the generated TIN surface
        processor = new PipelineProcessor(requestDescriptor: RequestDescriptor,
          dataModelID: DataModelID,
          siteModel: null,
          gridDataType: GridDataFromModeConverter.Convert(DisplayMode.Height),
          response: SurfaceSubGridsResponse,
          filters: Filters,
          cutFillDesignID: Guid.Empty,
          task: new SurfaceTask(RequestDescriptor, RequestingTRexNodeID, GridDataFromModeConverter.Convert(DisplayMode.Height)),
          pipeline: new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(),
          requestAnalyser: new RequestAnalyser(),
          requireSurveyedSurfaceInformation: Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(DisplayMode.Height) && Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
          requestRequiresAccessToDesignFileExistanceMap: false, //Rendering.Utilities.RequestRequiresAccessToDesignFileExistanceMap(DisplayMode.Height),
          overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted());

        if (!processor.Build())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {DataModelID}");
          return false;
        }

        processor.Process();

        if (SurfaceSubGridsResponse.ResultStatus != RequestErrorStatus.OK)
        {
          Log.LogError($"Subgrids response status not OK: {SurfaceSubGridsResponse.ResultStatus}");
          return false;
        }

        ISiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(DataModelID);

        if (SiteModel == null)
        {
          Log.LogError($"Failed to obtain sitemodel for {DataModelID}");
          return false;
        }

        // Create the TIN decimator and populate it with the retrieve subgrids
        GenericSubGridTree<float> datastore = new GenericSubGridTree<float>(SiteModel.Grid.NumLevels, SiteModel.Grid.CellSize);
        foreach (var subgrid in ((SurfaceTask)processor.Task).SurfaceSubgrids)
        {
          INodeSubGrid newGridNode = datastore.ConstructPathToCell(subgrid.OriginX, subgrid.OriginY, SubGridPathConstructionType.CreatePathToLeaf) as INodeSubGrid;

          if (newGridNode == null)
          {
            Log.LogError($"Result from datastore.ConstructPathToCell({subgrid.OriginX}, {subgrid.OriginY}) was null. Aborting...");
            return false;
          }

          subgrid.Owner = datastore;
          newGridNode.GetSubGridCellIndex(subgrid.OriginX, subgrid.OriginY, out byte subGridIndexX, out byte subGridIndexY);
          newGridNode.SetSubGrid(subGridIndexX, subGridIndexY, subgrid);
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
        Log.LogError($"ExecutePipeline raised exception {E}");
        return false;
      }

      return true;
    }
  }

}

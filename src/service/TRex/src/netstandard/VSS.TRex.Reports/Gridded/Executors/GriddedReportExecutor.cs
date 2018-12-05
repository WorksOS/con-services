using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Reports.Gridded.Executors.Tasks;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.RequestStatistics;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.Gridded.Executors
{
  /// <summary>
  /// Generates a patch of subgrids from a wider query
  /// </summary>
  public class GriddedReportExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public GriddedReportRequestResponse GridSubGridsResponse { get; set; } = new GriddedReportRequestResponse();

    ///// <summary>
    ///// The TRex application service node performing the request
    ///// </summary>
    //private string RequestingTRexNodeID { get; set; }

    //private Guid DataModelID;
    //private IFilterSet Filters;

    ///// <summary>
    ///// The identifier for the design held in the designs list ofr the project to be used to calculate cut/fill values
    ///// </summary>
    //public Guid CutFillDesignID { get; set; }

    private GriddedReportRequestArgument griddedReportRequestArgument;

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public GriddedReportExecutor(GriddedReportRequestArgument arg) => griddedReportRequestArgument = arg;

    /// <summary>
    /// Executor that implements requesting and rendering subgrid information to create the rendered tile
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Log.LogInformation($"Performing Execute for DataModel:{griddedReportRequestArgument.ProjectID}");

      try
      {
        ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

        Guid RequestDescriptor = Guid.NewGuid();

        processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(requestDescriptor: RequestDescriptor,
          dataModelID: griddedReportRequestArgument.ProjectID,
          siteModel: null,
          gridDataType: GridDataType.CellProfile,
          response: GridSubGridsResponse,
          filters: griddedReportRequestArgument.Filters,
          cutFillDesignID: griddedReportRequestArgument.ReferenceDesignID,
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.GriddedReport),
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requireSurveyedSurfaceInformation: Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(griddedReportRequestArgument.Filters),
          requestRequiresAccessToDesignFileExistenceMap: griddedReportRequestArgument.ReferenceDesignID != Guid.Empty,
          overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
          );

        // todoJeannie Units?
        processor.RequestAnalyser.WorldExtents = new BoundingWorldExtent3D(griddedReportRequestArgument.StartEasting,
          griddedReportRequestArgument.StartNorthing,
          griddedReportRequestArgument.EndEasting,
          griddedReportRequestArgument.EndNorthing);

        //todoJeannie what to do with: GridReportOption; GridInterval; Azimuth


        if (!processor.Build())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {griddedReportRequestArgument.ProjectID}");
          return false;
        }
        
        processor.Process();

        if (GridSubGridsResponse.ResultStatus == RequestErrorStatus.OK)
          GridSubGridsResponse.SubGrids = ((GriddedReportTask)processor.Task).ResultantSubgrids;
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

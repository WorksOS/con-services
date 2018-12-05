using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
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

    // FExternalDescriptor :TASNodeRequestDescriptor;

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; set; }

    private Guid DataModelID;
    private IFilterSet Filters;

    /// <summary>
    /// The identifier for the design held in the designs list ofr the project to be used to calculate cut/fill values
    /// </summary>
    public Guid CutFillDesignID { get; set; }

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    /// <param name="dataModelID"></param>
    /// <param name="mode"></param>
    /// <param name="filters"></param>
    /// <param name="cutFillDesignID"></param>
    /// <param name="requestingTRexNodeId"></param>
    public GriddedReportExecutor(Guid dataModelID,
      IFilterSet filters,
      Guid cutFillDesignID, //DesignDescriptor ACutFillDesign,
      //AReferenceVolumeType : TComputeICVolumesType;
      string requestingTRexNodeId
    )
    {
      DataModelID = dataModelID;
      Filters = filters;
      CutFillDesignID = cutFillDesignID; // CutFillDesign = ACutFillDesign;
      //ReferenceVolumeType = AReferenceVolumeType;
      RequestingTRexNodeID = requestingTRexNodeId;
    }

    /// <summary>
    /// Executor that implements requesting and rendering subgrid information to create the rendered tile
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}");

      try
      {
        ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

        Guid RequestDescriptor = Guid.NewGuid();

        processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(requestDescriptor: RequestDescriptor,
          dataModelID: DataModelID,
          siteModel: null,
          gridDataType: GridDataType.CellProfile,
          response: GridSubGridsResponse,
          filters: Filters,
          cutFillDesignID: CutFillDesignID,
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.GriddedReport),
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requireSurveyedSurfaceInformation: Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
          requestRequiresAccessToDesignFileExistenceMap: CutFillDesignID != Guid.Empty,
          overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
          );

        // processor.Task.MyPackager = new todoJeannie Contstruc;

        if (!processor.Build())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {DataModelID}");
          return false;
        }

        //// If this is the first page requested then count the total number of patches required for all subgrids to be returned
        //if (DataPatchPageNumber == 0)
        //  PatchSubGridsResponse.TotalNumberOfPagesToCoverFilteredData =
        //    (int) Math.Truncate(Math.Ceiling(processor.RequestAnalyser.CountOfSubgridsThatWillBeSubmitted() / (double)DataPatchPageSize));

        //processor.Process();

        //if (PatchSubGridsResponse.ResultStatus == RequestErrorStatus.OK)
        //  PatchSubGridsResponse.SubGrids = ((GridTask) processor.Task).PatchSubgrids;
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

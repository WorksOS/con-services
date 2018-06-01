using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.RequestStatistics;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.Patches.Executors
{
  /// <summary>
  /// Generates a patch of subgrids from a wider query
  /// </summary>
  public class PatchExecutor
  {
    private static readonly ILogger
      Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object avbailable for inspection once the Executor has completed processing
    /// </summary>
    public PatchRequestResponse PatchSubGridsResponse { get; set; } = new PatchRequestResponse();

    // FExternalDescriptor :TASNodeRequestDescriptor;

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; set; }

    private Guid DataModelID;
    private DisplayMode Mode;
    private FilterSet Filters;

    private int DataPatchPageNumber;
    private int DataPatchPageSize;

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
    /// <param name="dataPatchPageNumber"></param>
    /// <param name="dataPatchPageSize"></param>
    public PatchExecutor(Guid dataModelID,
      //AExternalDescriptor :TASNodeRequestDescriptor;
      DisplayMode mode,
      FilterSet filters,
      Guid cutFillDesignID, //DesignDescriptor ACutFillDesign,
      //AReferenceVolumeType : TComputeICVolumesType;
      //AICOptions: TSVOICOptions;
      string requestingTRexNodeId,
      int dataPatchPageNumber,
      int dataPatchPageSize
    )
    {
      DataModelID = dataModelID;
      // ExternalDescriptor = AExternalDescriptor
      Mode = mode;
      Filters = filters;
      CutFillDesignID = cutFillDesignID; // CutFillDesign = ACutFillDesign;
      //ReferenceVolumeType = AReferenceVolumeType;
      //ICOptions = AICOptions;
      RequestingTRexNodeID = requestingTRexNodeId;
      DataPatchPageNumber = dataPatchPageNumber;
      DataPatchPageSize = dataPatchPageSize;
    }

    /// <summary>
    /// Executor that implements requesting and rendering subgrid information to create the rendered tile
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}, Mode={Mode}");

      try
      {
        ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

        Guid RequestDescriptor = Guid.NewGuid();

        // Provide the processor with a customised request analyser configured to return a specific page of subgrids
        processor = new PipelineProcessor(requestDescriptor: RequestDescriptor,
          dataModelID: DataModelID, 
          gridDataType: GridDataFromModeConverter.Convert(Mode), 
          response: PatchSubGridsResponse, 
          filters: Filters, 
          cutFillDesignID: CutFillDesignID,
          task: new PatchTask(RequestDescriptor, RequestingTRexNodeID, GridDataFromModeConverter.Convert(Mode)),
          pipeline: new SubGridPipelineProgressive<SubGridsRequestArgument, SubGridRequestsResponse>(),
          requestAnalyser: new RequestAnalyser
          {
            SinglePageRequestNumber = DataPatchPageNumber,
            SinglePageRequestSize = DataPatchPageSize,
            SubmitSinglePageOfRequests = true
          },
          requireSurveyedSurfaceInformation: Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode) && Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
          requestRequiresAccessToDesignFileExistanceMap: Utilities.RequestRequiresAccessToDesignFileExistanceMap(Mode /*ReferenceVolumeType*/));

        if (!processor.Build())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {DataModelID}");
          return false;
        }

        // If this is the first page requested then make a count the total number of patches required for all subgrids to be returned
        if (DataPatchPageNumber == 0)
          PatchSubGridsResponse.TotalNumberOfPagesToCoverFilteredData =
            (int) Math.Truncate(Math.Ceiling(processor.RequestAnalyser.CountOfSubgridsThatWillBeSubmitted() /
                                             (1.0 * DataPatchPageSize)));

        processor.Process();

        if (PatchSubGridsResponse.ResultStatus == RequestErrorStatus.OK)
          PatchSubGridsResponse.SubGrids = ((PatchTask) processor.Task).PatchSubgrids;
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

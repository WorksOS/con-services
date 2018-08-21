using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Pipelines;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Models.Arguments;
using VSS.TRex.GridFabric.Models.Responses;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.RequestStatistics;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.Patches.Executors
{
  /// <summary>
  /// Generates a patch of subgrids from a wider query
  /// </summary>
  public class PatchExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public PatchRequestResponse PatchSubGridsResponse { get; set; } = new PatchRequestResponse();

    // FExternalDescriptor :TASNodeRequestDescriptor;

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; set; }

    private Guid DataModelID;
    private DisplayMode Mode;
    private IFilterSet Filters;

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
      IFilterSet filters,
      Guid cutFillDesignID, //DesignDescriptor ACutFillDesign,
      //AReferenceVolumeType : TComputeICVolumesType;
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
          siteModel:null,
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
          requireSurveyedSurfaceInformation: VSS.TRex.Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode) 
                                             && VSS.TRex.Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
          requestRequiresAccessToDesignFileExistanceMap: VSS.TRex.Rendering.Utilities.RequestRequiresAccessToDesignFileExistanceMap(Mode /*ReferenceVolumeType*/),
          overrideSpatialCellRestriction:BoundingIntegerExtent2D.Inverted());

        if (!processor.Build())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {DataModelID}");
          return false;
        }

        // If this is the first page requested then count the total number of patches required for all subgrids to be returned
        if (DataPatchPageNumber == 0)
          PatchSubGridsResponse.TotalNumberOfPagesToCoverFilteredData =
            (int) Math.Truncate(Math.Ceiling(processor.RequestAnalyser.CountOfSubgridsThatWillBeSubmitted() / (double)DataPatchPageSize));

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

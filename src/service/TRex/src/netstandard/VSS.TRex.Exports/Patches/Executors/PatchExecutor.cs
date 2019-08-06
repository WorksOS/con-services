using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Models;
using VSS.TRex.DI;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Exports.Patches.Executors.Tasks;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.Common.RequestStatistics;
using VSS.TRex.Designs.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.Patches.Executors
{
  /// <summary>
  /// Generates a patch of sub grids from a wider query
  /// </summary>
  public class PatchExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public PatchRequestResponse PatchSubGridsResponse { get; } = new PatchRequestResponse();

    // FExternalDescriptor :TASNodeRequestDescriptor;

    /// <summary>
    /// The TRex application service node performing the request
    /// </summary>
    private string RequestingTRexNodeID { get; }

    private Guid DataModelID;
    private DisplayMode Mode;
    private IFilterSet Filters;
    private ILiftParameters LiftParams;

    private int DataPatchPageNumber;
    private int DataPatchPageSize;

    /// <summary>
    /// The identifier for the design held in the designs list of the project to be used to calculate cut/fill values
    /// together with the offset if it's a reference surface
    /// </summary>
    public DesignOffset CutFillDesign { get; set; }

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public PatchExecutor(
      Guid dataModelID,
      //AExternalDescriptor :TASNodeRequestDescriptor;
      DisplayMode mode,
      IFilterSet filters,
      DesignOffset cutFillDesign, 
      //AReferenceVolumeType : TComputeICVolumesType;
      string requestingTRexNodeId,
      int dataPatchPageNumber,
      int dataPatchPageSize,
      ILiftParameters liftParams
    )
    {
      DataModelID = dataModelID;
      // ExternalDescriptor = AExternalDescriptor
      Mode = mode;
      Filters = filters;
      CutFillDesign = cutFillDesign; 
      //ReferenceVolumeType = AReferenceVolumeType;
      RequestingTRexNodeID = requestingTRexNodeId;
      DataPatchPageNumber = dataPatchPageNumber;
      DataPatchPageSize = dataPatchPageSize;
      LiftParams = liftParams;
    }

    /// <summary>
    /// Executor that implements requesting and rendering sub grid information to create the rendered tile
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      Log.LogInformation($"Performing Execute for DataModel:{DataModelID}, Mode={Mode}, RequestingNodeID={RequestingTRexNodeID}");

      ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

      Guid RequestDescriptor = Guid.NewGuid();

      processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(
        RequestDescriptor,
        DataModelID,
        GridDataFromModeConverter.Convert(Mode),
        PatchSubGridsResponse,
        Filters,
        CutFillDesign,
        DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.PatchExport),
        DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
        DIContext.Obtain<IRequestAnalyser>(),
        Rendering.Utilities.DisplayModeRequireSurveyedSurfaceInformation(Mode)
                                 && Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(Filters),
        Rendering.Utilities.RequestRequiresAccessToDesignFileExistenceMap(Mode /*ReferenceVolumeType*/),
        BoundingIntegerExtent2D.Inverted(),
        LiftParams);

      // Configure the request analyser to return a single page of results.
      processor.RequestAnalyser.SinglePageRequestNumber = DataPatchPageNumber;
      processor.RequestAnalyser.SinglePageRequestSize = DataPatchPageSize;
      processor.RequestAnalyser.SubmitSinglePageOfRequests = true;

      if (!await processor.BuildAsync())
      {
        Log.LogError($"Failed to build pipeline processor for request to model {DataModelID}");
        return false;
      }

      // If this is the first page requested then count the total number of patches required for all sub grids to be returned
      if (DataPatchPageNumber == 0)
        PatchSubGridsResponse.TotalNumberOfPagesToCoverFilteredData =
          (int) Math.Truncate(Math.Ceiling(processor.RequestAnalyser.CountOfSubGridsThatWillBeSubmitted() / (double) DataPatchPageSize));

      processor.Process();

      if (PatchSubGridsResponse.ResultStatus == RequestErrorStatus.OK)
        PatchSubGridsResponse.SubGrids = ((PatchTask) processor.Task).PatchSubGrids;

      return true;
    }
  }
}

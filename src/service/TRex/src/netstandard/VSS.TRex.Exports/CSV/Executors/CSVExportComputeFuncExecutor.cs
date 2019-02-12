using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.DI;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.RequestStatistics;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.CSV.Executors
{
  /// <summary>
  /// Generates a patch of sub grids from a wider query
  /// </summary>
  public class CSVExportComputeFuncExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The response object available for inspection once the Executor has completed processing
    /// </summary>
    public CSVExportRequestResponse CSVExportRequestResponse { get; set; } = new CSVExportRequestResponse();

    private readonly CSVExportRequestArgument _CSVExportRequestArgument;

    /// <summary>
    /// The pipeline processor used to coordinate construction, coordinate and orchestration of the pipelined request
    /// </summary>
    private IPipelineProcessor processor;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public CSVExportComputeFuncExecutor(CSVExportRequestArgument arg) => _CSVExportRequestArgument = arg;

    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the grid rows
    /// </summary>
    /// <returns></returns>
    public bool Execute()
    {
      Log.LogInformation($"Performing Execute for DataModel:{_CSVExportRequestArgument.ProjectID}");

      try
      {
        ApplicationServiceRequestStatistics.Instance.NumSubgridPageRequests.Increment();

        Guid requestDescriptor = Guid.NewGuid();
        var gridDataType = _CSVExportRequestArgument.OutputType == OutputTypes.PassCountLastPass || _CSVExportRequestArgument.OutputType == OutputTypes.VedaFinalPass ? GridDataType.CellProfile : GridDataType.CellPasses;

        processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(requestDescriptor: requestDescriptor,
          dataModelID: _CSVExportRequestArgument.ProjectID,
          siteModel: null,
          gridDataType: gridDataType,
          response: CSVExportRequestResponse,
          filters: _CSVExportRequestArgument.Filters,
          cutFillDesignID: Guid.Empty, 
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.VetaExport), // todoJeanne combine veta and PassCount?
          pipeline: DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          requestAnalyser: DIContext.Obtain<IRequestAnalyser>(),
          requireSurveyedSurfaceInformation: Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(_CSVExportRequestArgument.Filters),
          requestRequiresAccessToDesignFileExistenceMap: false,
          overrideSpatialCellRestriction: BoundingIntegerExtent2D.Inverted()
          );

        // Set the grid TRexTask parameters for progressive processing
        processor.Task.RequestDescriptor = requestDescriptor;
        processor.Task.TRexNodeID = _CSVExportRequestArgument.TRexNodeID;
        processor.Task.GridDataType = gridDataType;

        ((CSVExportTask) processor.Task).requestArgument = _CSVExportRequestArgument;

        if (!processor.Build())
        {
          Log.LogError($"Failed to build pipeline processor for request to model {_CSVExportRequestArgument.ProjectID}");
          return false;
        }
        
        processor.Process();

        if (CSVExportRequestResponse.ResultStatus != RequestErrorStatus.OK)
        {
          throw new ArgumentException($"Unable to obtain data for CSV Export. CSVExportRequestResponse: {CSVExportRequestResponse.ResultStatus.ToString()}.");
        }

      //var response        ((CSVExportTask)processor.Task).requestArgument = _CSVExportRequestArgument;

      }
      catch (Exception e)
      {
        Log.LogError(e, "ExecutePipeline raised exception");
        return false;
      }

      return true;
    }

  }
}

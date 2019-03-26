using System;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.CSV.Executors
{
  /// <summary>
  /// Generates a patch of sub grids from a wider query
  /// </summary>
  public class CSVExportComputeFuncExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CSVExportComputeFuncExecutor>();

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
        Guid requestDescriptor = Guid.NewGuid();
        var gridDataType = _CSVExportRequestArgument.OutputType == OutputTypes.PassCountLastPass || _CSVExportRequestArgument.OutputType == OutputTypes.VedaFinalPass
          ? GridDataType.CellProfile
          : GridDataType.CellPasses;

        processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild(requestDescriptor: requestDescriptor,
          dataModelID: _CSVExportRequestArgument.ProjectID,
          gridDataType: gridDataType,
          response: new SubGridsPipelinedResponseBase(),
          filters: _CSVExportRequestArgument.Filters,
          cutFillDesignID: Guid.Empty,
          task: DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.CSVExport),
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

        ((CSVExportTask) processor.Task).SubGridExportProcessor = new CSVExportSubGridProcessor(_CSVExportRequestArgument);

        if (!processor.Build())
        {
          Log.LogError($"Failed to build CSV export pipeline processor for project: {_CSVExportRequestArgument.ProjectID} filename: {_CSVExportRequestArgument.FileName}");
          CSVExportRequestResponse.ResultStatus = processor.Response.ResultStatus;
          return false;
        }

        processor.Process();

        if (processor.Response.ResultStatus != RequestErrorStatus.OK)
        {
          Log.LogError($"Failed to obtain data for CSV Export, for project: {_CSVExportRequestArgument.ProjectID} filename: {_CSVExportRequestArgument.FileName}. response: {processor.Response.ResultStatus.ToString()}.");
          CSVExportRequestResponse.ResultStatus = processor.Response.ResultStatus;
          return false;
        }

        if (((CSVExportTask) processor.Task).DataRows.Count > 0)
        {
          var csvExportFileWriter = new CSVExportFileWriter(_CSVExportRequestArgument);
          var s3FullPath = csvExportFileWriter.PersistResult(((CSVExportTask) processor.Task).DataRows);

          if (string.IsNullOrEmpty(s3FullPath))
          {
            Log.LogError($"CSV export failed to write to S3. project: {_CSVExportRequestArgument.ProjectID} filename: {_CSVExportRequestArgument.FileName}.");
            return false;
          }

          if (((CSVExportTask) processor.Task).SubGridExportProcessor.RecordCountLimitReached())
            CSVExportRequestResponse.ResultStatus = RequestErrorStatus.ExportExceededRowLimit;
          else
            CSVExportRequestResponse.ResultStatus = RequestErrorStatus.OK;
          CSVExportRequestResponse.fileName = s3FullPath;
        }
        else
        {
          CSVExportRequestResponse.ResultStatus = RequestErrorStatus.ExportNoDataFound;
        }

      }
      catch (Exception e)
      {
        Log.LogError(e, "ExecutePipeline raised exception");
        CSVExportRequestResponse.ResultStatus = RequestErrorStatus.Exception;
        return false;
      }

      return true;
    }
  }
}

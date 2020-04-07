using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Geometry;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGrids.GridFabric.Arguments;
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
    public CSVExportRequestResponse CSVExportRequestResponse { get; } = new CSVExportRequestResponse();

    private readonly CSVExportRequestArgument _CSVExportRequestArgument;

    /// <summary>
    /// Constructor for the renderer accepting all parameters necessary for its operation
    /// </summary>
    public CSVExportComputeFuncExecutor(CSVExportRequestArgument arg) => _CSVExportRequestArgument = arg;

    /// <summary>
    /// Executor that implements requesting and rendering grid information to create the grid rows
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ExecuteAsync()
    {
      Log.LogInformation($"Performing Execute for DataModel:{_CSVExportRequestArgument.ProjectID}");

      try
      {
        var requestDescriptor = Guid.NewGuid();
        var gridDataType = _CSVExportRequestArgument.OutputType == OutputTypes.PassCountLastPass || _CSVExportRequestArgument.OutputType == OutputTypes.VedaFinalPass
          ? GridDataType.CellProfile
          : GridDataType.CellPasses;

        using (var _processor = DIContext.Obtain<IPipelineProcessorFactory>().NewInstanceNoBuild<SubGridsRequestArgument>(
          requestDescriptor,
          _CSVExportRequestArgument.ProjectID,
          gridDataType,
          new SubGridsPipelinedResponseBase(),
          _CSVExportRequestArgument.Filters,
          new DesignOffset(),
          DIContext.Obtain<Func<PipelineProcessorTaskStyle, ITRexTask>>()(PipelineProcessorTaskStyle.CSVExport),
          DIContext.Obtain<Func<PipelineProcessorPipelineStyle, ISubGridPipelineBase>>()(PipelineProcessorPipelineStyle.DefaultProgressive),
          DIContext.Obtain<IRequestAnalyser>(),
          Rendering.Utilities.FilterRequireSurveyedSurfaceInformation(_CSVExportRequestArgument.Filters),
          false,
          BoundingIntegerExtent2D.Inverted(),
          _CSVExportRequestArgument.LiftParams
        ))
        {
          // Set the grid TRexTask parameters for progressive processing
          _processor.Task.RequestDescriptor = requestDescriptor;
          _processor.Task.TRexNodeID = _CSVExportRequestArgument.TRexNodeID;
          _processor.Task.GridDataType = gridDataType;

          ((CSVExportTask) _processor.Task).SubGridExportProcessor = new CSVExportSubGridProcessor(_CSVExportRequestArgument);

          if (!await _processor.BuildAsync())
          {
            Log.LogError($"Failed to build CSV export pipeline processor for project: {_CSVExportRequestArgument.ProjectID} filename: {_CSVExportRequestArgument.FileName}");
            CSVExportRequestResponse.ResultStatus = _processor.Response.ResultStatus;
            return false;
          }

          _processor.Process();

          if (_processor.Response.ResultStatus != RequestErrorStatus.OK)
          {
            Log.LogError($"Failed to obtain data for CSV Export, for project: {_CSVExportRequestArgument.ProjectID} filename: {_CSVExportRequestArgument.FileName}. response: {_processor.Response.ResultStatus.ToString()}.");
            CSVExportRequestResponse.ResultStatus = _processor.Response.ResultStatus;
            return false;
          }

          if (((CSVExportTask) _processor.Task).DataRows.Count > 0)
          {
            var csvExportFileWriter = new CSVExportFileWriter(_CSVExportRequestArgument);
            var s3FullPath = csvExportFileWriter.PersistResult(((CSVExportTask) _processor.Task).DataRows);

            if (string.IsNullOrEmpty(s3FullPath))
            {
              Log.LogError($"CSV export failed to write to S3. project: {_CSVExportRequestArgument.ProjectID} filename: {_CSVExportRequestArgument.FileName}.");
              CSVExportRequestResponse.ResultStatus = RequestErrorStatus.ExportUnableToLoadFileToS3;
              return false;
            }

            if (((CSVExportTask) _processor.Task).SubGridExportProcessor.RecordCountLimitReached())
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

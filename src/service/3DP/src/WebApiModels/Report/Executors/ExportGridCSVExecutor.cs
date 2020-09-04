using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary pass counts request to Raptor
  /// </summary>
  public class ExportGridCSVExecutor : RequestExecutorContainer
  {
    private const string STR_NORTHING = "Northing";
    private const string STR_EASTING = "Easting";
    private const string STR_INSERT_LINE = "/n";
    private const string STR_ELEVATION = "Elevation";
    private const string STR_CUT_FILL = "Cut/Fill";
    private const string STR_CMV = "CMV";
    private const string STR_MDP = "MDP";
    private const string STR_PASS_COUNT = "PassCount";
    private const string STR_TEMPERATURE = "Temperature";

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ExportGridCSVExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the summary pass counts request by passing the request to TRex and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<ExportGridCSV>(item);

        if (request.reportType == GriddedCSVReportType.Alignment)
          throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));

        var overrides = AutoMapperUtility.Automapper.Map<OverridingTargets>(request.liftBuildSettings);
        var liftSettings = AutoMapperUtility.Automapper.Map<LiftSettings>(request.liftBuildSettings);

        var compactionReportGridRequest = new CompactionReportGridTRexRequest
          (
            request.ProjectUid.Value, 
            request.filter, 
            request.reportElevation, 
            request.reportCMV, 
            request.reportMDP, 
            request.reportPassCount,
            request.reportTemperature,
            request.reportCutFill,
            request.designFile?.FileUid,
            request.designFile?.Offset,
            request.interval,
            request.reportOption,
            request.startNorthing,
            request.startEasting,
            request.endNorthing,
            request.endEasting,
            request.direction,
            overrides,
            liftSettings
          );
  
        log.LogInformation($"Calling TRex SendCompactionReportGridRequest for projectUid: {request.ProjectUid}");

        var responseData = await trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(compactionReportGridRequest, "/report/grid", customHeaders);

        return responseData.Length > 0
          ? ConvertTRexGridResult(responseData)
          : CreateNullGridReturnedResult();
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private static ContractExecutionResult CreateNullGridReturnedResult()
    {
      return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Null grid stream returned");
    }

    private ExportResult ConvertTRexGridResult(Stream stream)
    {
      log.LogDebug($"{nameof(ConvertTRexGridResult)}: Retrieving response data from TRex");

      try
      {
        var griddedReportResult = new GriddedReportResult(ReportType.Gridded);
        griddedReportResult.Read((stream as MemoryStream)?.ToArray());

        var outputStream = new MemoryStream();
        var sb = new StringBuilder();

        sb.Append($"{STR_NORTHING}, {STR_EASTING}");

        if (griddedReportResult.GriddedData.ReportElevation) sb.Append($", {STR_ELEVATION}");
        if (griddedReportResult.GriddedData.ReportCutFill) sb.Append($", {STR_CUT_FILL}");
        if (griddedReportResult.GriddedData.ReportCmv) sb.Append($", {STR_CMV}");
        if (griddedReportResult.GriddedData.ReportMdp) sb.Append($", {STR_MDP}");
        if (griddedReportResult.GriddedData.ReportPassCount) sb.Append($", {STR_PASS_COUNT}");
        if (griddedReportResult.GriddedData.ReportTemperature) sb.Append($", {STR_TEMPERATURE}");
        sb.Append(STR_INSERT_LINE);

        // Write a header...
        var bytes = Encoding.ASCII.GetBytes(sb.ToString());
        outputStream.Write(bytes, 0, bytes.Length);

        // Write a series of CSV records from the data...
        foreach (GriddedReportDataRowBase row in griddedReportResult.GriddedData.Rows)
        {
          sb.Clear();

          sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:F3}, {1:F3}", row.Northing, row.Easting));
          if (griddedReportResult.GriddedData.ReportElevation)
            sb.Append(string.Format(CultureInfo.InvariantCulture, ", {0:F3}", row.Elevation));
          if (griddedReportResult.GriddedData.ReportCutFill)
            sb.Append(row.CutFill == VelociraptorConstants.NULL_SINGLE
              ? ", "
              : string.Format(CultureInfo.InvariantCulture, ", {0:F3}", row.CutFill));
          if (griddedReportResult.GriddedData.ReportCmv)
            sb.Append(row.Cmv == VelociraptorConstants.NO_CCV
              ? ", "
              : string.Format(CultureInfo.InvariantCulture, ", {0}", row.Cmv));
          if (griddedReportResult.GriddedData.ReportMdp)
            sb.Append(row.Mdp == VelociraptorConstants.NO_MDP
              ? ", "
              : string.Format(CultureInfo.InvariantCulture, ", {0}", row.Mdp));
          if (griddedReportResult.GriddedData.ReportPassCount)
            sb.Append(row.PassCount == VelociraptorConstants.NO_PASSCOUNT
              ? ", "
              : string.Format(CultureInfo.InvariantCulture, ", {0}", row.PassCount));
          if (griddedReportResult.GriddedData.ReportTemperature)
            sb.Append(row.Temperature == VelociraptorConstants.NO_TEMPERATURE
              ? ", "
              : string.Format(CultureInfo.InvariantCulture, ", {0}", row.Temperature));
          sb.Append(STR_INSERT_LINE);

          bytes = Encoding.ASCII.GetBytes(sb.ToString());
          outputStream.Write(bytes, 0, bytes.Length);
        }

        outputStream.Close();
        
        return ExportResult.Create(outputStream.ToArray(), 1);
      }
      catch
      {
        throw CreateServiceException<ExportGridCSVExecutor>();
      }
    }
  }
}

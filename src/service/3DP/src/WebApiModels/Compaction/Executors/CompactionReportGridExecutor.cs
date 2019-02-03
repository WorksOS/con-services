using System;
using System.IO;
#if RAPTOR
using ASNodeDecls;
using ASNodeRaptorReports;
#endif
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Processes the request to get grid details.
  /// </summary>
  public class CompactionReportGridExecutor : RequestExecutorContainer
  {
    public CompactionReportGridExecutor()
    {
      ProcessErrorCodes();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as CompactionReportGridRequest;

        if (request == null)
          ThrowRequestTypeCastException<CompactionReportGridRequest>();
#if RAPTOR
        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_GRIDREPORT"), out var useTrexGateway);

        if (useTrexGateway)
        {
#endif
          var responseData = trexCompactionDataProxy
            .SendGridReportRequest(AutoMapperUtility.Automapper.Map<CompactionReportGridTRexRequest>(request), customHeaders).Result;
          return responseData.Length > 0
            ? ConvertTRexGridResult(request, responseData)
            : CreateNullGridReturnedResult();
#if RAPTOR
        }

        return ProcessWithRaptor(request);
#endif
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

    private CompactionReportResult ConvertTRexGridResult(CompactionReportGridRequest request, Stream stream)
    {
      log.LogDebug($"{nameof(ConvertTRexGridResult)}: Retrieving response data from TRex");

      var griddedReportResult = new GriddedReportResult(ReportType.Gridded);
      griddedReportResult.Read((stream as MemoryStream)?.ToArray());

      var gridRows = new GridRow[griddedReportResult.GriddedData.NumberOfRows];

      // Populate an array of grid rows from the data
      for (var i = 0; i < griddedReportResult.GriddedData.NumberOfRows; i++)
        gridRows[i] = GridRow.CreateRow(griddedReportResult.GriddedData.Rows[i], request);

      var startTime = request.Filter != null && request.Filter.StartUtc.HasValue
        ? request.Filter.StartUtc.Value
        : DateTime.Now;
      var endTime = request.Filter != null && request.Filter.EndUtc.HasValue
        ? request.Filter.EndUtc.Value
        : DateTime.Now;

      var gridReport = GridReport.CreateGridReport(startTime, endTime, gridRows);

      return CompactionReportResult.CreateExportDataResult(gridReport, 1);
    }

#if RAPTOR
    private ContractExecutionResult ProcessWithRaptor(CompactionReportGridRequest request)
    {
      var raptorFilter = RaptorConverters.ConvertFilter(request.Filter);

      var options = RaptorConverters.convertOptions(null, request.LiftBuildSettings, 0,
        request.Filter?.LayerType ?? FilterLayerMethod.None, DisplayMode.Height, false);

      log.LogDebug($"{nameof(ProcessWithRaptor)}: About to call GetReportGrid");

      var args = ASNode.GridReport.RPC.__Global.Construct_GridReport_Args(
        request.ProjectId ?? -1,
        (int)CompactionReportType.Grid,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtProdDataReport),
        RaptorConverters.DesignDescriptor(request.DesignFile),
        request.GridInterval,
        request.ReportElevation,
        request.ReportCutFill,
        request.ReportCMV,
        request.ReportMDP,
        request.ReportPassCount,
        request.ReportTemperature,
        (int)request.GridReportOption,
        request.StartNorthing,
        request.StartEasting,
        request.EndNorthing,
        request.EndEasting,
        request.Azimuth,
        raptorFilter,
        RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
        options
      );

      int raptorResult = raptorClient.GetReportGrid(args, out var responseData);

      if (raptorResult == 1) // icsrrNoError
      {
        return responseData.Length > 0
          ? ConvertGridResult(request, responseData)
          : CreateNullGridReturnedResult();
      }

      throw CreateServiceException<CompactionReportGridExecutor>();
    }

    private CompactionReportResult ConvertGridResult(CompactionReportGridRequest request, Stream stream)
    {
      log.LogDebug($"{nameof(ConvertGridResult)}");

      // Unpack the data for the report and construct a stream containing the result
      var reportPackager = new TRaptorReportsPackager(TRaptorReportType.rrtGridReport)
      {
        ReturnCode = TRaptorReportReturnCode.rrrcUnknownError
      };

      reportPackager.GridReport.ElevationReport = request.ReportElevation;
      reportPackager.GridReport.CutFillReport = request.ReportCutFill;
      reportPackager.GridReport.CMVReport = request.ReportCMV;
      reportPackager.GridReport.MDPReport = request.ReportMDP;
      reportPackager.GridReport.PassCountReport = request.ReportPassCount;
      reportPackager.GridReport.TemperatureReport = request.ReportTemperature;

      log.LogDebug($"{nameof(ConvertGridResult)}: Retrieving response data");

      reportPackager.ReadFromStream(stream);

      var gridRows = new GridRow[reportPackager.GridReport.NumberOfRows];

      // Populate an array of grid rows from the data
      for (var i = 0; i < reportPackager.GridReport.NumberOfRows; i++)
      {
        gridRows[i] = GridRow.CreateRow(reportPackager.GridReport.Rows[i], request);
      }

      var startTime = request.Filter != null && request.Filter.StartUtc.HasValue
        ? request.Filter.StartUtc.Value
        : DateTime.Now;
      var endTime = request.Filter != null && request.Filter.EndUtc.HasValue
        ? request.Filter.EndUtc.Value
        : DateTime.Now;

      var gridReport = GridReport.CreateGridReport(startTime, endTime, gridRows);

      return CompactionReportResult.CreateExportDataResult(gridReport, 1);
    }
#endif
    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }
  }
}

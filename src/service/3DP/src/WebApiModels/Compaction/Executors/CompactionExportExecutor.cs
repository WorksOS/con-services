using System;
using System.IO;
using System.Net;
using ASNodeDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// The executor which passes the summary pass counts request to Raptor V2.
  /// This is the same as ExportReportExecutor V1 but with different error handling.
  /// </summary>
  public class CompactionExportExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionExportExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the exports request by passing the request to Raptor and returning the result.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as ExportReport;

        if (request == null)
          ThrowRequestTypeCastException<ExportReport>();

        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_SURFACE"), out var useTrexGateway);

        if (useTrexGateway && request?.ExportType == ExportTypes.SurfaceExport)
        {
          var cmvChangeDetailsRequest = new CompactionExportRequest(request.ProjectUid, request.Filter, request.Tolerance, request.Filename);

          return trexCompactionDataProxy.SendSurfaceExportRequest(cmvChangeDetailsRequest, customHeaders).Result;
        }

        return ProcessWithRaptor(request);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private ContractExecutionResult ProcessWithRaptor(ExportReport request)
    {
      var raptorFilter = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId);

      bool success = raptorClient.GetProductionDataExport(request.ProjectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtProdDataExport),
        request.UserPrefs, (int)request.ExportType, request.CallerId, raptorFilter,
        RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
        request.TimeStampRequired, request.CellSizeRequired, request.RawData, request.RestrictSize, true,
        request.Tolerance, request.IncludeSurveydSurface,
        request.Precheckonly, request.Filename, request.MachineList, (int)request.CoordType,
        (int)request.OutputType,
        request.DateFromUTC, request.DateToUTC,
        request.Translations, request.ProjectExtents, out TDataExport dataexport);

      if (success)
      {
        try
        {
          return new CompactionExportResult(BuildFilePath(request.ProjectId ?? -1, request.CallerId, request.Filename, true));
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to retrieve received export data: " + ex.Message));
        }
      }

      throw CreateServiceException<CompactionExportExecutor>(dataexport.ReturnCode);
    }

    private string BuildFilePath(long projectid, string callerid, string filename, bool zipped)
    {
      string prodFolder = configStore.GetValueString("RaptorProductionDataFolder");
      return
        $"{prodFolder}\\DataModels\\{projectid}\\Exports\\{callerid}\\{Path.GetFileNameWithoutExtension(filename) + (zipped ? ".zip" : ".csv")}";
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddExportErrorMessages(ContractExecutionStates);
    }
  }
}

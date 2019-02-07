using System;
using System.IO;
using System.Net;
#if RAPTOR
using ASNodeDecls;
#endif
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
        var request = CastRequestObjectTo<ExportReport>(item);

        if (
#if RAPTOR
          UseTRexGateway("ENABLE_TREX_GATEWAY_SURFACE") &&
#endif
          request?.ExportType == ExportTypes.SurfaceExport)
        {
          var compactionSurfaceExportRequest =
            new CompactionSurfaceExportRequest(request.ProjectUid.Value, request.Filter, request.Filename,
              request.Tolerance);

          return trexCompactionDataProxy.SendSurfaceExportRequest(compactionSurfaceExportRequest, customHeaders).Result;
        }
        else if (
#if RAPTOR
          UseTRexGateway("ENABLE_TREX_GATEWAY_VETA") &&
#endif
          request?.ExportType == ExportTypes.VedaExport)
        {
          // todoJeannie note that only OutputTypes.VedaAllPasses is currently supported in 3dp
          var compactionVetaExportRequest =
            new CompactionVetaExportRequest(request.ProjectUid.Value, request.Filter, request.Filename, request.CoordType, request.OutputType, request.MachineNames);

          return trexCompactionDataProxy.SendVetaExportRequest(compactionVetaExportRequest, customHeaders).Result;
        }
        else
          if (
#if RAPTOR
            UseTRexGateway("ENABLE_TREX_GATEWAY_EXPORT_PASSCOUNT") &&
#endif
            request?.ExportType == ExportTypes.PassCountExport)
          {
            var compactionPassCountExportRequest =
              new CompactionPassCountExportRequest(request.ProjectUid.Value, request.Filter, request.Filename, request.CoordType, request.OutputType, request.RestrictSize, request.RawData);

            return trexCompactionDataProxy.SendPassCountExportRequest(compactionPassCountExportRequest, customHeaders).Result;

#if !RAPTOR
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));

        }
#else
          }


          return ProcessWithRaptor(request);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
#if RAPTOR
    private ContractExecutionResult ProcessWithRaptor(ExportReport request)
    {
      var raptorFilter = RaptorConverters.ConvertFilter(request.Filter);

      bool success = raptorClient.GetProductionDataExport(request.ProjectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtProdDataExport),
        RaptorConverters.convertToRaptorUserPreferences(request.UserPrefs), (int)request.ExportType, request.CallerId, raptorFilter,
        RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
        request.TimeStampRequired, request.CellSizeRequired, request.RawData, request.RestrictSize, true,
        request.Tolerance, request.IncludeSurveydSurface,
        request.Precheckonly, request.Filename, RaptorConverters.convertToRaptorMachines(request.MachineList), (int)request.CoordType,
        (int)request.OutputType,
        request.DateFromUTC, request.DateToUTC,
        RaptorConverters.convertToRaptorTranslations(request.Translations), 
        RaptorConverters.convertToRaptorProjectExtents(request.ProjectExtents), out var dataexport);

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
#endif
    private string BuildFilePath(long projectid, string callerid, string filename, bool zipped)
    {
      string prodFolder = configStore.GetValueString("RaptorProductionDataFolder");
      return
        $"{prodFolder}\\DataModels\\{projectid}\\Exports\\{callerid}\\{Path.GetFileNameWithoutExtension(filename) + (zipped ? ".zip" : ".csv")}";
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddExportErrorMessages(ContractExecutionStates);
#endif
    }
  }
}

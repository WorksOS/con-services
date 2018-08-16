using ASNodeDecls;
using SVOICFilterSettings;
using System;
using System.IO;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// The executor which passes the summary pass counts request to Raptor
  /// </summary>
  public class ExportReportExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the summary pass counts request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a PassCountSummaryResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as ExportReport;
      if (request == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Conversion from {item.GetType()} to {typeof(ExportReport)} failed"));
      }

      TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter,
        request.ProjectId);

      bool success = raptorClient.GetProductionDataExport(request.ProjectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.callId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtProdDataExport),
        request.userPrefs, (int)request.exportType, request.callerId, raptorFilter,
        RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
        request.timeStampRequired, request.cellSizeRequired, request.rawData, request.restrictSize, true,
        request.tolerance, request.includeSurveydSurface,
        request.precheckonly, request.filename, request.machineList, (int)request.coordType, (int)request.outputType,
        request.dateFromUTC, request.dateToUTC,
        request.translations, request.projectExtents, out TDataExport dataexport);

      ContractExecutionResult result;

      if (success)
      {
        try
        {
          result = ExportResult.Create(
            File.ReadAllBytes(BuildFilePath(request.ProjectId ?? -1, request.callerId, request.filename, true)),
            dataexport.ReturnCode);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to retrieve received export data: " + ex.Message));
        }
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to get requested export data"));
      }
      return result;
    }
    
    private string BuildFilePath(long projectid, string callerid, string filename, bool zipped)
    {
      string prodFolder = configStore.GetValueString("RaptorProductionDataFolder");
      return
        $"{prodFolder}\\DataModels\\{projectid}\\Exports\\{callerid}\\{Path.GetFileNameWithoutExtension(filename) + (zipped ? ".zip" : ".csv")}";
    }
  }
}

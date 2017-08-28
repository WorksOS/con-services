using ASNodeDecls;
using SVOICFilterSettings;
using System;
using System.IO;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
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
      ContractExecutionResult result = null;
      ExportReport request = item as ExportReport;


 
      TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterID, request.filter,
        request.projectId);
      TDataExport dataexport;

      bool success = raptorClient.GetProductionDataExport(request.projectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid) (request.callId ?? Guid.NewGuid()), 0,
          TASNodeCancellationDescriptorType.cdtProdDataExport),
        request.userPrefs, (int)request.exportType, request.callerId, raptorFilter,
        RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
        request.timeStampRequired, request.cellSizeRequired, request.rawData, request.restrictSize, true,
        request.tolerance, request.includeSurveydSurface,
        request.precheckonly, request.filename, request.machineList, (int)request.coordType, (int)request.outputType,
        request.dateFromUTC, request.dateToUTC,
        request.translations, request.projectExtents, out dataexport);

      if (success)
      {
        try
        {
          result = ExportResult.CreateExportDataResult(
            File.ReadAllBytes(BuildFilePath(request.projectId ?? -1, request.callerId, request.filename, true)),
            dataexport.ReturnCode);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "Failed to get requested export data" + ex.Message));
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
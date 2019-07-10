using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using DesignProfilerDecls;
using VLPDDecls;
#endif
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Processes the request to get design boundaries from Raptor's Project/DataModel.
  /// </summary>
  /// 
  public class DesignExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        log.LogDebug("Getting GeoJson design boundaries from Raptor");

        var request = CastRequestObjectTo<DesignBoundariesRequest>(item);
        JObject[] geoJsons;

        if (fileList != null && fileList.Count > 0)
        {
          var geoJsonList = new List<JObject>();

          for (var i = 0; i < fileList.Count; i++)
          {
            log.LogDebug($"Getting GeoJson design boundary from Raptor for file: {fileList[i].Name}");
#if RAPTOR
            if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_DESIGN_BOUNDARY") ?? false)
#endif
              await ProcessWithTRex(request, fileList[i].ImportedFileUid, fileList[i].Name, geoJsonList);
#if RAPTOR
            else
            {
              string fileSpaceId = FileDescriptorExtensions.GetFileSpaceId(configStore, log);
              var fileDescriptor = FileDescriptor.CreateFileDescriptor(fileSpaceId, fileList[i].Path, fileList[i].Name);

              ProcessWithRaptor(request, fileDescriptor, ref geoJsonList);
            }
#endif
          }

          geoJsons = geoJsonList.ToArray();
        }
        else
        {
          geoJsons = new JObject[0];
        }

        return new DesignResult(geoJsons);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private async Task ProcessWithTRex(DesignBoundariesRequest request, string designUid, string fileName, List<JObject> geoJsonList)
    {
      var siteModelId = request.ProjectUid.ToString();

      var queryParams = new Dictionary<string, string>()
      {
        { "projectUid", siteModelId },
        { "designUid", designUid },
        { "fileName", fileName },
        { "tolerance", request.Tolerance.ToString(CultureInfo.CurrentCulture) }
      };

      var returnedResult = await trexCompactionDataProxy.SendDataGetRequest<DesignBoundaryResult>(siteModelId, "/design/boundaries", customHeaders, queryParams);

      if (returnedResult?.GeoJSON != null)
      {
        var jo = JObject.FromObject(returnedResult.GeoJSON);
        geoJsonList.Add(jo);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Failed to get design boundary for file: {fileName}"));
      }
    }

#if RAPTOR
    private void ProcessWithRaptor(DesignBoundariesRequest request, FileDescriptor fileDescriptor, ref List<JObject> geoJsonList)
    {
      bool result = raptorClient.GetDesignBoundary(
        DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args(
          request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          fileDescriptor.DesignDescriptor(configStore, log, 0, 0),
          DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtJson,
          request.Tolerance,
          TVLPDDistanceUnits.vduMeters,
          0),
        out var memoryStream,
        out var designProfilerResult);

      if (result)
      {
        if (designProfilerResult == TDesignProfilerRequestResult.dppiOK && memoryStream != null && memoryStream.Length > 0)
        {
          // TODO (Aaron) Re do this.
          memoryStream.Position = 0;
          var sr = new StreamReader(memoryStream);
          string geoJSONStr = sr.ReadToEnd();
          var jo = JObject.Parse(geoJSONStr);
          geoJsonList.Add(jo);
        }
      }
      else
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Failed to get design boundary for file: {fileDescriptor.FileName}"));
      }
    }
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}

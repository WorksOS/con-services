using DesignProfilerDecls;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  /// <summary>
  /// Processes the request to get design boundaries from Raptor's Project/DataModel.
  /// </summary>
  /// 
  public class DesignExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        log.LogDebug("Getting GeoJson design boundaries from Raptor");

        DesignBoundariesRequest request = item as DesignBoundariesRequest;

        if (request == null)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Undefined requested data DesignRequest"));
        }

        JObject[] geoJsons = null;

        if (fileList != null && fileList.Count > 0)
        {
          List<JObject> geoJsonList = new List<JObject>();

          for (var i = 0; i < fileList.Count; i++)
          {
            log.LogDebug($"Getting GeoJson design boundary from Raptor for file: {fileList[i].Name}");

            MemoryStream memoryStream;
            TDesignProfilerRequestResult designProfilerResult;
            string fileSpaceId = FileDescriptor.GetFileSpaceId(configStore, log);
            FileDescriptor fileDescriptor = FileDescriptor.CreateFileDescriptor(fileSpaceId, fileList[i].Path, fileList[i].Name);

            bool result = raptorClient.GetDesignBoundary(
              DesignProfiler.ComputeDesignBoundary.RPC.__Global.Construct_CalculateDesignBoundary_Args(
                request.projectId ?? -1,
                fileDescriptor.DesignDescriptor(configStore, log, 0, 0),
                DesignProfiler.ComputeDesignBoundary.RPC.TDesignBoundaryReturnType.dbrtJson,
                request.tolerance,
                TVLPDDistanceUnits.vduMeters,
                0),
              out memoryStream,
              out designProfilerResult);

            if (result)
            {
              if (designProfilerResult == TDesignProfilerRequestResult.dppiOK && memoryStream != null && memoryStream.Length > 0)
              {
                memoryStream.Position = 0;
                var sr = new StreamReader(memoryStream);
                string geoJSONStr = sr.ReadToEnd();
                JObject jo = JObject.Parse(geoJSONStr);
                geoJsonList.Add(jo);
              }
            }
            else
            {
              throw new ServiceException(HttpStatusCode.InternalServerError,
                new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                  $"Failed to get design boundary for file: {fileList[i].Name}"));
            }
          }

          geoJsons = geoJsonList.ToArray();
        }
        else
        {
          geoJsons = new JObject[0];
        }

        return DesignResult.CreateDesignResult(geoJsons); ;
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}
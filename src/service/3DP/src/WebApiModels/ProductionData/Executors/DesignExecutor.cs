using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Processes the request to get design boundaries from Raptor's Project/DataModel.
  /// </summary>
  public class DesignExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        log.LogDebug("Fetching GeoJson design boundaries");

        var request = CastRequestObjectTo<DesignBoundariesRequest>(item);
        JObject[] geoJsons;

        if (fileList != null && fileList.Count > 0)
        {
          var geoJsonList = new List<JObject>();

          for (var i = 0; i < fileList.Count; i++)
          {
            log.LogDebug($"Fetching GeoJson design boundary for file: {fileList[i].Name}");
            await ProcessWithTRex(request, fileList[i].ImportedFileUid, fileList[i].Name, geoJsonList);
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

      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>( "projectUid", siteModelId ),
        new KeyValuePair<string, string>( "designUid", designUid ),
        new KeyValuePair<string, string>( "fileName", fileName ),
        new KeyValuePair<string, string>( "tolerance", request.Tolerance.ToString(CultureInfo.CurrentCulture))
      };

      var returnedResult = await trexCompactionDataProxy.SendDataGetRequest<DesignBoundaryResult>(siteModelId, "/design/boundaries", customHeaders, queryParams);

      if (returnedResult?.GeoJSON != null)
      {
        geoJsonList.Add(JObject.FromObject(returnedResult.GeoJSON));
      }
      else
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Failed to get design boundary for file: {fileName}"));
      }
    }
  }
}

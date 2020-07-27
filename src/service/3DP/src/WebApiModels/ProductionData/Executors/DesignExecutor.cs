using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models.MapHandling;
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

        if (fileList != null && fileList.Count > 0)
        {
          var geoJsonList = new List<GeoJson>(fileList.Count);

          for (var i = 0; i < fileList.Count; i++)
          {
            log.LogDebug($"Fetching GeoJson design boundary for file: {fileList[i].Name}");
            var geoJson = await GetDesignBoundary(request, fileList[i].ImportedFileUid, fileList[i].Name);

            ConvertFromRadiansToDegrees(geoJson);

            geoJsonList.Add(geoJson);
          }

          return new DesignResult(geoJsonList);
        }

        return new DesignResult(null);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private void ConvertFromRadiansToDegrees(GeoJson geoJson)
    {
      foreach (var feature in geoJson.Features)
      {
        foreach (var coordList in feature.Geometry.Coordinates)
        {
          foreach (var coordPair in coordList)
          {
            // GeoJson uses lon/lat.
            coordPair[0] = coordPair[0].LonRadiansToDegrees();
            coordPair[1] = coordPair[1].LatRadiansToDegrees();
          }
        }
      }
    }

    private async Task<GeoJson> GetDesignBoundary(DesignBoundariesRequest request, string designUid, string fileName)
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
        return returnedResult.GeoJSON;
      }

      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
        $"Failed to get design boundary for file: {fileName}"));
    }
  }
}

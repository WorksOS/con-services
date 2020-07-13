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
using VSS.Productivity3D.Productivity3D.Models.Designs;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class AlignmentGeometryExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
#if RAPTOR
        throw new ServiceException("Master alignment geometry request is not support for Raptor");
#endif

        log.LogDebug("Getting master alignment geometry from TRex");

        var request = CastRequestObjectTo<AlignmentGeometryRequest>(item);

        var siteModelId = request.ProjectUid.ToString();
        var convertArcsToChords = request.ConvertArcsToChords.ToString();
        var arcChordTolerance = request.ArcChordTolerance.ToString(CultureInfo.InvariantCulture);

        JObject[] geoJsons;
        var geoJsonList = new List<JObject>();

        const string LOG_MSG = "Getting GeoJson alignment geometry for file";

        if (request.DesignUid == null)
        {
          if (fileList?.Count > 0)
          { 
            for (var i = 0; i < fileList.Count; i++)
            {
              log.LogDebug($"{LOG_MSG}: {fileList[i].Name}");
              await ProcessRequest(siteModelId, fileList[i].ImportedFileUid, convertArcsToChords, arcChordTolerance, fileList[i].Name, geoJsonList);
            }

            geoJsons = geoJsonList.ToArray();
          }
          else
            geoJsons = new JObject[0];
         }
        else
        {
          log.LogDebug($"{LOG_MSG} ID: {request.DesignUid}");

          await ProcessRequest(siteModelId, request.DesignUid.ToString(), convertArcsToChords, arcChordTolerance, request.FileName, geoJsonList);
          geoJsons = geoJsonList.ToArray();
        }

        return new AlignmentResult(geoJsons);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private async Task ProcessRequest(string siteModelId, string designUid, string convertArcsToChords, string arcChordTolerance, string fileName, List<JObject> geoJsonList)
    {
      var queryParams = new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>( "projectUid", siteModelId ),
          new KeyValuePair<string, string>( "designUid", designUid ),
          new KeyValuePair<string, string>( "fileName", fileName ),
          new KeyValuePair<string, string>( "convertArcsToChords", convertArcsToChords ),
          new KeyValuePair<string, string>( "arcChordTolerance", arcChordTolerance )
        };

      var returnedResult = await trexCompactionDataProxy.SendDataGetRequest<AlignmentDesignGeometryResult>(siteModelId, "/design/alignment/master/geometry", customHeaders, queryParams);

      if (returnedResult?.GeoJSON != null)
        geoJsonList.Add(JObject.FromObject(returnedResult.GeoJSON));
      else
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Failed to get alignment center line geometry for alignment: {designUid}"));
      }
    }
  }
}

using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Designs;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

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

        log.LogDebug($"Getting an alignment geometry for file ID: {request.DesignUid}");

        return await ProcessRequest(siteModelId, request.DesignUid.ToString(), convertArcsToChords, arcChordTolerance, request.FileName);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    private async Task<AlignmentGeometryResult> ProcessRequest(string siteModelId, string designUid, string convertArcsToChords, string arcChordTolerance, string fileName)
    {
      var queryParams = new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>( "projectUid", siteModelId ),
          new KeyValuePair<string, string>( "designUid", designUid ),
          new KeyValuePair<string, string>( "fileName", fileName ),
          new KeyValuePair<string, string>( "convertArcsToChords", convertArcsToChords ),
          new KeyValuePair<string, string>( "arcChordTolerance", arcChordTolerance )
        };

      var returnedResult = await trexCompactionDataProxy.SendDataGetRequest<AlignmentGeometryResult>(siteModelId, "/design/alignment/master/geometry", customHeaders, queryParams);

      if (returnedResult?.Code == 0 && returnedResult?.AlignmentGeometry != null)
        return returnedResult;

      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"Failed to get alignment center line geometry for alignment: {designUid}"));
    }
  }
}

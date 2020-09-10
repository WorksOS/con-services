using System.Threading.Tasks;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.WebApi.Models.Coord.Executors
{
  /// <summary>
  /// Get coordinate system definition file executor.
  /// </summary>
  public class CoordinateSystemExecutorGet : CoordinateSystemExecutor
  {
    protected override async Task<CoordinateSystemSettings> SendRequestToTRexGatewayClient(object item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);

      var siteModelId = request.ProjectUid.ToString();

      return await trexCompactionDataProxy.SendDataGetRequest<CoordinateSystemSettings>(siteModelId, $"/projects/{siteModelId}/coordsystem", customHeaders);
    }
  }
}

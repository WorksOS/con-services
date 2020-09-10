using System.Threading.Tasks;
using VSS.Productivity3D.Models.Interfaces;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;

namespace VSS.Productivity3D.WebApi.Models.Coord.Executors
{
  /// <summary>
  /// Post coordinate system definition file executor.
  /// </summary>
  public class CoordinateSystemExecutorPost : CoordinateSystemExecutor
  {
    protected override Task<CoordinateSystemSettings> SendRequestToTRexGatewayClient(object item)
    {
      if (!(item is IIsProjectIDApplicable file))
      {
        return null;
      }

      if (file.HasProjectID())
      {
        var request = file as CoordinateSystemFile;
        return trexCompactionDataProxy.SendDataPostRequest<CoordinateSystemSettings, CoordinateSystemFile>(request, "/coordsystem", customHeaders, true);
      }

      var validationRequest = file as CoordinateSystemFileValidationRequest;
      return trexCompactionDataProxy.SendDataPostRequest<CoordinateSystemSettings, CoordinateSystemFileValidationRequest>(validationRequest, "/coordsystem/validation", customHeaders);
    }
  }
}

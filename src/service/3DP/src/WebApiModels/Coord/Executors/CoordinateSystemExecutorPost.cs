using System.IO;
using System.Threading.Tasks;
using VSS.Productivity3D.Common;
#if RAPTOR
using ASNodeDecls;
using VLPDDecls;
#endif
using VSS.Productivity3D.Models.Interfaces;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.WebApi.Models.Interfaces;

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
        return null;

      if (file.HasProjectID())
      {
        var request = file as CoordinateSystemFile;
        return trexCompactionDataProxy.SendDataPostRequest<CoordinateSystemSettings, CoordinateSystemFile>(request, "/coordsystem", customHeaders, true);
      }

      var validationRequest = file as CoordinateSystemFileValidationRequest;
      return trexCompactionDataProxy.SendDataPostRequest<CoordinateSystemSettings, CoordinateSystemFileValidationRequest>(validationRequest, "/coordsystem/validation", customHeaders);
    }

#if RAPTOR
    protected override TASNodeErrorStatus SendRequestToPDSClient(object item)
    {
      var code = TASNodeErrorStatus.asneUnknown;

      var tempCoordSystemSettings = new TCoordinateSystemSettings();

      if (item is IIsProjectIDApplicable file)
      {
        if (file.HasProjectID())
        {
          var request = file as CoordinateSystemFile;
          code = raptorClient.PassSelectedCoordinateSystemFile(new MemoryStream(request.CSFileContent), request.CSFileName, request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, out tempCoordSystemSettings);
        }
        else
        {
          var request = file as CoordinateSystemFileValidationRequest;
          code = raptorClient.PassSelectedCoordinateSystemFile(new MemoryStream(request.CSFileContent), request.CSFileName, -1, out tempCoordSystemSettings);
        }
      }

      if (code == TASNodeErrorStatus.asneOK)
        coordSystemSettings = tempCoordSystemSettings;

      return code;
    }
#endif
  }
}

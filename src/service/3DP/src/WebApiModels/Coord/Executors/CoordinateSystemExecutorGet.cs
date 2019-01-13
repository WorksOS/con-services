using ASNodeDecls;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Coord.Executors
{
  /// <summary>
  /// Get coordinate system definition file executor.
  /// </summary>
  public class CoordinateSystemExecutorGet : CoordinateSystemExecutor
  {
    protected override TASNodeErrorStatus SendRequestToPDSClient(object item)
    {
      var request = item as ProjectID;
      var code = raptorClient.RequestCoordinateSystemDetails(request.ProjectId ?? -1, out var tempCoordSystemSettings);

      if (code == TASNodeErrorStatus.asneOK)
        coordSystemSettings = tempCoordSystemSettings;

      return code;
    }
  }
}

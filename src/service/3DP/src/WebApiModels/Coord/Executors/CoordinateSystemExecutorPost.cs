using System.IO;
using ASNodeDecls;
using VLPDDecls;
using VSS.Productivity3D.WebApi.Models.Coord.Models;
using VSS.Productivity3D.WebApi.Models.Interfaces;

namespace VSS.Productivity3D.WebApi.Models.Coord.Executors
{
  /// <summary>
  /// Post coordinate system definition file executor.
  /// </summary>
  public class CoordinateSystemExecutorPost : CoordinateSystemExecutor
  {
    protected override TASNodeErrorStatus SendRequestToPDSClient(object item)
    {
      var code = TASNodeErrorStatus.asneUnknown;

      var tempCoordSystemSettings = new TCoordinateSystemSettings();

      if (item is IIsProjectIDApplicable file)
      {
        if (file.HasProjectID())
        {
          var request = file as CoordinateSystemFile;
          code = raptorClient.PassSelectedCoordinateSystemFile(new MemoryStream(request.csFileContent), request.csFileName, request.ProjectId ?? -1, out tempCoordSystemSettings);
        }
        else
        {
          var request = file as CoordinateSystemFileValidationRequest;
          code = raptorClient.PassSelectedCoordinateSystemFile(new MemoryStream(request.csFileContent), request.csFileName, -1, out tempCoordSystemSettings);
        }
      }

      if (code == TASNodeErrorStatus.asneOK)
        coordSystemSettings = tempCoordSystemSettings;

      return code;
    }
  }
}

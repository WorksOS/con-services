using ASNodeDecls;
using System.IO;
using VLPDDecls;
using VSS.Productivity3D.WebApiModels.Coord.Models;
using VSS.Productivity3D.WebApiModels.Interfaces;

namespace VSS.Productivity3D.WebApiModels.Coord.Executors
{
  /// <summary>
  /// Post coordinate system definition file executor.
  /// </summary>
  /// 
  public class CoordinateSystemExecutorPost : CoordinateSystemExecutor
  {
    protected override TASNodeErrorStatus SendRequestToPDSClient(object item)
    {

      TASNodeErrorStatus code = TASNodeErrorStatus.asneUnknown;
      TCoordinateSystemSettings tempCoordSystemSettings = new TCoordinateSystemSettings();

      if (item is IIsProjectIDApplicable)
      {
        if ((item as IIsProjectIDApplicable).HasProjectID())
        {
          CoordinateSystemFile request = item as CoordinateSystemFile;
          code = raptorClient.PassSelectedCoordinateSystemFile(new MemoryStream(request.csFileContent), request.csFileName, request.ProjectId ?? -1, out tempCoordSystemSettings);
        }
        else
        {
          CoordinateSystemFileValidationRequest request = item as CoordinateSystemFileValidationRequest;
          code = raptorClient.PassSelectedCoordinateSystemFile(new MemoryStream(request.csFileContent), request.csFileName, -1, out tempCoordSystemSettings);
        };
      };

      if (code == TASNodeErrorStatus.asneOK)
        coordSystemSettings = tempCoordSystemSettings;

      return code;
    }
  }
}

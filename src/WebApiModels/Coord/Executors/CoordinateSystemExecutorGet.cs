using ASNodeDecls;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.Coord.Executors
{
  /// <summary>
  /// Get coordinate system definition file executor.
  /// </summary>
  /// 
  public class CoordinateSystemExecutorGet : CoordinateSystemExecutor
  {

    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    /// 
    public CoordinateSystemExecutorGet(ILoggerFactory logger, IASNodeClient raptorClient)
        : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CoordinateSystemExecutorGet()
    {
    }

    protected override TASNodeErrorStatus SendRequestToPDSClient(object item)
    {
      TCoordinateSystemSettings tempCoordSystemSettings;

      ProjectID request = item as ProjectID;
      TASNodeErrorStatus code = raptorClient.RequestCoordinateSystemDetails(request.projectId ?? -1, out tempCoordSystemSettings);

      if (code == TASNodeErrorStatus.asneOK)
        coordSystemSettings = tempCoordSystemSettings;

      return code;
    }

  }
}
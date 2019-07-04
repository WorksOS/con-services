using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;

namespace VSS.TRex.Gateway.Common.Executors.Coords
{
  /// <summary>
  /// Processes the request to post coordinate system definition data.
  /// </summary>
  public class CoordinateSystemGetExecutor : CoordinateSystemBaseExecutor
  {
    public CoordinateSystemGetExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CoordinateSystemGetExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as ProjectID;

      if (request == null)
        ThrowRequestTypeCastException<ProjectID>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var csib = siteModel.CSIB();

      if (csib ==  string.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          $"The project does not have Coordinate System definition data. Project UID: {siteModel.ID}"));

      var csd = DIContext.Obtain<IConvertCoordinates>().CSIBContentToCSD(csib);

      if (csd.CoordinateSystem == null || csd.CoordinateSystem.ZoneInfo == null || csd.CoordinateSystem.DatumInfo == null)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to convert CSIB content to Coordinate System definition data."));

      return ConvertResult("", csd.CoordinateSystem);
    }
  }
}

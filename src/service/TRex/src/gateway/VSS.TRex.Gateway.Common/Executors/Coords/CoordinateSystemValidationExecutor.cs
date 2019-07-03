using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using CSFileValidationRequest = VSS.Productivity3D.Models.Models.Coords.CoordinateSystemFileValidationRequest;

namespace VSS.TRex.Gateway.Common.Executors.Coords
{
  /// <summary>
  /// Processes the request to validate coordinate system definition data.
  /// </summary>
  public class CoordinateSystemValidationExecutor : CoordinateSystemBaseExecutor
  {
    public CoordinateSystemValidationExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CoordinateSystemValidationExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CSFileValidationRequest;

      if (request == null)
        ThrowRequestTypeCastException<CSFileValidationRequest>();

      var csd = DIContext.Obtain<IConvertCoordinates>().DCFileContentToCSD(request.CSFileName, request.CSFileContent);

      if (csd.CoordinateSystem != null && csd.CoordinateSystem.ZoneInfo != null && csd.CoordinateSystem.DatumInfo != null)
        return ConvertResult(request.CSFileName, csd.CoordinateSystem);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        $"Failed to convert DC File {request.CSFileName} content to Coordinate System definition data."));
    }

  }
}

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.GridFabric.Arguments;
using VSS.TRex.CoordinateSystems.GridFabric.Requests;
using VSS.TRex.DI;

namespace VSS.TRex.Gateway.Common.Executors.Coords
{
  /// <summary>
  /// Processes the request to post coordinate system definition data.
  /// </summary>
  public class CoordinateSystemPostExecutor : CoordinateSystemBaseExecutor
  {
    public CoordinateSystemPostExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CoordinateSystemPostExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CoordinateSystemFile;

      if (request == null)
        ThrowRequestTypeCastException<CoordinateSystemFile>();

      var csd = await DIContext.Obtain<IConvertCoordinates>().DCFileContentToCSD(request.CSFileName, request.CSFileContent);

      if (csd.CoordinateSystem == null || csd.CoordinateSystem.ZoneInfo == null || csd.CoordinateSystem.DatumInfo == null)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          $"Failed to convert DC File {request.CSFileName} content to Coordinate System definition data."));

      var projectUid = request.ProjectUid ?? Guid.Empty;

      var addCoordinateSystemRequest = new AddCoordinateSystemRequest();

      var addCoordSystemResponse = await addCoordinateSystemRequest.ExecuteAsync(new AddCoordinateSystemArgument()
      {
        ProjectID = projectUid,
        CSIB = csd.CoordinateSystem.Id
      });

      if (addCoordSystemResponse?.Succeeded ?? false)
        return ConvertResult(request.CSFileName, csd.CoordinateSystem);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        $"Failed to post Coordinate System definition data. Project UID: {projectUid}"));
    }

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}

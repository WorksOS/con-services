using System;
using System.Net;
using System.Threading.Tasks;
using CoreX.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models;
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
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CoordinateSystemGetExecutor()
    { }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);
      var siteModel = GetSiteModel(request.ProjectUid);
      var csib = siteModel.CSIB();

      if (csib == string.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          $"The project does not have Coordinate System definition data. Project UID: {siteModel.ID}"));
      }

      var coordinateSystem = DIContext
        .Obtain<ICoreXWrapper>()
        .GetCSDFromCSIB(csib);

      if (coordinateSystem == null || coordinateSystem.ZoneInfo == null || coordinateSystem.DatumInfo == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to convert CSIB content to Coordinate System definition data."));
      }

      return ConvertResult("", coordinateSystem);
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

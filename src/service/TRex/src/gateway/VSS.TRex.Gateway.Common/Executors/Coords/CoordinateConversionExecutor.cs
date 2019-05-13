using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors.Coords
{
  /// <summary>
  /// Coordinate conversion executor.
  /// </summary>
  public class CoordinateConversionExecutor : BaseExecutor
  {
    public CoordinateConversionExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CoordinateConversionExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CoordinateConversionRequest;

      if (request == null)
        ThrowRequestTypeCastException<CoordinateConversionRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var csib = siteModel.CSIB();

      if (csib == string.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          $"The project does not have Coordinate System definition data. Project UID: {siteModel.ID}"));

      var coordinates = request.ConversionCoordinates.Select(cc => new XYZ(cc.X, cc.Y)).ToArray();

      (RequestErrorStatus errorStatus, XYZ[] resultCoordinates) conversionResult;

      switch (request.ConversionType)
      {
        case TwoDCoordinateConversionType.NorthEastToLatLon:
          conversionResult = ConvertCoordinates.NEEToLLH(csib, coordinates);
          break;
        case TwoDCoordinateConversionType.LatLonToNorthEast:
          conversionResult = ConvertCoordinates.LLHToNEE(csib, coordinates);
          break;
        default:
          throw new ArgumentException($"Unknown TwoDCoordinateConversionType {Convert.ToInt16(request.ConversionType)}");
      }

      if (conversionResult.errorStatus == RequestErrorStatus.OK)
        return CreateConversionResult(conversionResult.resultCoordinates);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        $"Coordinate conversion failed. Error status: {conversionResult.errorStatus}"));
    }

    /// <summary>
    /// Returns an instance of the CoordinateConversionResult class as execution result.
    /// </summary>
    private ContractExecutionResult CreateConversionResult(XYZ[] pointList)
    {
      TwoDConversionCoordinate[] convertedPoints = pointList?.Select(cp => new TwoDConversionCoordinate(cp.X, cp.Y)).ToArray();

      return new CoordinateConversionResult(convertedPoints);
    }

  }
}

using System.Linq;
using System.Net;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;

namespace VSS.Productivity3D.WebApi.Models.Coord.Executors
{
  /// <summary>
  /// Coordinate conversion executor.
  /// </summary>
  /// 
  public class CoordinateConversionExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CoordinateConversionExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Populates ContractExecutionStates with Production Data Server error messages.
    /// </summary>
    /// 
    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddCoordinateResultErrorMessages(ContractExecutionStates);
#endif
    }

    /// <summary>
    /// Coordinate conversion executor (Post).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">A Domain object.</param>
    /// <returns></returns>
    /// 
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      if (item != null)
      {
        try
        {
          var request = CastRequestObjectTo<CoordinateConversionRequest>(item);
#if RAPTOR
          if (UseTRexGateway("ENABLE_TREX_GATEWAY_CS") || UseTRexGateway("ENABLE_TREX_GATEWAY_PROJECTSTATISTICS"))
          {
#endif
          return trexCompactionDataProxy.SendDataPostRequest<CoordinateConversionResult, CoordinateConversionRequest>(request, "/coordinateconversion", customHeaders).Result;
#if RAPTOR
          }

          var latLongs = new TWGS84FenceContainer { FencePoints = request.ConversionCoordinates.Select(cc => TWGS84Point.Point(cc.X, cc.Y)).ToArray() };

          var code = raptorClient.GetGridCoordinates
            (
              request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, 
              latLongs, 
              request.ConversionType == TwoDCoordinateConversionType.LatLonToNorthEast ? TCoordConversionType.ctLLHtoNEE : TCoordConversionType.ctNEEtoLLH, 
              out var pointList
            );

          if (code == TCoordReturnCode.nercNoError)
            return ExecutionResult(pointList.Points.Coords);

          throw CreateServiceException<CoordinateConversionExecutor>((int)code);
#endif
        }
        finally
        {
          ContractExecutionStates.ClearDynamic();
        }
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "No  coordinate conversion request sent."));
    }

#if RAPTOR
    /// <summary>
    /// Returns an instance of the ContractExecutionResult class as POST method execution result.
    /// </summary>
    private ContractExecutionResult ExecutionResult(TCoordPoint[] pointList)
    {
      TwoDConversionCoordinate[] convertedPoints = pointList?.Select(cp => new TwoDConversionCoordinate(cp.X, cp.Y)).ToArray();

      return new CoordinateConversionResult(convertedPoints);
    }
#endif
  }
}

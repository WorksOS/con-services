using System.Linq;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Coord.Models;
using VSS.Productivity3D.WebApiModels.Coord.ResultHandling;

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
      RaptorResult.AddCoordinateResultErrorMessages(ContractExecutionStates);
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

          var latLongs = new TWGS84FenceContainer { FencePoints = request.conversionCoordinates.Select(cc => TWGS84Point.Point(cc.x, cc.y)).ToArray() };

          var code = raptorClient.GetGridCoordinates
            (
              request.ProjectId ?? -1, 
              latLongs, 
              request.conversionType == TwoDCoordinateConversionType.LatLonToNorthEast ? TCoordConversionType.ctLLHtoNEE : TCoordConversionType.ctNEEtoLLH, 
              out var pointList
            );

          if (code == TCoordReturnCode.nercNoError)
            return ExecutionResult(pointList.Points.Coords);

          throw CreateServiceException<CoordinateConversionExecutor>((int)code);
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

    /// <summary>
    /// Returns an instance of the ContractExecutionResult class as POST method execution result.
    /// </summary>
    private ContractExecutionResult ExecutionResult(TCoordPoint[] pointList)
    {
      TwoDConversionCoordinate[] convertedPoints = pointList?.Select(cp => TwoDConversionCoordinate.CreateTwoDConversionCoordinate(cp.X, cp.Y)).ToArray();

      return CoordinateConversionResult.CreateCoordinateConversionResult(convertedPoints);
    }
  }
}

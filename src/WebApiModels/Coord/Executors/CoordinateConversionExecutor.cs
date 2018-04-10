using System.Linq;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Coord.Models;
using VSS.Productivity3D.WebApiModels.Coord.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Coord.Executors
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
      RaptorResult.AddErrorMessages(ContractExecutionStates);
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
      ContractExecutionResult result = null;

      if ((object)item != null)
      {
        try
        {
          CoordinateConversionRequest request = item as CoordinateConversionRequest;

          TWGS84FenceContainer latLongs = new TWGS84FenceContainer { FencePoints = request.conversionCoordinates.Select(cc => TWGS84Point.Point(cc.x, cc.y)).ToArray() };

          TCoordPointList pointList;

          TCoordReturnCode code = raptorClient.GetGridCoordinates
            (
              request.projectId ?? -1, 
              latLongs, 
              request.conversionType == TwoDCoordinateConversionType.LatLonToNorthEast ? TCoordConversionType.ctLLHtoNEE : TCoordConversionType.ctNEEtoLLH, 
              out pointList
            );

          if (code == TCoordReturnCode.nercNoError)
            result = ExecutionResult(pointList.Points.Coords);
          else
          {
            throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                                       string.Format("Failed to process coordinate conversion request with error: {0}.", ContractExecutionStates.FirstNameWithOffset((int)code))));
          }
        }
        finally
        {
          ContractExecutionStates.ClearDynamic();
        }
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "No  coordinate conversion request sent."));
      }

      return result;
    }

    /// <summary>
    /// Returns an instance of the ContractExecutionResult class as POST method execution result.
    /// </summary>
    /// <returns>An instance of the ContractExecutionResult class.</returns>
    /// 
    private ContractExecutionResult ExecutionResult(TCoordPoint[] pointList)
    {
      TwoDConversionCoordinate[] convertedPoints = pointList != null ? pointList.Select(cp => TwoDConversionCoordinate.CreateTwoDConversionCoordinate(cp.X, cp.Y)).ToArray() : null;

      return CoordinateConversionResult.CreateCoordinateConversionResult(convertedPoints);
    }
  }
}
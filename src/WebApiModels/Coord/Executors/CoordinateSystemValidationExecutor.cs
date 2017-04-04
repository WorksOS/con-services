using ASNodeDecls;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Coord.Models;
using VLPDDecls;
using VSS.Raptor.Service.WebApiModels.Coord.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Coord.Executors
{
  /// <summary>
  /// Coordinate system defination file validation executor.
  /// </summary>
  /// 
  public class CoordinateSystemValidationExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    /// 
    public CoordinateSystemValidationExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
      // ...
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build...
    /// </summary>
    public CoordinateSystemValidationExecutor()
    {
      // ...
    }

    /// <summary>
    /// Populates ContractExecutionStates with Production Data Server error messages.
    /// </summary>
    /// 
    protected override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }

    /// <summary>
    ///Coordinate system defination file validation (Post).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">A Domain object.</param>
    /// <returns></returns>
    /// 
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;

      if (item != null)
      {
        try
        {
          TCoordinateSystemSettings tempCoordSystemSettings;

          CoordinateSystemFileValidationRequest request = item as CoordinateSystemFileValidationRequest;

          TASNodeErrorStatus code = raptorClient.PassSelectedCoordinateSystemFile(new MemoryStream(request.csFileContent), request.csFileName, -1, out tempCoordSystemSettings);

          result = CoordinateSystemValidationResult.CreateCoordinateSystemValidationResult(code == TASNodeErrorStatus.asneOK);
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

  }
}

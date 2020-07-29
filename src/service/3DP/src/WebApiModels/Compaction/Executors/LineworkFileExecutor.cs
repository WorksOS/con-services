using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models.Files;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Executor for processing DXF linework files.
  /// </summary>
  public class LineworkFileExecutor : RequestExecutorContainer
  {
    public LineworkFileExecutor()
    {
      ProcessErrorCodes();
    }

    protected sealed override void ProcessErrorCodes()
    { }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      if (!(item is DxfFileRequest dxfRequest))
      {
        throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Request is not a DxfFileRequest"));
      }

      var request = new LineworkRequest(dxfRequest).Validate();

      return await ProcessForTRex(request);
    }

    private async Task<DxfLineworkFileResult> ProcessForTRex(LineworkRequest request)
    {
      try
      {
        log.LogDebug($"{nameof(LineworkFileExecutor)}::{nameof(ProcessForTRex)}()");

        var req = new DXFBoundariesRequest(request.CoordinateSystemFileData, ImportedFileType.SiteBoundary,
          request.DxfFileData, (DxfUnitsType)request.LineworkUnits, (uint)request.NumberOfBoundariesToProcess,
          request.ConvertLineStringCoordsToPolygon);
        var returnResult = await trexCompactionDataProxy.SendDataPostRequest<DXFBoundaryResult, DXFBoundariesRequest>(req, "files/dxf/boundaries");

        log.LogInformation($"RequestBoundariesFromLineWork: result: {JsonConvert.SerializeObject(returnResult)}");

        if (returnResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        {
          throw CreateServiceException<LineworkFileExecutor>(returnResult.Code);
        }

        return new DxfLineworkFileResult(returnResult.Boundaries, returnResult.Code, returnResult.Message);
      }
      catch (ServiceException ex)
      {
        var errorMessage = ex.GetResult.Message;

        log.LogError($"RequestBoundariesFromLinework: exception {errorMessage}");

        return new DxfLineworkFileResult(ContractExecutionStatesEnum.InternalProcessingError, errorMessage, null);
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}

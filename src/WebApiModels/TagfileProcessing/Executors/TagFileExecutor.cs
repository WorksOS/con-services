using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{
  /// <summary>
  /// For submitting TAG files to Raptor.
  /// </summary>
  public class TagFileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build.
    /// </summary>
    public TagFileExecutor()
    {
      ProcessErrorCodes();
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddTagProcessorErrorMessages(ContractExecutionStates);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as TagFileRequestLegacy;

        if (request.ProjectId != VelociraptorConstants.NO_PROJECT_ID && request.Boundary == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to process tagfile with error: Manual tag file submissions must include a boundary fence."));
        }

        if (request.ProjectId == VelociraptorConstants.NO_PROJECT_ID && request.Boundary != null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to process tagfile with error: Automatic tag file submissions cannot include boundary fence."));
        }

        var resultCode = tagProcessor.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor
          (request.FileName,
            new MemoryStream(request.Data),
            request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, 0, 0, request.MachineId ?? -1,
            request.Boundary != null
              ? RaptorConverters.convertWGS84Fence(request.Boundary)
              : TWGS84FenceContainer.Null(), request.TccOrgId);

        if (resultCode == TTAGProcServerProcessResult.tpsprOK)
        {
          return TagFilePostResult.Create();
        }
        else
        {
          if (resultCode == TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions)
          {
            var result = new ContractExecutionResult(
              ContractExecutionStates.GetErrorNumberwithOffset((int)resultCode),
              $"Failed to process tagfile with error: {ContractExecutionStates.FirstNameWithOffset((int)resultCode)}");

            log.LogInformation(JsonConvert.SerializeObject(result));

            // Serialize the request ignoring the Data property so not to overwhelm the logs.
            var serializedRequest = JsonConvert.SerializeObject(
              request,
              Formatting.None,
              new JsonSerializerSettings
              {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                ContractResolver = new JsonContractPropertyResolver("Data")
              });

            log.LogInformation("TAG file submission request with file data omitted:" + JsonConvert.SerializeObject(serializedRequest));

            return result;
          }

          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStates.GetErrorNumberwithOffset((int)resultCode),
              $"Failed to process tagfile with error: {ContractExecutionStates.FirstNameWithOffset((int)resultCode)}"));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}

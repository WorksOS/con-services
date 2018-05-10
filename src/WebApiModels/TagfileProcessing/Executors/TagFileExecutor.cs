using System.IO;
using System.Net;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
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
    /// Default constructor for RequestExecutorContainer.Build
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
        var request = item as TagFileRequest;

        var returnResult = tagProcessor.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor
          (request.FileName,
            new MemoryStream(request.Data),
            request.ProjectId ?? -1, 0, 0, request.MachineId ?? -1,
            request.Boundary != null
              ? RaptorConverters.convertWGS84Fence(request.Boundary)
              : TWGS84FenceContainer.Null(), request.TccOrgId);

        if (returnResult == TTAGProcServerProcessResult.tpsprOK)
        {
          return TagFilePostResult.Create();
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStates.GetErrorNumberwithOffset((int)returnResult),
              $"Failed to process tagfile with error: {ContractExecutionStates.FirstNameWithOffset((int)returnResult)}"));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}

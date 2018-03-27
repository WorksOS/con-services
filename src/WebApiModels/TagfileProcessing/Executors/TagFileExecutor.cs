using System.IO;
using System.Net;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.Models;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.TagfileProcessing.Executors
{
  /// <summary>
  /// TagFileExecutor for submitting tag files to Raptor
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


    /// <summary>
    /// ContractExecutionResult
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        TagFileRequest request = item as TagFileRequest;

        TTAGProcServerProcessResult returnResult = tagProcessor.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor
          (request.fileName,
            new MemoryStream(request.data),
            request.projectId ?? -1, 0, 0, request.machineId ?? -1,
            request.boundary != null
              ? RaptorConverters.convertWGS84Fence(request.boundary)
              : TWGS84FenceContainer.Null(),request.tccOrgId);

        if (returnResult == TTAGProcServerProcessResult.tpsprOK)
          return TAGFilePostResult.CreateTAGFilePostResult();
        else
          throw new ServiceException(HttpStatusCode.BadRequest,
                  new ContractExecutionResult(ContractExecutionStates.GetErrorNumberwithOffset((int)returnResult),
                    $"Failed to process tagfile with error: {ContractExecutionStates.FirstNameWithOffset((int)returnResult)}"));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}
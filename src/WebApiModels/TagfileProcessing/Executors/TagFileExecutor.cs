using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.Models;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.ResultHandling;
using TAGProcServiceDecls;

namespace VSS.Raptor.Service.WebApiModels.TagfileProcessing.Executors
{
    /// <summary>
    /// TagFileExecutor for submitting tag files to Raptor
    /// </summary>
    public class TagFileExecutor : RequestExecutorContainer
    {
      /// <summary>
      /// This constructor allows us to mock raptorClient & tagProcessor
      /// </summary>
      /// <param name="raptorClient"></param>
      /// <param name="tagProcessor"></param>
      public TagFileExecutor(ILoggerFactory logger, IASNodeClient raptorClient, ITagProcessor tagProcessor) : base(logger, raptorClient, tagProcessor)
      {
      }

      /// <summary>
      /// Default constructor for RequestExecutorContainer.Build
      /// </summary>
      public TagFileExecutor()
      {
      }

        protected override void ProcessErrorCodes()
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

            TTAGProcServerProcessResult returnResult = tagProcessor.ProjectDataServerTAGProcessorClient().SubmitTAGFileToTAGFileProcessor
                                (request.fileName,
                                new MemoryStream(request.data),
                                request.projectId ?? -1, 0, 0, request.machineId ?? -1,
                                RaptorConverters.convertWGS84Fence(request.boundary));

              if (returnResult == TTAGProcServerProcessResult.tpsprOK)
                  return TAGFilePostResult.CreateTAGFilePostResult();
              else
                  throw new ServiceException(HttpStatusCode.BadRequest,
                          new ContractExecutionResult(ContractExecutionStates.GetErrorNumberwithOffset((int)returnResult),
                                  String.Format("Failed to process tagfile with error: {0}",
                                          ContractExecutionStates.FirstNameWithOffset((int) returnResult))));
          }
          finally
          {
              ContractExecutionStates.ClearDynamic();
          }          
      }

    }
}
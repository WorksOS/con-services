using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.ConnectedSite.Gateway.WebApi;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Models;
using VSS.TRex.ConnectedSite.Gateway.WebApi.ResultHandling;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;

namespace VSS.TRex.ConnectedSite.Gateway.Executors
{
  /// <summary>
  /// Executor for Conntect Site Message submission.
  /// - Preforms a PreScan of supplied tag file
  /// - If there is enough information submits the details to the Cnnected Site API
  /// </summary>
  public class ConnectedSiteMessageSubmissionExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    public ConnectedSiteMessageSubmissionExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ConnectedSiteMessageSubmissionExecutor()
    {
    }

    /// <summary>
    /// Process tagfile request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as CompactionTagFileRequest;

      ContractExecutionResult result = new ContractExecutionResult((int)TRexTagFileResultCode.TRexUnknownException, $"TRex unknown result TagFilePreScanExecutor");
      try
      {
        var tagDetails = new TAGFilePreScan();
        log.LogInformation($"Starting TAGFilePreScan for file {request.FileName}");
        using (MemoryStream tagData = new MemoryStream(request.Data))
        {
          tagDetails.Execute(tagData);
         }

        if (tagDetails != null)
        {
          var client = DIContext.Obtain<ConnectedSiteClient>();
          var rest2 = client.PostMessage(new L1ConnectedSiteMessage(tagDetails)).Result;
          
          if (rest2.IsSuccessStatusCode)
          {
            result = ConnectedSiteMessageResult.Create(0, rest2.Content.ReadAsStringAsync().Result);
          }
        }


        //log.LogInformation($"#In# TagFileExecutor. Process tagfile:{request.FileName}, Project:{request.ProjectUid}, TCCOrgID:{request.OrgId}");

        //SubmitTAGFileRequest submitRequest = new SubmitTAGFileRequest();
        //SubmitTAGFileRequestArgument arg = null;

        //arg = new SubmitTAGFileRequestArgument()
        //{
        //  ProjectID = request.ProjectUid,
        //  AssetID = null, // not available via TagFileController APIs
        //  TAGFileName = request.FileName,
        //  TagFileContent = request.Data,
        //  TCCOrgID = request.OrgId
        //};

        //var res = submitRequest.Execute(arg);

        //if (res.Success)
        //  result = TagFileResult.Create(0, ContractExecutionResult.DefaultMessage);
        //else
        //  result = TagFileResult.Create(res.Code, res.Message);

      }
      finally
      {
        if (request != null)
          log.LogInformation($"#Out# TagFileExecutor. Process tagfile:{request.FileName}, Submission Code: {result.Code}, Message:{result.Message}");
        else
          log.LogInformation($"#Out# TagFileExecutor. Invalid request");

      }
      return result;

    }


    /// <summary>
    /// Processes the tagfile request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}

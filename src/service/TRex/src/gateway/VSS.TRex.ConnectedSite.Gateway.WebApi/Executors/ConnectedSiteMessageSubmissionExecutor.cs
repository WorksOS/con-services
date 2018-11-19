using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.ConnectedSite.Gateway.WebApi;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Models;
using VSS.TRex.ConnectedSite.Gateway.WebApi.ResultHandling;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.TAGFiles.Executors;

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
      throw new NotImplementedException("Use the ProcessAsyncEx method");

    }

    /// <summary>
    /// Processes the tagfile request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as ConnectedSiteRequest;

      ContractExecutionResult result = ConnectedSiteMessageResult.Create((int)TRexTagFileResultCode.TRexUnknownException, $"TRex unknown result TagFilePreScanExecutor");
      try
      {
        var tagDetails = new TAGFilePreScan();
        log.LogInformation($"Starting TAGFilePreScan for file {request.TagRequest.FileName}");
        using (MemoryStream tagData = new MemoryStream(request.TagRequest.Data))
        {
          if (tagDetails.Execute(tagData))
          {
            var client = DIContext.Obtain<IConnectedSiteClient>();
            if (client == null)
            {
              throw new ConnectedSiteClientException("Could not obtain Connected Site Client, have you added it to DI?");
            }
            IConnectedSiteMessage message = null;
            switch (request.MessageType)
            {
              case ConnectedSiteMessageType.L1PositionMessage:
                message = new L1ConnectedSiteMessage(tagDetails);
                break;
              case ConnectedSiteMessageType.L2StatusMessage:
                message = new L2ConnectedSiteMessage(tagDetails);
                break;
              default:
                throw new NotImplementedException("Unknown ConnectedSite Message Type");
            }
            var response = await client.PostMessage(message);

            if (response.IsSuccessStatusCode)
            {
              result = ConnectedSiteMessageResult.Create(0, await response.Content.ReadAsStringAsync());
            } else
            {
              result = ConnectedSiteMessageResult.Create((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
          }
        }
      }
      finally
      {
        if (request != null)
          log.LogInformation($"#Out# ConnectedSiteMessageSubmissionExecutor. Process tagfile:{request.TagRequest.FileName}, Submission Code: {result.Code}, Message:{result.Message}");
        else
          log.LogInformation($"#Out# ConnectedSiteMessageSubmissionExecutor. Invalid request");

      }
      return result;
    }
  }
}

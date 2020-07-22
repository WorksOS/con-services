using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.TagFileGateway.Common.Executors;
using VSS.Productivity3D.TagFileGateway.Common.Models.Sns;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileGateway.Common.Services
{
  public class TagFileSqsService : BaseHostedService
  {
    private const string CONFIG_KEY = "AWS_SQS_TAG_FILE_URL";
    private const string CONCURRENT_KEY = "AWS_SQS_TAG_FILE_CONCURRENT_COUNT";

    private readonly string _url;

    private readonly AmazonSQSClient _awSqsClient;
    private readonly int _concurrentCount;

    public TagFileSqsService(ILoggerFactory logger, IServiceScopeFactory serviceScope, IConfigurationStore configurationStore) : base(logger, serviceScope)
    {
      _url = configurationStore.GetValueString(CONFIG_KEY);
      if(string.IsNullOrEmpty(_url))
        throw new ArgumentException($"No URL Provided for SQS. Configuration Key: {CONFIG_KEY}");
      else
        Logger.LogInformation($"{nameof(TagFileSqsService)} Tag File SQS URL: {_url}");


      var awsProfile = configurationStore.GetValueString("AWS_PROFILE", null);
      if (string.IsNullOrEmpty(awsProfile))
      {
        Logger.LogInformation($"{nameof(TagFileSqsService)} AWS Using Assumed Roles for SQS");
        _awSqsClient = new AmazonSQSClient(RegionEndpoint.USWest2);
      }
      else
      {
        Logger.LogInformation($"{nameof(TagFileSqsService)} Using AWS Profile: {awsProfile}");
        _awSqsClient = new AmazonSQSClient(new StoredProfileAWSCredentials(awsProfile), RegionEndpoint.USWest2);
      }

      _concurrentCount = configurationStore.GetValueInt(CONCURRENT_KEY, 10);
      Logger.LogInformation($"{nameof(TagFileSqsService)} Processing {_concurrentCount} Concurrent SQS Tag File Entries");
    }

    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(_url))
      {
        Logger.LogError($"{nameof(ExecuteAsync)} No SQS Url Provided - exiting");
        return;
      }

      while (!cancellationToken.IsCancellationRequested)
      {
        try
        {
          var count = await ProcessMessages();
          // Wait if no messages, so we don't hammer SQS
          if (count == 0)
            await Task.Delay(1000, cancellationToken);
        }
        catch (Exception e)
        {
          // Could be AWS outage, or our service outage
          Logger.LogError(e, $"{nameof(ExecuteAsync)} Failed to process messages");
          await Task.Delay(60 * 1000, cancellationToken);
        }
      }
    }

    private async Task<int> ProcessMessages()
    {
      var receiveMessageRequest = new ReceiveMessageRequest
      {
        QueueUrl = _url,
        MaxNumberOfMessages = _concurrentCount,
      };

      var receiveMessageResponse = await _awSqsClient.ReceiveMessageAsync(receiveMessageRequest);
      var tasks = new List<Task>(_concurrentCount);
      foreach (var m in receiveMessageResponse.Messages)
      {
        var task = ProcessSingleMessage(m);
        tasks.Add(task);
      }

      await Task.WhenAll(tasks);
      return tasks.Count;
    }

    private async Task ProcessSingleMessage(Message m)
    {
      try
      {
        var snsPayload = JsonConvert.DeserializeObject<SnsPayload>(m.Body);
        if (snsPayload == null)
        {
          // Can occur with badly formed Json.
          // Delete, so they don't just fill up the que
          var deleteMessage = new DeleteMessageRequest(_url, m.ReceiptHandle);
          var deleteResponse = await _awSqsClient.DeleteMessageAsync(deleteMessage);
          Logger.LogWarning($"{nameof(ProcessSingleMessage)} Failed to parse SQS Message. MessageID: {m.MessageId}, Body: {m.Body} Delete SQS Message Response Code: {deleteResponse.HttpStatusCode}");
          return;
        }

        Logger.LogInformation($"{nameof(ProcessSingleMessage)} Processing SQS Message ID: {m.MessageId}.");
        // We need to create a scope, as a hosted service is a singleton, but some of the services are transient, we can't inject them.
        // Instead we create a scope for 'our' work
        using var serviceScope = ScopeFactory.CreateScope();
        var executor = RequestExecutorContainer.Build<TagFileSnsProcessExecutor>(
          serviceScope.ServiceProvider.GetService<ILoggerFactory>(),
          serviceScope.ServiceProvider.GetService<IConfigurationStore>(),
          serviceScope.ServiceProvider.GetService<IDataCache>(),
          serviceScope.ServiceProvider.GetService<ITRexTagFileProxy>(),
          serviceScope.ServiceProvider.GetService<ITransferProxyFactory>(),
          serviceScope.ServiceProvider.GetService<IWebRequest>());

        var result = await executor.ProcessAsync(snsPayload);

        // internalErrors are retry-able, so leave them on the que to be picked up again.
        if (result.Code != ContractExecutionStatesEnum.InternalProcessingError)
        {
          // Mark as processed
          var deleteMessage = new DeleteMessageRequest(_url, m.ReceiptHandle);
          var deleteResponse = await _awSqsClient.DeleteMessageAsync(deleteMessage);
          Logger.LogInformation($"{nameof(ProcessSingleMessage)} Delete SQS Message Response Code: {deleteResponse.HttpStatusCode}");
        }
        else
        {
          Logger.LogWarning($"{nameof(ProcessSingleMessage)} No response for Message ID: {m.MessageId}.");
        }
      }
      catch (Exception e)
      {
        Logger.LogError(e, $"{nameof(ProcessSingleMessage)} Failed to process message with ID {m.MessageId} - not deleted from the queue");
      }
    }
  }
}

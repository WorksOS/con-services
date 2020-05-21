using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.TagFileGateway.Common.Abstractions;
using VSS.Productivity3D.TagFileGateway.Common.Executors;
using VSS.Productivity3D.TagFileGateway.Common.Models.Sns;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileGateway.Common.Services
{
  public class TagFileSqsService : BaseHostedService
  {
    private const string CONFIG_KEY = "AWS_SQS_TAG_FILE_URL";

    private readonly string _url;

    private readonly AmazonSQSClient _awSqsClient;

    public TagFileSqsService(ILoggerFactory logger, IServiceScopeFactory serviceScope, IConfigurationStore configurationStore) : base(logger, serviceScope)
    {
      _url = configurationStore.GetValueString(CONFIG_KEY);
      if(string.IsNullOrEmpty(_url))
        throw new ArgumentException($"No URL Provided for SQS. Configuration Key: {CONFIG_KEY}");
      else
        Logger.LogInformation($"Tag File SQS URL: {_url}");


      var awsProfile = configurationStore.GetValueString("AWS_PROFILE", null);
      if (string.IsNullOrEmpty(awsProfile))
      {
        Logger.LogInformation("AWS Using Assumed Roles for SQS");
        _awSqsClient = new AmazonSQSClient(RegionEndpoint.USWest2);
      }
      else
      {
        Logger.LogInformation($"Using AWS Profile: {awsProfile}");
        _awSqsClient = new AmazonSQSClient(new StoredProfileAWSCredentials(awsProfile), RegionEndpoint.USWest2);
      }
    }

    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(_url))
      {
        Logger.LogError("No SQS Url Provided - exiting");
        return;
      }

      // We need to create a scope, as a hosted service is a singleton, but some of the services are transient, we can't inject them.
      // Instead we create a scope for 'our' work
      using var serviceScope = ScopeFactory.CreateScope();
      var executor = RequestExecutorContainer.Build<TagFileSnsProcessExecutor>(
        serviceScope.ServiceProvider.GetService<ILoggerFactory>(),
        serviceScope.ServiceProvider.GetService<IConfigurationStore>(),
        serviceScope.ServiceProvider.GetService<IDataCache>(),
        serviceScope.ServiceProvider.GetService<ITagFileForwarder>(),
        serviceScope.ServiceProvider.GetService<ITransferProxy>(),
        serviceScope.ServiceProvider.GetService<IWebRequest>());

      while (!cancellationToken.IsCancellationRequested)
      {
        try
        {
          await ProcessMessages(executor, cancellationToken);
        }
        catch (Exception e)
        {
          // Could be AWS outage, or our service outage
          Logger.LogError(e, "Failed to process messages");
          await Task.Delay(60 * 1000, cancellationToken);
        }
      }
    }

    private async Task ProcessMessages(TagFileSnsProcessExecutor executor, CancellationToken cancellationToken)
    {
      var receiveMessageRequest = new ReceiveMessageRequest
      {
        QueueUrl = _url,
        MaxNumberOfMessages = 10
      };

      var receiveMessageResponse = await _awSqsClient.ReceiveMessageAsync(receiveMessageRequest);
      foreach (var m in receiveMessageResponse.Messages)
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        var snsPayload = JsonConvert.DeserializeObject<SnsPayload>(m.Body);
        if (snsPayload == null)
        {
          // I don't think we will get these kinds of messages - not sure exactly how to solve just yet.
          // If we delete them, we may miss tag files
          // But if we don't delete, we may fill up the queue with bad messages
          // Will monitor during testing
          Logger.LogWarning($"Failed to parse SQS Message. MessageID: {m.MessageId}, Body: {m.Body}");
          continue;
        }

        Logger.LogInformation($"Processing SQS Message ID: {m.MessageId}.");

        var result = await executor.ProcessAsync(snsPayload);
        if (result != null)
        {
          // Mark as processed
          var deleteMessage = new DeleteMessageRequest(_url, m.ReceiptHandle);
          var deleteResponse = await _awSqsClient.DeleteMessageAsync(deleteMessage);
          Logger.LogInformation($"Delete SQS Message Response Code: {deleteResponse.HttpStatusCode}");
        }
        else
        {
          Logger.LogWarning($"No response for Message ID: {m.MessageId}.");
        }
      }
    }
  }
}

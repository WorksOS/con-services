using System.Net;
using Slack.Webhooks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  /// <summary>
  /// Notify DevOps using Slack
  /// </summary>
  public class SlackNotification : IDevOpsNotification
  {
    private readonly string url;
    private readonly string channel;
    private readonly string username;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    public SlackNotification(IConfigurationStore configStore)
    {
      //see https://productivity3d.slack.com/apps/new/A0F7XDUAZ-incoming-webhooks for configuration values
      url = configStore.GetValueString("SLACK_URL");
      channel = configStore.GetValueString("SLACK_CHANNEL");
      username = configStore.GetValueString("SLACK_USERNAME");
      if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(username))
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "Missing slack client configuration"));
      }
    }

    /// <summary>
    /// Notify DevOps with the given message via Slack
    /// </summary>
    public bool Notify(string message)
    {
      var slackClient = new SlackClient(url);
      
      var slackMessage = new SlackMessage
      {
        Channel = channel,
        Text = message,
        Username = username
      };
      return slackClient.Post(slackMessage);
    }
  }
}

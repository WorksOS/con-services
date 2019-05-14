using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Slack.Webhooks;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  /// <summary>
  /// Notify DevOps using Slack
  /// </summary>
  public class SlackNotification : IDevOpsNotification
  {
    private readonly string url = String.Empty;
    private readonly string channel = String.Empty;
    private readonly string username = String.Empty;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    public SlackNotification(IConfigurationStore configStore, ILogger<SlackNotification> logger)
    {
      //see https://productivity3d.slack.com/apps/new/A0F7XDUAZ-incoming-webhooks for configuration values
      url = configStore.GetValueString("SLACK_URL");
      channel = configStore.GetValueString("SLACK_CHANNEL");
      username = configStore.GetValueString("SLACK_USERNAME");
      if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(username))
      {
        logger.LogWarning("Missing environemtn variables for Slack - Slacking is switched off");
      }
    }

    /// <summary>
    /// Notify DevOps with the given message via Slack
    /// </summary>
    public bool Notify(string message)
    {
      if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(username))
        return false;
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleNotificationService.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Models;

namespace VSS.Productivity3D.Push.Controllers
{
  public class NotificationController : Controller
  {
    private readonly ILogger<NotificationController> _logger;
    private readonly INotificationHubClient _notificationHubClient;

    public NotificationController(ILogger<NotificationController> logger, INotificationHubClient notificationHubClient)
    {
      _logger = logger;
      _notificationHubClient = notificationHubClient;
    }

    [HttpPost("api/v1/notification/sns")]
    public async Task<IActionResult> SnsNotification()
    {
      // https://forums.aws.amazon.com/thread.jspa?threadID=69413
      // AWS SNS is in text/plain, not application/json - so need to parse manually
      var payloadMs = new MemoryStream();
      await Request.Body.CopyToAsync(payloadMs);
      var payload = Message.ParseMessage(Encoding.UTF8.GetString(payloadMs.ToArray()));

      bool isValid;

      try
      {
        isValid = payload.IsMessageSignatureValid();
      }
      catch (AmazonClientException e)
      {
        _logger.LogWarning($"Failed to validate SNS Message. Error: {e.Message}");
        return BadRequest(e.Message);
      }

      if (!isValid)
        return BadRequest();

      _logger.LogInformation($"Received SNS Message: {payload.MessageId}. Topic: {payload.TopicArn} Type: {payload.Type} Valid: {payload.IsMessageSignatureValid()}");
      if (payload.IsSubscriptionType)
      {
        _logger.LogInformation($"SNS SUBSCRIPTION REQUEST: {payload.MessageText}, Subscription URL: '{payload.SubscribeURL}'");
      }
      else if(payload.IsNotificationType)
      {
        // Got a valid message
        var notification = JsonConvert.DeserializeObject<CwsTrnUpdate>(payload.MessageText);
        if (notification != null)
        {
          // Iterate all 
          var trns = notification.UpdatedTrns ?? new List<string>();
          trns.Add(notification.AccountTrn);
          trns.Add(notification.ProjectTrn);
          var tasks = new Task[trns.Count];
          for (var i = 0; i < trns.Count; i++)
          {
            var guid = TRNHelper.ExtractGuid(trns[i]);
            if (guid.HasValue)
              tasks[i] = _notificationHubClient.Notify(new ProjectChangedNotification(guid.Value));
            else
              _logger.LogWarning($"Failed to extra GUID from TRN: {trns[i]}");
          }

          await Task.WhenAll(tasks);
          _logger.LogInformation($"Processed notifications. Total TRNS: {trns.Count}");
        }
        else
        {
          _logger.LogWarning($"Failed to parse notification message with content: {payload.MessageText}");
        }
      }

      return Ok();
    }
  }
}


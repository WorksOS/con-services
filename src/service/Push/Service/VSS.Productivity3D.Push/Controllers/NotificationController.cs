using System;
using Amazon.Runtime;
using Amazon.SimpleNotificationService.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Productivity3D.Push.Abstractions.Notifications;

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
    public IActionResult SnsNotification([FromBody]Message payload)
    {
      bool isValid;

      try
      {
        isValid = payload.IsMessageSignatureValid();
      }
      catch (AmazonClientException e)
      {
        _logger.LogWarning($"Falied to validate SNS Message. Error: {e.Message}");
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
        if (payload.IsMessageSignatureValid())
        {
          _logger.LogWarning($"Bad Request. Message Signature Failed: {JsonConvert.SerializeObject(payload)}");
          return BadRequest();
        }

        // Got a valid message
        // TODO Action these once CWS define the payload
        _logger.LogInformation($"Received notification, no actions executed currently. Message ID: {payload.MessageId}. Text: {payload.MessageText}");
      }

      return Ok();
    }
  }
}


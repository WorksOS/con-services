using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.TagFileGateway.Common.Models.Sns
{
  public class SnsPayload
  {
    public const string SubscriptionType = "SubscriptionConfirmation";

    public const string NotificationType = "Notification";

    [JsonIgnore] public bool IsNotification => string.Compare(Type, NotificationType, StringComparison.InvariantCulture) == 0;

    public string Type { get; set; }

    public string MessageId { get; set; }

    public string Token { get; set; }

    public string TopicArn { get; set; }

    public string Message { get; set; }

    public string SubscribeURL { get; set; }

    public DateTime Timestamp { get; set; }

    public string SignatureVersion { get; set; }

    public string Signature { get; set; }

    public string SingingCertURL { get; set; }
  }
}

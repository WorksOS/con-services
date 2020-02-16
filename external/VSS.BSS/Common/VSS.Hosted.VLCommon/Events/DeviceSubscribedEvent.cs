using System;
using MassTransit;

namespace VSS.Hosted.VLCommon.Events
{
  public class DeviceSubscribedEvent : CorrelatedBy<Guid>
  {
    public long AssetId { get; set; }
    public long OwnerId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public Guid CorrelationId { get; private set; }
    public string _id { get; set; }
    public int Source { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? ConsumedUtc { get; set; }
    public int RetryAttempt { get; set; }

    public DeviceSubscribedEvent()
    {
      CorrelationId = Guid.NewGuid();
    }
  }
}

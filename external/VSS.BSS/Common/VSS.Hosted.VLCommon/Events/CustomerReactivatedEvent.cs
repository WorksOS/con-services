using System;
using MassTransit;

namespace VSS.Hosted.VLCommon.Events
{
  public class CustomerReactivatedEvent : CorrelatedBy<Guid>
  {
    public long CustomerId { get; set; }

    public Guid CorrelationId { get; private set; }
    public string _id { get; set; }
    public int Source { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? ConsumedUtc { get; set; }
    public int RetryAttempt { get; set; }

    public CustomerReactivatedEvent()
    {
      CorrelationId = Guid.NewGuid();
    }
  }
}
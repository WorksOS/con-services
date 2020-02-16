using System;
using MassTransit;

namespace VSS.Hosted.VLCommon.Events
{
  public class DeviceOwnershipTransferredEvent : CorrelatedBy<Guid>
  {
    public long NewCustomerId { get; set; }
    public long CustomerId { get; set; }
    public long AssetId { get; set; }
    public long ParentCustomerId { get; set; }
    public string ParentCustomerName { get; set; }
    public string SerialNumber { get; set; }

    public Guid CorrelationId { get; private set; }
    public string _id { get; set; }
    public int Source { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? ConsumedUtc { get; set; }
    public int RetryAttempt { get; set; }

    public DeviceOwnershipTransferredEvent()
    {
      CorrelationId = Guid.NewGuid();
    }
  }
}

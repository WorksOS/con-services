using System;
using MassTransit;

namespace VSS.Hosted.VLCommon.Events
{
  public class AssetUpdatedEvent : CorrelatedBy<Guid>
  {
    public string SerialNumber { get; set; }
    public string ProductFamilyName { get; set; }
    public string MakeCode { get; set; }
    public string Model { get; set; }
    public int? ManufactureYear { get; set; }
    public long AssetId { get; set; }
    public long OwnerId { get; set; }
    public string Name { get; set; }
    public long ParentCustomerId { get; set; }
    public string ParentCustomerName { get; set; }

    public Guid CorrelationId { get; private set; }
    public string _id { get; set; }
    public int Source { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? ConsumedUtc { get; set; }
    public int RetryAttempt { get; set; }

    public AssetUpdatedEvent()
    {
      CorrelationId = Guid.NewGuid();
    }
  }
}

using System;
using MassTransit;

namespace VSS.Hosted.VLCommon.Events
{
  public class LocationReceivedEvent : CorrelatedBy<Guid>
  {
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime? EventUtc { get; set; }
    public DateTime? InsertUtc { get; set; }
    public long AssetId { get; set; }

    public Guid CorrelationId { get; private set; }
    public string _id { get; set; }
    public int Source { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? ConsumedUtc { get; set; }
    public int RetryAttempt { get; set; }

    public LocationReceivedEvent()
    {
      CorrelationId = Guid.NewGuid();
    }
  }
}

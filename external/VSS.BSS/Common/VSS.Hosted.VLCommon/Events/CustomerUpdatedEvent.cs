using System;
using MassTransit;

namespace VSS.Hosted.VLCommon.Events
{
  public class CustomerUpdatedEvent : CorrelatedBy<Guid>
  {
    public string Name { get; set; }
    public string CustomerType { get; set; }
    public long CustomerId { get; set; }
    public string CustomerGuid { get; set; }
    public string BssId { get; set; }

    // ParentDealer
    public long ParentDealerId { get; set; }
    public string ParentDealerName { get; set; }
    public string ParentDealerGuid { get; set; }

    // ParentCustomer
    public long ParentCustomerId { get; set; }
    public string ParentCustomerName { get; set; }
    public string ParentCustomerGuid { get; set; }

    public Guid CorrelationId { get; private set; }
    public string _id { get; set; }
    public int Source { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public DateTime? ConsumedUtc { get; set; }
    public int RetryAttempt { get; set; }

    public CustomerUpdatedEvent()
    {
      CorrelationId = Guid.NewGuid();
    }
  }
}

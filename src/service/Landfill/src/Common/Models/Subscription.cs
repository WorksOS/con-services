using System;

namespace Common.Models
{
  public class Subscription
  {
    public string SubscriptionUID { get; set; }
    public string CustomerUID { get; set; }
    public int ServiceTypeID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }

  public class ServiceType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public int ServiceTypeFamilyID { get; set; }
    public string ServiceTypeFamilyName { get; set; }
  }
}

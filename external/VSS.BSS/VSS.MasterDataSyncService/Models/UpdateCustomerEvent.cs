using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class UpdateCustomerEvent : ICustomerEvent
  {
    public string CustomerName { get; set; }
    public string BSSID { get; set; }
    public string DealerNetwork { get; set; }
    public string NetworkDealerCode { get; set; }
    public string NetworkCustomerCode { get; set; }
    public string DealerAccountCode { get; set; }
    public Guid CustomerUID { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
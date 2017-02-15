using System;

namespace VSS.TagFileAuthentication.Data.Models
{
  public class CustomerProject
  {
    public string ProjectUID { get; set; }
    public string CustomerUID { get; set; }
    
    // this belongs in Customer table, however for expediancy it arrives in the CustomerProject kafka Event.
    public long LegacyCustomerID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}

using System;

namespace VSS.Project.Data.Models
{
  public class CustomerProject
  {
    public string ProjectUID { get; set; }
    public string CustomerUID { get; set; }
    public long LegacyCustomerID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}

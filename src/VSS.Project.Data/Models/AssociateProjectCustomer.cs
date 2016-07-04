using System;
using VSS.Project.Data.Interfaces;

namespace VSS.Project.Data.Models
{
  public class AssociateProjectCustomer : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public Guid CustomerUID { get; set; }
    public long LegacyCustomerID { get; set; }
    public RelationType RelationType { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}

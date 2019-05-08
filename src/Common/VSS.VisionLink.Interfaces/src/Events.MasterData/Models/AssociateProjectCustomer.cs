using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
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
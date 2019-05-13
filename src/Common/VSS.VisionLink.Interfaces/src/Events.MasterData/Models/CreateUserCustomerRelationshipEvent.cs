using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class CreateUserCustomerRelationshipEvent : ICustomerUserEvent
  {
    public Guid CustomerUID { get; set; }
    public Guid UserUID { get; set; }
    public string JobTitle { get; set; }
    public string JobType { get; set; }
    public Guid? CreatedByUserUID { get; set; }
    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }
  }
}

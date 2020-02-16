using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDAPIs.ProjectMasterData
{
  public class AssociateProjectCustomer : IProjectEvent
  {
    public Guid ProjectUID { get; set; }
    public Guid CustomerUID { get; set; }
    public long LegacyCustomerID { get; set; }
    public int RelationType { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }

  public enum RelationType
  {
    Owner = 0,
    Customer = 1,
    Dealer = 2,
    Operations = 3,
    Corporate = 4,
    SharedOwner = 5
  }

}

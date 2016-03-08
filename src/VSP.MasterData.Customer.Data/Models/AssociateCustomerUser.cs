using System;

namespace VSP.MasterData.Customer.Data.Models
{
  public class AssociateCustomerUser
  {
    public Guid CustomerUID { get; set; }

    public Guid UserUID { get; set; }

    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }
  }
}

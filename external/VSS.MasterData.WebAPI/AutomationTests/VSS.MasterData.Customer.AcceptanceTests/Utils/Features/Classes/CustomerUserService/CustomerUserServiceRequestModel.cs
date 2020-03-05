using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerUserService
{
  #region AssociateCustomerUser
  public class AssociateCustomerUserModel
  {
    public AssociateCustomerUserEvent AssociateCustomerUserEvent;
  }
  public class AssociateCustomerUserEvent 
  {

    public Guid CustomerUID { get; set; }

    public Guid UserUID { get; set; }

    public DateTime ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ReceivedUTC { get; set; }
  }
#endregion

  #region DissociateCustomerUser
  public class DissociateCustomerUserModel
  {
    public DissociateCustomerUserEvent DissociateCustomerUserEvent;
  }
  public class DissociateCustomerUserEvent 
  {

    public Guid CustomerUID { get; set; }

    public Guid UserUID { get; set; }

    public DateTime ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ReceivedUTC { get; set; }
  }
  #endregion

  #region InvalidAssociateCustomerUser

  public class InvalidAssociateCustomerUserEvent
  {

    public string CustomerUID { get; set; }

    public string UserUID { get; set; }

    public string ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ReceivedUTC { get; set; }
  }
  #endregion

  #region InvalidDissociateCustomerUser
  public class InvalidDissociateCustomerUserEvent
  {

    public string CustomerUID { get; set; }

    public string UserUID { get; set; }

    public string ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ReceivedUTC { get; set; }
  }
  #endregion

}

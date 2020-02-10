using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerAssetService
{
  #region AssociateCustomerAsset
  public class AssociateCustomerAssetModel
  {
    public AssociateCustomerAssetEvent AssociateCustomerAssetEvent;
  }

  public class AssociateCustomerAssetEvent
  {

    public Guid CustomerUID { get; set; }


    public Guid AssetUID { get; set; }


    public string RelationType { get; set; }


    public DateTime ActionUTC { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ReceivedUTC { get; set; }
  }
  #endregion

  #region DissociateCustomerAsset
  public class DissociateCustomerAssetModel
  {
    public DissociateCustomerAssetEvent DissociateCustomerAssetEvent;
  }

  public class DissociateCustomerAssetEvent
  {

    public Guid CustomerUID { get; set; }


    public Guid AssetUID { get; set; }


    public DateTime ActionUTC { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ReceivedUTC { get; set; }
  }
  #endregion

  #region InvalidAssociateCustomerAssetEvent
  public class InvalidAssociateCustomerAssetEvent
  {

    public string CustomerUID { get; set; }


    public string AssetUID { get; set; }

    public string RelationType { get; set; }


    public string ActionUTC { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ReceivedUTC { get; set; }
  }
  #endregion

  #region InvalidDissociateCustomerAsset


  public class InvalidDissociateCustomerAssetEvent
  {

    public string CustomerUID { get; set; }


    public string AssetUID { get; set; }


    public string ActionUTC { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ReceivedUTC { get; set; }
  }
  #endregion

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

using System;
using System.Runtime.Serialization;
using VSS.Hosted.VLCommon.ServiceContracts;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Bss.Schema.V2
{
  [DataContract(Namespace = ContractConstants.NHBssSvc + "/v2/AssetIDChanges", Name = "AssetIDChanges")]
  public class AssetIDChanges : BssCommon
  {
    [DataMember(Order = 0, Name = "Assets", IsRequired = true)]
    public List<AssetInfo> AssetInfo { get; set; }
  }

  [DataContract(Namespace = ContractConstants.NHBssSvc + "/v2/AssetIDChanges", Name = "AssetInfo")]
  public class AssetInfo 
  {
    [DataMember(Order = 0, Name = "SerialNumber", IsRequired = true)]
    public string SerialNumber { get; set; }
    [DataMember(Order = 1, Name = "MakeCode", IsRequired = true)]
    public string MakeCode { get; set; }
    [DataMember(Order = 2, Name = "AssetName", IsRequired = true)]
    public string AssetName { get; set; }
    [DataMember(Order = 3, Name = "AssetNameUpdatedUTC", IsRequired = true)]
    public DateTime AssetNameUpdatedUTC { get; set; }
    [DataMember(Order = 4, Name = "OwnerBssID", IsRequired = true)]
    public string OwnerBssID { get; set; }
    [DataMember(Order = 5, Name = "IBKey", IsRequired = true)]
    public string IBKey { get; set; }
    [DataMember(Order = 6, Name = "DealerCode", IsRequired = false)]
    public string DealerCode { get; set; }
    [DataMember(Order = 7, Name = "CustomerCode", IsRequired = false)]
    public string CustomerCode { get; set; }
    [DataMember(Order = 8, Name = "DealerAccountCode", IsRequired = false)]
    public string DealerAccountCode { get; set; }
  }
}

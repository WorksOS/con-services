using System;
using System.Runtime.Serialization;
using VSS.Hosted.VLCommon.ServiceContracts;

namespace VSS.Hosted.VLCommon.Bss.Schema.V2
{
  [DataContract(Namespace = ContractConstants.NHBssSvc + "/v2/AccountHierarchy", Name = "AccountHierarchy")]
  public class AccountHierarchy : BssCommon
  {
    [DataMember(Order = 0, Name = "TargetStack", IsRequired = true)]
    public string TargetStack { get; set; }
    [DataMember(Order = 1, Name = "SequenceNumber", IsRequired = true)]
    public long SequenceNumber { get; set; }
    [DataMember(Order = 2, Name = "ControlNumber", IsRequired = true)]
    public string ControlNumber { get; set; }
    [DataMember(Order = 3, Name = "Action", IsRequired = true)]
    public string Action { get; set; }
    [DataMember(Order = 4, Name = "ActionUTC", IsRequired = true)]
    public string ActionUTC { get; set; }

    [DataMember(Order = 5, Name = "PrimaryContact", IsRequired = false)]
    public PrimaryContact contact { get; set; }

    [DataMember(Order = 6, Name = "CustomerName", IsRequired = true)]
    public string CustomerName { get; set; }
    [DataMember(Order = 7, Name = "BSSID", IsRequired = true)]
    public string BSSID{ get; set; }
    [DataMember(Order = 8, Name = "CustomerType", IsRequired = true)]
    public string CustomerType{ get; set; }
    [DataMember(Order = 9, Name = "DealerNetwork", IsRequired = false)]
    public string DealerNetwork{ get; set; }
    [DataMember(Order = 10, Name = "NetworkDealerCode", IsRequired = false)]
    public string NetworkDealerCode { get; set; }
    [DataMember(Order = 11, Name = "NetworkCustomerCode", IsRequired = false)]
    public string NetworkCustomerCode { get; set; }
    [DataMember(Order = 12, Name = "DealerAccountCode", IsRequired = false)]
    public string DealerAccountCode { get; set; }
    [DataMember(Order = 13, Name = "ParentBSSID", IsRequired = false)]
    public string ParentBSSID { get; set; }
    [DataMember(Order = 14, Name = "RelationshipID", IsRequired = false)]
    public string RelationshipID { get; set; }
    [DataMember(Order = 15, Name = "HierarchyType", IsRequired = true)]
    public string HierarchyType { get; set; }

    public enum BSSCustomerTypeEnum
    {
      ACCOUNT = 0,
      CUSTOMER = 1,
      DEALER = 2
    }
  }

  [DataContract(Namespace = ContractConstants.NHBssSvc + "/v2/AccountHierarchy", Name = "PrimaryContact")]
  public class PrimaryContact
  {
    [DataMember(Order = 0, Name = "FirstName", IsRequired = false)]
    public string FirstName { get; set; }
    [DataMember(Order = 1, Name = "LastName", IsRequired = false)]
    public string LastName { get; set; }
    [DataMember(Order = 2, Name = "Email", IsRequired = false)]
    public string Email { get; set; }
  }
}

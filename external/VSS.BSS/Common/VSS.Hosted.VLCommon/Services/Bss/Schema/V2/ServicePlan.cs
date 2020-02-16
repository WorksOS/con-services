using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using VSS.Hosted.VLCommon.ServiceContracts;

namespace VSS.Hosted.VLCommon.Bss.Schema.V2
{
  [DataContract(Namespace = ContractConstants.NHBssSvc + "/v2/ServicePlan", Name = "ServicePlan")]
  public class ServicePlan : BssCommon
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

    [DataMember(Order = 5, Name = "ServicePlanName", IsRequired = true)]
    public string ServicePlanName { get; set; }
    [DataMember(Order = 6, Name = "ServiceTerminationDate", IsRequired = false)]
    public string ServiceTerminationDate { get; set; }
    [DataMember(Order = 7, Name = "ServicePlanlineID", IsRequired = true)]
    public string ServicePlanlineID { get; set; }
    [DataMember(Order = 8, Name = "IBKey", IsRequired = true)]
    public string IBKey { get; set; }
    [DataMember(Order = 9, Name = "OwnerVisibilityDate", IsRequired = false)]
    public string OwnerVisibilityDate { get; set; }
  }
}

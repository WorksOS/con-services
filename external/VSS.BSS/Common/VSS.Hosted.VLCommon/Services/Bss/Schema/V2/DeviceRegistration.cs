using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using VSS.Hosted.VLCommon.ServiceContracts;

namespace VSS.Hosted.VLCommon.Bss.Schema.V2
{
  [DataContract(Namespace = ContractConstants.NHBssSvc + "/v2/DeviceRegistration", Name = "DeviceRegistration")]
  public class DeviceRegistration : BssCommon
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

    [DataMember(Order = 5, Name = "IBKey", IsRequired = true)]
    public string IBKey { get; set; }
    [DataMember(Order = 6, Name = "Status", IsRequired = true)]
    public string Status { get; set; }
  }

  public enum DeviceRegistrationStatusEnum
  {
    DEREG_TECH = 0,
    DEREG_STORE = 1,
    REG = 2
  }
}

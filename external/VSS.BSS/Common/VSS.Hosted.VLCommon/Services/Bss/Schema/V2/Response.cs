using System.Runtime.Serialization;
using VSS.Hosted.VLCommon.ServiceContracts;
using System;

namespace VSS.Hosted.VLCommon.Bss.Schema.V2
{
  [DataContract(Namespace = ContractConstants.NHBssSvc + "/v2/Response", Name = "Response")]
  public class Response : BssCommon
  {
    [DataMember(Order = 0, Name = "TargetStack", IsRequired = true)]
    public string TargetStack { get; set; }
    [DataMember(Order = 1, Name = "SequenceNumber", IsRequired = true)]
    public long SequenceNumber { get; set; }
    [DataMember(Order = 2, Name = "ControlNumber", IsRequired = true)]
    public string ControlNumber { get; set; }

    [DataMember(Order = 3, Name = "EndPointName", IsRequired = true)]
    public EndpointEnum EndPointName { get; set; }
    [DataMember(Order = 4, Name = "Success", IsRequired = true)]
    public string Success { get; set; }
    [DataMember(Order = 4, Name = "ErrorCode", IsRequired = true)]
    public string ErrorCode { get; set; }
    [DataMember(Order = 4, Name = "ErrorDescription", IsRequired = true)]
    public string ErrorDescription { get; set; }
    [DataMember(Order = 4, Name = "ProcessedUTC", IsRequired = true)]
    public string ProcessedUTC { get; set; }

    public enum EndpointEnum
    {
      AccountHierarchy = 1,
      InstallBase = 2,
      ServicePlan = 3,
      DeviceReplacement = 4,
      DeviceRegistration = 5,
    }
  }
}

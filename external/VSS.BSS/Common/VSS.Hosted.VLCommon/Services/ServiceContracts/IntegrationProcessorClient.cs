using System;
using System.Runtime.Serialization;
using VSS.Hosted.VLCommon.ServiceContracts;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
    [DataContract(Namespace = ContractConstants.IntegrationNS)]
  public class ByteArrayMessageWrapper
  {
    [DataMember]
    public string DeviceSerialNumber { get; set; }

    [DataMember]
    public DeviceTypeEnum DeviceType { get; set; }

    [DataMember]
    public DeviceICDSeriesEnum ICD { get; set; }

    [DataMember]
    public byte[] Message { get; set; }

    [DataMember]
    public DateTime ReceivedUTC { get; set; }

    [DataMember]
    public int StoreAttempts { get; set; }

    [DataMember]
    public int? PacketID { get; set; }

    [DataMember]
    public long? TypeID { get; set; }
  }
}

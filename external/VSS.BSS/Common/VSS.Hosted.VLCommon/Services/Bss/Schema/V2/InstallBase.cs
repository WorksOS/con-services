using System;
using System.Runtime.Serialization;
using VSS.Hosted.VLCommon.ServiceContracts;

namespace VSS.Hosted.VLCommon.Bss.Schema.V2
{
  [DataContract(Namespace = ContractConstants.NHBssSvc + "/v2/InstallBase", Name = "InstallBase")]
  public class InstallBase : BssCommon
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
    [DataMember(Order = 6, Name = "OwnerBSSID", IsRequired = true)]
    public string OwnerBSSID { get; set; }
    [DataMember(Order = 7, Name = "GPSDeviceID", IsRequired = true)]
    public string GPSDeviceID { get; set; }
    [DataMember(Order = 8, Name = "PartNumber", IsRequired = true)]
    public string PartNumber { get; set; }
    [DataMember(Order = 9, Name = "FirmwareVersionID", IsRequired = false)]
    public string FirmwareVersionID { get; set; }
    [DataMember(Order = 10, Name = "EquipmentSN", IsRequired = true)]
    public string EquipmentSN { get; set; }
    [DataMember(Order = 11, Name = "EquipmentVIN", IsRequired = false)]
    public string EquipmentVIN { get; set; }
    [DataMember(Order = 12, Name = "MakeCode", IsRequired = true)]
    public string MakeCode { get; set; }
    [DataMember(Order = 13, Name = "Model", IsRequired = false)]
    public string Model { get; set; }
    [DataMember(Order = 14, Name = "ModelYear", IsRequired = false)]
    public string ModelYear { get; set; }
    [DataMember(Order = 15, Name = "EquipmentLabel", IsRequired = false)]
    public string EquipmentLabel { get; set; }
    [DataMember(Order = 16, Name = "SIMSerialNumber", IsRequired = false)]
    public string SIMSerialNumber { get; set; }
    [DataMember(Order = 17, Name = "SIMState", IsRequired = false)]
    public string SIMState { get; set; }
    [DataMember(Order = 18, Name = "CellularModemIMEA", IsRequired = false)]
    public string CellularModemIMEA { get; set; }

    // V2.1 to differentiate DeviceTransfer from DeviceReplacement
    // DeviceState will be Active/Inactive based on Active SP in BSS.
    [DataMember(Order = 19, Name = "DeviceState", IsRequired = false)]
    public string DeviceState { get; set; }
    [DataMember(Order = 20, Name = "PreviousDeviceState", IsRequired = false)]
    public string PreviousDeviceState { get; set; }
    [DataMember(Order = 21, Name = "PreviousEquipmentSN", IsRequired = false)]
    public string PreviousEquipmentSN { get; set; }
    [DataMember(Order = 22, Name = "PreviousMakeCode", IsRequired = false)]
    public string PreviousMakeCode { get; set; }
  }
}

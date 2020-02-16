using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VSS.Hosted.VLCommon
{
  [DataContract]
  public class MTSEcmInfo
  {
    [DataMember]
    public string[] engineSerialNumbers;
    [DataMember]
    public string[] transmissionSerialNumbers;
    [DataMember]
    public byte datalink;
    [DataMember]
    public bool actingMasterECM;
    [DataMember]
    public bool syncSMUClockSupported;
    [DataMember]
    public byte eventProtocolVersion;
    [DataMember]
    public byte diagnosticProtocolVersion;
    [DataMember]
    public string mid1;
    [DataMember]
    public ushort toolSupportChangeLevel1;
    [DataMember]
    public ushort applicationLevel1;
    [DataMember]
    public ushort? mid2 = null;
    [DataMember]
    public ushort? toolSupportChangeLevel2 = null;
    [DataMember]
    public ushort? applicationLevel2 = null;
    [DataMember]
    public string softwarePartNumber = string.Empty;
    [DataMember]
    public string serialNumber = string.Empty;
    [DataMember]
    public string SoftwareDescription = null;
    [DataMember]
    public string ReleaseDate = null;
    [DataMember]
    public string PartNumber = null;
    [DataMember]
    public ushort? SourceAddress = null;
    [DataMember]
    public bool? ArbitraryAddressCapable;
    [DataMember]
    public byte? IndustryGroup;
    [DataMember]
    public byte? VehicleSystemInstance;
    [DataMember]
    public byte? VehicleSystem;
    [DataMember]
    public byte? Function;
    [DataMember]
    public byte? FunctionInstance;
    [DataMember]
    public byte? ECUInstance;
    [DataMember]
    public ushort? ManufacturerCode;
    [DataMember]
    public int? IdentityNumber;
    [DataMember]
    public string J1939Name;

    public long ecmID = -1;
  }
}

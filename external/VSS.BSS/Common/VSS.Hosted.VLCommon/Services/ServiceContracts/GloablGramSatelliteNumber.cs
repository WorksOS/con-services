using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VSS.Hosted.VLCommon
{
  [DataContract]
  public class GlobalGramSatelliteNumber
  {
    [DataMember]
    public string GPSDeviceID = string.Empty;
    [DataMember]
    public bool? GlobalGramEnabled;
    [DataMember]
    public int? SatelliteNumber;
    [DataMember]
    public int DeviceType;
  }
}

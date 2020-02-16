using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using System.Text;
using Newtonsoft.Json;
using VSS.Nighthawk.NHOPSvc.Interfaces.Helpers;
using VSS.Nighthawk.MassTransit;
using System.Collections;

namespace VSS.Nighthawk.NHOPSvc.Interfaces.Models
{
  [Serializable]
  public class ECMInfoList : INHOPDataObject
  {
    public ECMInfoList()
    {
      cdlEcmList = new List<ECM>();
      j1939EcmList = new List<ECM>();
    }

    public long AssetID { get; set; }
    public string GPSDeviceID { get; set; }
    public DeviceTypeEnum DeviceType { get; set; }
    public long? SourceMsgID { get; set; }
    public DateTime? TimeStampUtc { get; set; }
    [JsonConverter(typeof(PolymorphicIListConverter<List<ECM>>))]
    public List<ECM> cdlEcmList;
    [JsonConverter(typeof(PolymorphicIListConverter<List<ECM>>))]
    public List<ECM> j1939EcmList;
  }
}



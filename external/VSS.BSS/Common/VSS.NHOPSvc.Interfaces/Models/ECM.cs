using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Nighthawk.NHOPSvc.Interfaces.Models
{
  [Serializable]
  public class ECM
  {
    public string PartNumber { get; set; }
    public string SoftwarePartNumber { get; set; }
    public string J1939Name { get; set; }
    public ushort SourceAddress { get; set; }
    public string SerialNumber { get; set; }
    public int MID { get; set; }
    public bool SyncSMUClockSupported { get; set; }
    public bool ActingMasterECM { get; set; }
    public byte DataLink { get; set; }

    public ECM()
    {
      J1939Name = null;
      SourceAddress = 0;
    }
  }
}

using System;
using Newtonsoft.Json.Linq;
using VSS.Productivity3D.Models.ResultHandling.Coords;

namespace CoordinateSystemFileResolver.Interfaces
{
  public interface ICSIBAgent
    { 
      CSIBResult GetCSIBForProject(Guid projectUid, Guid customerUid);
      JObject GetCoordSysInfoFromCSIB64(Guid projectUid, string coordSysId);
      string GetCalibrationFileForCoordSysId(Guid projectUid, string csib);
  }
}

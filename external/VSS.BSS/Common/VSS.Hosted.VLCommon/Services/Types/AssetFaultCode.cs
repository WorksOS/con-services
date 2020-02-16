using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class AssetFaultCode
  {
    public long AssetID;
    public string NameOrSerialNumberVIN;
    public double? Latitude;
    public double? Longitude;
    public int HighFaultCodeCount;
    public int MediumFaultCodeCount;
    public int LowFaultCodeCount;    
    public int TotalFaultCodeCount;

    [ScriptIgnore] 
    public string Name;
    [ScriptIgnore]
    public string SerialNumberVIN;
  }
}

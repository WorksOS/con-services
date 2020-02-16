using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class AssetAlerts
  {
    public long AssetID;
    public string NameOrSerialNumberVIN;
    public double? Latitude;
    public double? Longitude;
    public int AlertCount;

    [ScriptIgnore] 
    public string Name;
    [ScriptIgnore] 
    public string SerialNumberVIN;
  }
}

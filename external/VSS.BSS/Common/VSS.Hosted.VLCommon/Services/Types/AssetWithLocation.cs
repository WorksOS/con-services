using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class AssetWithLocation
  {
    public long AssetID;
    public string NameOrSerialNumberVIN;
    public double? Latitude;
    public double? Longitude;

    [ScriptIgnore] 
    public string Name;

    [ScriptIgnore]
    public string SerialNumberVIN;
  }
}


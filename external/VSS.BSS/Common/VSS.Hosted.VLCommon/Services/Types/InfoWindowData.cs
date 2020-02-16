using System;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class InfoWindowData
  {
    public string NameOrSerialNumberVIN;
    public string AssetAddress;
    public double? Latitude;
    public double? Longitude;
    public string LastLocationUserTimeZone;
    public string DirectionsURL;

    [ScriptIgnore] // tells the JSON serializer not to serialize this property.  the client doesn't need it.
    public string Name;
    [ScriptIgnore]
    public string SerialNumberVIN;
    [ScriptIgnore]
    public DateTime? LastLocationUTC;

  }
}
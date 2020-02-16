using System;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class AssetFuelLevel
  {
    public long AssetID;
    public string NameOrSerialNumberVIN;
    public double? Latitude;
    public double? Longitude;
    public int? FuelPercentRemaining;

    [ScriptIgnore] // tells the JSON serializer not to serialize this property.  the client doesn't need it.
    public string Name;
    [ScriptIgnore]
    public string SerialNumberVIN;
  }
}

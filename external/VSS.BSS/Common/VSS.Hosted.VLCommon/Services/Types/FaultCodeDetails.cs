using System;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class FaultCodeDetails
  {
    public string FaultCodeTitle;
    public int SeverityID;

    [ScriptIgnore] // tells the JSON serializer not to serialize this property.  the client doesn't need it.
    public DateTime EventUTC;

    public string EventUserTime;
    public string Source;
    public double Lat;
    public double Lon;
    public string Location;

    [ScriptIgnore]
    public string AssetName;
    [ScriptIgnore]
    public string SerialNumberVIN;

    public string NameOrSerialNumberVIN;
    public int OccurrenceCount;
    public string OEMDetailsCode;//SIS Link for cat
    public bool ReverseGeocodeFromClient;
  }
}

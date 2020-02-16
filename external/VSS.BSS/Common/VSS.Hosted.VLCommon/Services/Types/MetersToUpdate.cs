using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class MetersToUpdate
  {
    public long AssetID;
    public string NameOrSerialNumberVIN;
    public double? LastReportedHourMeter;
    public double HourMeterMinValue;
    public double HourMeterMaxValue;

    [ScriptIgnore]
    public string Name;

    [ScriptIgnore]
    public string SerialNumberVIN;
  }
}

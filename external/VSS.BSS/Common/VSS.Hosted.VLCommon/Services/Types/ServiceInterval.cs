using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class ServiceInterval
  {

    [ScriptIgnore] // tell Json not to serialize this field
    public long ID;
    public string IntervalTitle;
    public int UpcomingOrOverdueStateID;
    public double OverdueByHours;
    public double DueInHours;
    public double DueAtHours;
    [ScriptIgnore]
    public double OverdueByMiles;
    public double OverdueByUserDistance;
    [ScriptIgnore]
    public double DueInMiles;
    public double DueInUserDistance;
    [ScriptIgnore]
    public double DueAtMiles;
    public double DueAtUserDistance;
    public string NameOrSerialNumberVIN;
    
    [ScriptIgnore]
    public int DueAtKeyDate;
    public string DueOnUserDate;

    public int TrackingTypeID;

    [ScriptIgnore]
    public double OdometerMiles;
    [ScriptIgnore]
    public double RuntimeMeterHours;

    public int AssetWorkingStateID;
    public string UserDistanceUnitsAbbrev;

    [ScriptIgnore]
    public string AssetName;
    [ScriptIgnore]
    public string SerialNumberVIN;        
  }
}

using System;
using System.Globalization;
using System.Text;
using System.Web.Script.Serialization;
using VSS.Hosted.VLCommon.Resources;

namespace VSS.Hosted.VLCommon
{
  public class AlertIncidentDescription
  {
    public string Title;

    public string IncidentUserTime;
    public string NameOrSerialNumberVIN;
    public string Location;
    public string Description;

    public double? Latitude;
    public double? Longitude;
    [ScriptIgnore]
    public string AssetName;
    [ScriptIgnore]
    public string SerialNumberVIN;
    [ScriptIgnore]
    public int AlertTypeNum;
    [ScriptIgnore]
    public string DataDescription;
    [ScriptIgnore]
    public string FaultDescription;
    [ScriptIgnore]
    public string CodedDescription;
    [ScriptIgnore]
    public string PMDescription;
    [ScriptIgnore]
    public DateTime IncidentUTC;

    [ScriptIgnore]
    public double? LoadDistanceMeters;
    [ScriptIgnore]
    public double? LoadDistanceThresholdMeters;

    [ScriptIgnore]
    public int? MachineStartMode;
    [ScriptIgnore]
    public int? AxlePosition;
    [ScriptIgnore]
    public int? TirePosition;
    [ScriptIgnore]
    public int? TirePressure;
    [ScriptIgnore]
    public double? TireTemperature;
    [ScriptIgnore]
    public int? PressureAlertStatus;
    [ScriptIgnore]
    public int? TemperatureAlertStatus;
    [ScriptIgnore]
    public int? BatteryAlertStatus;

    [ScriptIgnore]
    public int? AlertSubTypeID;
    [ScriptIgnore]
    public int? IncidentCount;
    [ScriptIgnore]
    public DateTime? FirstOccuranceUTC;
    [ScriptIgnore]
    public DateTime? LastOccuranceUTC;
    [ScriptIgnore]
    public double? DueAtRuntimeHoursMeter;
    [ScriptIgnore]
    public double? DueAtMiles;


  }
}

using System;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class ReportedAndPendingMetersForAsset
  {
    public long AssetID;
    public string NameOrSerialNumberVIN;
    public string FormattedReportedHourMeterDate;
    public string FormattedReportedHourMeter;
    public string FormattedPendingHourMeterDate;
    public string FormattedPendingHourMeter;
    public bool CanUserAdjustHourMeter;

    [ScriptIgnore]
    public DateTime? ReportedHourMeterUTCDate;

    [ScriptIgnore]
    public double? ReportedHourMeter;

    [ScriptIgnore]
    public DateTime? PendingHourMeterUTCDate;

    [ScriptIgnore]
    public double? PendingHourMeter;

    [ScriptIgnore]
    public string Name;

    [ScriptIgnore]
    public string SerialNumberVIN;

    [ScriptIgnore]
    public string ModuleType;

    [ScriptIgnore]
    public int ServiceMeterSource;

    [ScriptIgnore]
    public bool ServiceMeterSourceIsPending;
  }
}
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class UtilizationHoursDetails
  {
    public string DateRange { get; set; }
    public long AssetID { get; set; }
    public string NameOrSerialNumberVIN;

    [ScriptIgnore]
    public double? IdleHours { get; set; }
    [ScriptIgnore]
    public double? WorkingHours { get; set; }
    [ScriptIgnore]
    public double? RuntimeHours { get; set; }

    public string IdleHoursString { get; set; }
    public string WorkingHoursString { get; set; }
    public string RuntimeHoursString { get; set; }

    [ScriptIgnore]
    public UtilizationCalloutDisplayResultEnum idleHoursUtilizationCalloutDisplayResult;
    [ScriptIgnore]
    public UtilizationCalloutDisplayResultEnum workingHoursUtilizationCalloutDisplayResult;
    [ScriptIgnore]
    public UtilizationCalloutDisplayResultEnum runtimeHoursUtilizationCalloutDisplayResult;

    public List<int> RuntimeHoursCalloutTypeIDs { get; set; }
    public List<int> IdleHoursCalloutTypeIDs { get; set; }
    public List<int> WorkingHoursCalloutTypeIDs { get; set; }
  };
}
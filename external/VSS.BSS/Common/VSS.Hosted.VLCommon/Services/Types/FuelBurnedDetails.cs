using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class FuelBurnedDetails
  {
    public string DateRange { get; set; }
    public long AssetID { get; set; }
    public string NameOrSerialNumberVIN;

    [ScriptIgnore]
    public double? RunningFuelBurned { get; set; }
    [ScriptIgnore]
    public double? IdleFuelBurned { get; set; }
    [ScriptIgnore]
    public double? WorkingFuelBurned { get; set; }
    [ScriptIgnore]
    public double? RunningBurnRate { get; set; }

    public string RunningFuelBurnedString { get; set; }
    public string IdleFuelBurnedString { get; set; }
    public string WorkingFuelBurnedString { get; set; }    
    public string RunningBurnRateString { get; set; }

    [ScriptIgnore]
    public UtilizationCalloutDisplayResultEnum runningFuelBurnedUtilizationCalloutDisplayResult;
    [ScriptIgnore]
    public UtilizationCalloutDisplayResultEnum idleFuelBurnedUtilizationCalloutDisplayResult;
    [ScriptIgnore]
    public UtilizationCalloutDisplayResultEnum workingFuelBurnedUtilizationCalloutDisplayResult;    
    [ScriptIgnore]
    public UtilizationCalloutDisplayResultEnum runningBurnRateUtilizationCalloutDisplayResult;

    public List<int> RunningFuelBurnedCalloutTypeIDs { get; set; }
    public List<int> IdleFuelBurnedCalloutTypeIDs { get; set; }
    public List<int> WorkingFuelBurnedCalloutTypeIDs { get; set; }    
    public List<int> RunningBurnRateCalloutTypeIDs { get; set; }
  };
}
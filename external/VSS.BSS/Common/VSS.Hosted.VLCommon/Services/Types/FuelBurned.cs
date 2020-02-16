using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class FuelBurned
  {
    public long AssetID { get; set; }
    public string NameOrSerialNumberVIN;

    [ScriptIgnore]
    public double? FuelBurnedValue { get; set; }

    public string FuelBurnedValueString { get; set; }

    [ScriptIgnore] 
    public UtilizationCalloutDisplayResultEnum utilizationCalloutDisplayResult;
    
    public List<int> CalloutTypeIDs { get; set; }
  };
}
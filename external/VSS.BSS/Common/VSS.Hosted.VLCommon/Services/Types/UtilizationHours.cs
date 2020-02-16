using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class UtilizationHours
  {
    public long AssetID { get; set; }
    public string NameOrSerialNumberVIN;

    [ScriptIgnore]
    public double? Hours { get; set; }

    public string HoursString { get; set; }

    [ScriptIgnore] 
    public UtilizationCalloutDisplayResultEnum utilizationCalloutDisplayResult;
    
    public List<int> CalloutTypeIDs { get; set; }
  };
}
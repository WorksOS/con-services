using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class ServiceIntervalItemsContainer
  {
    [ScriptIgnore] //tell Json not to serialize this field
    public long AssetID;
    [ScriptIgnore]
    public long IconID;
    [ScriptIgnore]
    public string AssetName;
    [ScriptIgnore]
    public string SerialNumberVIN;
    
    public string NameOrSerialNumberVIN;
    public double RuntimeHours = 0;
    public double Odometer = 0;
    public int MeterLabelPreferenceTypeID;
    public int AssetWorkingStateID;
    public string UserDistanceUnitsAbbrev;
    public List<ServiceIntervalItem> ServiceIntervalItems;
  }
}

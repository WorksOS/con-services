using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class AlertContainer
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
    public string DateRange;
    public int Count;
    public List<AlertIncidentItem> AlertIncidentItems;
  }
}

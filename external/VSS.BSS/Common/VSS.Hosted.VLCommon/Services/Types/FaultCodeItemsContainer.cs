using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class FaultCodeItemsContainer
  {
    [ScriptIgnore] // tells the JSON serializer not to serialize this property.  the client doesn't need it.
    public long AssetID;
    [ScriptIgnore] // tells the JSON serializer not to serialize this property.  the client doesn't need it.
    public long IconID;
    [ScriptIgnore]
    public string AssetName;
    [ScriptIgnore]
    public string SerialNumberVIN;

    public string NameOrSerialNumberVIN;
    public string DateRange;
    public List<FaultCodeItem> FaultCodeItems;
    [ScriptIgnore]
    public string MakeCode;
  }
}

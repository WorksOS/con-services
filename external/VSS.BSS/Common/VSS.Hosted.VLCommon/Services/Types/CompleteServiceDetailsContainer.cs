using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class CompleteServiceDetailsContainer
  {
    public long AssetID;
    public string NameOrSerialNumberVIN;
    public List<string> AccountNames;    
    public double MaxRuntimeHours;
    public double MaxOdometer;
    public long ServiceIntervalID;
    public int TrackingTypeID;
    public string IntervalTitle;
    public int UpcomingOrOverdueStateID;

    [ScriptIgnore] // tells the JSON serializer not to serialize this property.  the client doesn't need it.
    public string Name;
    [ScriptIgnore]
    public string SerialNumberVIN;

  }
}

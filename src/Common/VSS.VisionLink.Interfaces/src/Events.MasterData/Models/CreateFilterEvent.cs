using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class CreateFilterEvent : IFilterEvent
  {
    public string CustomerUID { get; set; }

    [Obsolete]
    public string UserUID { get; set; }

    // UserID will include either a UserUID (GUID) or ApplicationID (string)
    public string UserID { get; set; }
    public string ProjectUID { get; set; }
    public string FilterUID { get; set; }
    public string Name { get; set; }
    public string FilterJson { get; set; }
    public FilterType FilterType { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }
}

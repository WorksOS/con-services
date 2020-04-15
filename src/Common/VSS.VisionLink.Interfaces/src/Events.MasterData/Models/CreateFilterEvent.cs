using System;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Events.MasterData.Models
{
  public class CreateFilterEvent : IFilterEvent
  {
    public Guid CustomerUID { get; set; }
       
    // UserID will include either a UserUID (GUID) or ApplicationID (string)
    public string UserID { get; set; }
    public Guid ProjectUID { get; set; }
    public Guid FilterUID { get; set; }
    public string Name { get; set; }
    public string FilterJson { get; set; }
    public FilterType FilterType { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}

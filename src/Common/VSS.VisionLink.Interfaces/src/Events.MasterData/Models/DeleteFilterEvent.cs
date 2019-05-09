using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class DeleteFilterEvent : IFilterEvent
  {
    public Guid FilterUID { get; set; }

    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }

    // the following are provided to allow for insert where the delete arrives from kafka before the Create
    public Guid CustomerUID { get; set; }

    [Obsolete]
    public Guid UserUID { get; set; }

    // UserID will include either a UserUID (GUID) or ApplicationID (string)
    public string UserID { get; set; }
    public Guid ProjectUID { get; set; }
  }
}
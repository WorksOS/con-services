using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Models
{
  public class DeleteFilterEvent : IFilterEvent
  {
    public string FilterUID { get; set; }

    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }

    // the following are provided to allow for insert where the delete arrives from old kafka before the Create
    public string CustomerUID { get; set; }

    [Obsolete]
    public string UserUID { get; set; }

    // UserID will include either a UserUID (GUID) or ApplicationID (string)
    public string UserID { get; set; }
    public string ProjectUID { get; set; }
  }
}

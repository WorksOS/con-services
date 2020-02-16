using System;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
    public class DeleteGroupEvent
  {
    public Guid GroupUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }

  }
}

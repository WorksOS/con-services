using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
  public class CreateGroupEvent 
  {
    public string GroupName { get; set; }
    public List<Guid> AssetUID { get; set; }
    public Guid GroupUID { get; set; }
    public Guid CustomerUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }

  public class UpdateGroupEvent
  {
    public string GroupName { get; set; }
    public List<Guid> AssociatedAssetUID { get; set; }
    public List<Guid> DissociatedAssetUID { get; set; }
    public Guid GroupUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }
  }

  public class DeleteGroupEvent
  {
    public Guid GroupUID { get; set; }
    public Guid UserUID { get; set; }
    public DateTime ActionUTC { get; set; }
    public DateTime ReceivedUTC { get; set; }

  }
}

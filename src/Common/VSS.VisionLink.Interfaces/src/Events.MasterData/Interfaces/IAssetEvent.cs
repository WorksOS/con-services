using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
  public interface IAssetEvent
  {
    Guid AssetUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}
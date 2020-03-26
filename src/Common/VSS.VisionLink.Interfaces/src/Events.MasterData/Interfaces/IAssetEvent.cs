using System;

namespace VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces
{
  public interface IAssetEvent
  {
    Guid AssetUID { get; set; }
    DateTime ActionUTC { get; set; }
    DateTime ReceivedUTC { get; set; }
  }
}

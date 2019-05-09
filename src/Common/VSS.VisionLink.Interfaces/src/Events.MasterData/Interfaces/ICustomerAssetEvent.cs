using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Interfaces
{
  public interface ICustomerAssetEvent : ICustomerEvent
  {
    Guid AssetUID { get; set; }
  }
}
using System;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public interface IBssAssetDeviceHistoryService
  {
    AssetDeviceHistory CreateAssetDeviceHistory(long assetId, long deviceId, string ownerBssId, DateTime startUtc);
  }
}

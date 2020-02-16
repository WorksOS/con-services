using System;
using System.Linq;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssAssetDeviceHistoryService : IBssAssetDeviceHistoryService
  {
    public AssetDeviceHistory CreateAssetDeviceHistory(long assetId, long deviceId, string ownerBssId, DateTime startUtc)
    {
      return API.ServiceView.CreateAssetDeviceHistory(Data.Context.OP, assetId, deviceId, ownerBssId, startUtc);
    }
  }
}
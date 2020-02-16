using System;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  public class BssAssetDeviceHistoryServiceExceptionFake : IBssAssetDeviceHistoryService
  {

    public bool WasExecuted { get; set; }

    public AssetDeviceHistory CreateAssetDeviceHistory(long assetId, long deviceId, string ownerBssId, DateTime startUtc)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }
  }
}
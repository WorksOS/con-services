using System;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  public class BssAssetDeviceHistoryServiceFake : IBssAssetDeviceHistoryService 
  {
    private readonly AssetDeviceHistory _assetDeviceHistoryToReturn;

    public DateTime StartUtcArg { get; set; }
    public string OwnerBssIdArg { get; set; }
    public long DeviceIdArg { get; set; }
    public long AssetIdArg { get; set; }
    public bool WasExecuted { get; set; }

    public BssAssetDeviceHistoryServiceFake(AssetDeviceHistory assetDeviceHistoryToReturn)
    {
      _assetDeviceHistoryToReturn = assetDeviceHistoryToReturn;
    }

    public AssetDeviceHistory CreateAssetDeviceHistory(long assetId, long deviceId, string ownerBssId, DateTime startUtc)
    {
      AssetIdArg = assetId;
      DeviceIdArg = deviceId;
      OwnerBssIdArg = ownerBssId;
      StartUtcArg = startUtc;

      WasExecuted = true;

      return _assetDeviceHistoryToReturn;
    }
  }
}
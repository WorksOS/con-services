using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.MasterDataConsumer.Tests
{
  [TestClass]
  public class AssetEventsTests
  {
    [TestMethod]
    public void AssetEventsCopyModels()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var asset = new Asset()
      {
        AssetUID = Guid.NewGuid().ToString(),
        Name = "The Asset Name",
        AssetType = "whatever",
        LastActionedUtc = now
      };

      var kafkaAssetEvent = CopyModel(asset);
      var copiedAsset = CopyModel(kafkaAssetEvent);

      Assert.AreEqual(asset, copiedAsset, "Asset model conversion not completed sucessfully");
    }

    #region private
    private CreateAssetEvent CopyModel(Asset asset)
    {
      return new CreateAssetEvent()
      {
        AssetUID = Guid.Parse(asset.AssetUID),
        AssetName = asset.Name,
        AssetType = asset.AssetType,
        ActionUTC = asset.LastActionedUtc.HasValue ? asset.LastActionedUtc.Value : new DateTime(2017, 1, 1)
    };
    }

    private Asset CopyModel(CreateAssetEvent kafkaAssetEvent)
    {
      return new Asset()
      {
        AssetUID = kafkaAssetEvent.AssetUID.ToString(),
        Name = kafkaAssetEvent.AssetName,
        AssetType = kafkaAssetEvent.AssetType,
        LastActionedUtc = kafkaAssetEvent.ActionUTC
      };
    }
    #endregion

  }
}

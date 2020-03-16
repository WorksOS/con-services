using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public interface IAssetRepository
  {
    Task<Asset> GetAsset(string assetUid);
    Task<Asset> GetAsset(long legacyAssetId);
    Task<IEnumerable<Asset>> GetAssets();
    Task<IEnumerable<Asset>> GetAllAssetsInternal();
    Task<IEnumerable<Asset>> GetAssets(string[] productFamily);
    Task<IEnumerable<Asset>> GetAssets(IEnumerable<Guid> assetUids);
    Task<IEnumerable<Asset>> GetAssets(IEnumerable<long> assetIds);
    Task<int> StoreEvent(IAssetEvent filterEvent);
    Task<MatchingAssets> GetMatching3D2DAssets(MatchingAssets asset);
  }
}

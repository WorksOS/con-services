using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon.Services.Types;

namespace VSS.Hosted.VLCommon
{
  public interface IAssetFeatureAPI
  {
    Dictionary<long, List<AppFeatureEnum>> GetAssetsThatSupportAppFeatures(IEnumerable<long> assetIDs, IEnumerable<AppFeatureEnum> features, long customerID);

    List<AssetInfo> GetAssetSearchResults(string searchTerm);

    List<int> GetActiveServicePlans(long CustomerID, long assetID, int startDate, int endDate);
    
    AssetAlias GetAssetIDChanges(long assetID);

    bool DoesAssetSupportFeature(long assetId, AppFeatureEnum feature);

    List<DevicePersonality> GetDevicePersonality(string gpsDeviceID);
    
  }
}

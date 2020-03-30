using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.AssetMgmt3D.Models;

namespace VSS.Productivity3D.AssetMgmt3D.Extensions
{
  /// <summary>
  /// Extension methods for the <see cref="Asset"/> class.
  /// </summary>
  public static class AssetExtensions
  {
    /// <summary>
    /// Convert a List of Asset Database Models to Display Models, validating the Guid string can be parsed
    /// </summary>
    public static AssetDisplayModel ConvertDbAssetToDisplayModel(this IEnumerable<Asset> assets)
    {
      var results = assets.Select(a => Guid.TryParse(a.AssetUID, out var g)
                                    ? new KeyValuePair<Guid, long>(g, a.LegacyAssetID)
                                    : new KeyValuePair<Guid, long>(Guid.Empty, a.LegacyAssetID)).ToList();

      return new AssetDisplayModel
      {
        assetIdentifiers = results
      };
    }
  }
}

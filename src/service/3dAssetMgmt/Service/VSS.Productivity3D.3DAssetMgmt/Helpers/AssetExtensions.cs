using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.AssetMgmt3D.Models;

namespace VSS.Productivity3D.AssetMgmt3D.Helpers
{
  /// <summary>
  /// Helper methods for the <see cref="Asset"/> class.
  /// </summary>
  public class AssetExtensions
  {
    private readonly ILogger _log;

    public AssetExtensions(ILoggerFactory logger)
    {
      _log = logger.CreateLogger(GetType().Name);
    }

    /// <summary>
    /// Convert a List of Asset Database Models to Display Models, validating the Guid string can be parsed
    /// </summary>
    public AssetDisplayModel ConvertDbAssetToDisplayModel(IEnumerable<Asset> assets)
    {
      var results = assets.Select(a =>
      {
        if (Guid.TryParse(a.AssetUID, out var g))
        {
          return new KeyValuePair<Guid, long>(g, a.LegacyAssetID);
        }

        _log.LogWarning($"Failed to parse {a.AssetUID} to a guid for AssetID: {a.LegacyAssetID}");
        return new KeyValuePair<Guid, long>(Guid.Empty, a.LegacyAssetID);
      }).ToList();

      _log.LogInformation($"Matched assets: {JsonConvert.SerializeObject(results)}");

      return new AssetDisplayModel
      {
        assetIdentifiers = results
      };
    }
  }
}

using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using VSS.TagFileAuth.Service.Models;
using VSS.TagFileAuth.Service.WebApi.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TagFileAuth.Service.Repositories
{

  public class AssetRepository : RepositoryBase, IAssetRepository
  {
    private readonly ILogger log;
    public AssetRepository(string connectionString, ILoggerFactory logger)
        : base(connectionString)
    {
      log = logger.CreateLogger<AssetRepository>();
    }

    // needed for TFAS
    public async Task<AssetDevice> GetAssociatedAsset(string radioSerial, string deviceType)
    {
      try
      {
        await PerhapsOpenConnection();
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          return (await Connection.QueryAsync<AssetDevice>
                  (@"SELECT 
                        AssetUID, LegacyAssetID, OwningCustomerUID, DeviceUid, DeviceType, DeviceSerialNumber AS RadioSerial
                      FROM Device d
                        INNER JOIN AssetDevice ad ON ad.fk_DeviceUID = d.DeviceUID
                        INNER JOIN Asset a ON a.AssetUID = ad.fk_AssetUID
                      WHERE d.DeviceSerialNumber = @radioSerial
                        AND a.IsDeleted = 0
                        AND d.DeviceType LIKE @deviceType"
                      , new { radioSerial, deviceType }
                  )).FirstOrDefault();
        });
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }

    /// <summary>
    /// // need this for unit tests
    /// </summary>
    /// <param name="evt"></param>
    /// <returns></returns>
    public async Task<int> StoreAsset(IAssetEvent evt)
    {
      var upsertedCount = 0;
      var asset = new Asset();
      string eventType = "Unknown";
      if (evt is CreateAssetEvent)
      {
        var assetEvent = (CreateAssetEvent)evt;
        asset.AssetUid = assetEvent.AssetUID.ToString();
        asset.LegacyAssetID = assetEvent.LegacyAssetId;
        asset.Name = assetEvent.AssetName;
        asset.SerialNumber = assetEvent.SerialNumber;
        asset.MakeCode = assetEvent.MakeCode;
        asset.Model = assetEvent.Model;
        asset.AssetType = string.IsNullOrEmpty(assetEvent.AssetType) ? "Unassigned" : assetEvent.AssetType;
        asset.OwningCustomerUID = assetEvent.OwningCustomerUID.HasValue ? assetEvent.OwningCustomerUID.ToString() : "";
        asset.IconKey = assetEvent.IconKey;
        asset.IsDeleted = false;
        asset.LastActionedUtc = assetEvent.ActionUTC;
        eventType = "CreateAssetEvent";
      }
      //else if (evt is UpdateAssetEvent)
      //{
      //  var assetEvent = (UpdateAssetEvent)evt;
      //  asset.AssetUid = assetEvent.AssetUID.ToString();
      //  asset.Name = assetEvent.AssetName;
      //  asset.Model = assetEvent.Model;
      //  asset.AssetType = string.IsNullOrEmpty(assetEvent.AssetType) ? "Unassigned" : assetEvent.AssetType;
      //  asset.IconKey = assetEvent.IconKey;
      //  asset.IsDeleted = false;
      //  asset.LastActionedUtc = assetEvent.ActionUTC;
      //  eventType = "UpdateAssetEvent";
      //}
      //else if (evt is DeleteAssetEvent)
      //{
      //  var assetEvent = (DeleteAssetEvent)evt;
      //  asset.AssetUid = assetEvent.AssetUID.ToString();
      //  asset.IsDeleted = true;
      //  asset.LastActionedUtc = assetEvent.ActionUTC;
      //  eventType = "DeleteAssetEvent";
      //}

      upsertedCount = await UpsertAssetDetail(asset, eventType);
      return upsertedCount;
    }

    /// <summary>
    /// All detail-related columns can be inserted, 
    ///    but only certain columns can be updated.
    ///    on deletion, a flag will be set.
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertAssetDetail(Asset asset, string eventType)
    {
      try
      {
        {
          await PerhapsOpenConnection();
          log.LogDebug("AssetRepository: Upserting eventType{0} assetUid={1}", eventType, asset.AssetUid);
          var upsertedCount = 0;

          var existing = await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return (await Connection.QueryAsync<Asset>
                      (@"SELECT 
                              AssetUID AS AssetUid, Name, MakeCode, Model, SerialNumber, IconKey, AssetType, IsDeleted,
                              LastActionedUTC AS LastActionedUtc
                            FROM Asset
                            WHERE AssetUID = @assetUid"
                          , new { assetUid = asset.AssetUid }
                      )).FirstOrDefault();
          });

          if (existing == null || existing.IsDeleted == false)
          {
            if (eventType == "CreateAssetEvent")
            {
              upsertedCount = await CreateAsset(asset, existing);
            }

            //if (eventType == "UpdateAssetEvent")
            //{
            //  upsertedCount = await UpdateAsset(asset, existing);
            //}

            //if (eventType == "DeleteAssetEvent")
            //{
            //  upsertedCount = await DeleteAsset(asset, existing);
            //}
          }
          log.LogDebug("AssetRepository: upserted {0} rows", upsertedCount);
          log.LogInformation("Event stored SUCCESS: {0}, {1}", eventType, JsonConvert.SerializeObject(asset));
          return upsertedCount;
        }
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }

    private async Task<int> CreateAsset(Asset asset, Asset existing)
    {
      try
      {
        await PerhapsOpenConnection();
        if (existing == null)
        {
          const string upsert =
              @"INSERT Asset
                    (AssetUID, Name, MakeCode, Model, SerialNumber, IconKey, AssetType, IsDeleted, LastActionedUTC )
                  VALUES
                   (@AssetUid, @Name, @MakeCode, @Model, @SerialNumber, @IconKey, @AssetType, @IsDeleted, @LastActionedUtc)
          ";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return await Connection.ExecuteAsync(upsert, asset);
          });
        }
        else if (existing.LastActionedUtc == null
                 // was generated by one of the internal updates e.g. LastUpdatedTime
                 || asset.LastActionedUtc >= existing.LastActionedUtc) // potential intentional reprocessing
        {
          const string update =
              @"UPDATE Asset                
                  SET Name = @Name,
                      MakeCode = @MakeCode,
                      Model = @Model,
                      SerialNumber = @SerialNumber,
                      IconKey = @IconKey,     
                      AssetType = @AssetType,         
                      LastActionedUTC = @LastActionedUtc
                WHERE AssetUID = @AssetUid";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return await Connection.ExecuteAsync(update, asset);
          });
        }
        else if (asset.LastActionedUtc < existing.LastActionedUtc) // Create received after Update
        {
          const string update =
              @"UPDATE Asset                
                  SET MakeCode = @MakeCode,
                    SerialNumber = @SerialNumber
                  WHERE AssetUID = @AssetUid";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            return await Connection.ExecuteAsync(update, asset);
          });
        }

        return await Task.FromResult(0);
      }
      finally
      {
        PerhapsCloseConnection();
      }
    }

    //private async Task<int> DeleteAsset(Asset asset, Asset existing)
    //{
    //  try
    //  {
    //    await PerhapsOpenConnection();
    //    if (existing != null)
    //    {
    //      if (asset.LastActionedUtc >= existing.LastActionedUtc)
    //      {
    //        const string update =
    //            @"UPDATE Asset                
    //                SET IsDeleted = 1,
    //                  LastActionedUTC = @LastActionedUtc
    //                WHERE AssetUID = @AssetUid";
    //        return await dbAsyncPolicy.ExecuteAsync(async () =>
    //        {
    //          return await Connection.ExecuteAsync(update, asset);
    //        });
    //      }
    //      else
    //      {
    //        log.LogDebug(
    //            "AssetRepository: old delete event ignored currentActionedUTC{0} newActionedUTC{1}",
    //            existing.LastActionedUtc, asset.LastActionedUtc);
    //      }
    //    }
    //    else
    //    {
    //      log.LogDebug("AssetRepository: Inserted a DeleteAssetEvent as none existed. newActionedUTC{0}",
    //          asset.LastActionedUtc);

    //      string upsert = string.Format(
    //            "INSERT Asset " +
    //            "    (AssetUID, IsDeleted, LastActionedUTC, AssetType) " +
    //            "  VALUES " +
    //            "   (@AssetUid, @IsDeleted, @LastActionedUtc, \"Unassigned\")");
    //      return await dbAsyncPolicy.ExecuteAsync(async () =>
    //      {
    //        return await Connection.ExecuteAsync(@upsert, asset);
    //      });
    //    }
    //    return await Task.FromResult(0);
    //  }
    //  finally
    //  {
    //    PerhapsCloseConnection();
    //  }
    //}

    //private async Task<int> UpdateAsset(Asset asset, Asset existing)
    //{
    //  try
    //  {
    //    await PerhapsOpenConnection();
    //    if (existing != null)
    //    {
    //      if (asset.LastActionedUtc >= existing.LastActionedUtc)
    //      {
    //        const string update =
    //            @"UPDATE Asset                
    //                SET Name = @Name,
    //                  Model = @Model,
    //                  IconKey = @IconKey,
    //                  AssetType = @AssetType,
    //                  LastActionedUTC = @LastActionedUtc
    //                WHERE AssetUID = @AssetUid";
    //        return await dbAsyncPolicy.ExecuteAsync(async () =>
    //        {
    //          return await Connection.ExecuteAsync(update, asset);
    //        });
    //      }
    //      else
    //      {
    //        log.LogDebug(
    //            "AssetRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
    //            existing.LastActionedUtc, asset.LastActionedUtc);
    //      }
    //    }
    //    else
    //    {
    //      log.LogDebug("AssetRepository: Inserted an UpdateAssetEvent as none existed.  newActionedUTC{0}",
    //          asset.LastActionedUtc);

    //      const string upsert =
    //          @"INSERT Asset
    //                (AssetUID, Name, Model, IconKey, AssetType, IsDeleted, LastActionedUTC )
    //              VALUES
    //                (@AssetUid, @Name, @Model, @IconKey,@AssetType, @IsDeleted, @LastActionedUtc)
    //      ";
    //      return await dbAsyncPolicy.ExecuteAsync(async () =>
    //      {
    //        return await Connection.ExecuteAsync(upsert, asset);
    //      });
    //    }
    //    return await Task.FromResult(0);
    //  }
    //  finally
    //  {
    //    PerhapsCloseConnection();
    //  }
    //}

    //public async Task<Asset> GetAsset(string assetUid)
    //{
    //  try
    //  {
    //    await PerhapsOpenConnection();
    //    return await dbAsyncPolicy.ExecuteAsync(async () =>
    //    {
    //      return (await Connection.QueryAsync<Asset>
    //              (@"SELECT 
    //                    AssetUID AS AssetUid, Name, MakeCode, Model, SerialNumber, IconKey, AssetType, IsDeleted,
    //                    LastActionedUTC AS LastActionedUtc
    //                  FROM Asset
    //                  WHERE AssetUID = @assetUid 
    //                    AND IsDeleted = 0"
    //                  , new { assetUid }
    //              )).FirstOrDefault();
    //    });
    //  }
    //  finally
    //  {
    //    PerhapsCloseConnection();
    //  }
    //}

    //public async Task<IEnumerable<Asset>> GetAssets()
    //{
    //  try
    //  {
    //    await PerhapsOpenConnection();
    //    return await dbAsyncPolicy.ExecuteAsync(async () =>
    //    {
    //      return (await Connection.QueryAsync<Asset>
    //              (@"SELECT 
    //                    AssetUID AS AssetUid, Name, MakeCode, Model, SerialNumber,  IconKey, AssetType, IsDeleted,
    //                    LastActionedUTC AS LastActionedUtc
    //                  FROM Asset
    //                  WHERE IsDeleted = 0"
    //              )).ToList();
    //    });
    //  }
    //  finally
    //  {
    //    PerhapsCloseConnection();
    //  }
    //}

    ///// <summary>
    ///// Used for unit tests so we can test deleted assets
    ///// </summary>
    ///// <returns></returns>
    //public async Task<IEnumerable<Asset>> GetAllAssetsInternal()
    //{
    //  try
    //  {
    //    await PerhapsOpenConnection();
    //    return await dbAsyncPolicy.ExecuteAsync(async () =>
    //    {
    //      return (await Connection.QueryAsync<Asset>
    //              (@"SELECT 
    //                    AssetUID AS AssetUid, Name, MakeCode, Model, SerialNumber,  IconKey, AssetType, IsDeleted,
    //                    LastActionedUTC AS LastActionedUtc
    //                  FROM Asset"
    //              )).ToList();
    //    });
    //  }
    //  finally
    //  {
    //    PerhapsCloseConnection();
    //  }
    //}


    //public async Task<IEnumerable<Asset>> GetAssets(string[] productFamily)
    //{
    //  try
    //  {
    //    await PerhapsOpenConnection();
    //    return await dbAsyncPolicy.ExecuteAsync(async () =>
    //    {
    //      return (await Connection.QueryAsync<Asset>
    //              (@"SELECT 
    //                    AssetUID AS AssetUid, Name, MakeCode, Model, SerialNumber,  IconKey, AssetType, IsDeleted,
    //                    LastActionedUTC AS LastActionedUtc
    //                  FROM Asset 
    //                  WHERE AssetType IN @families
    //                    AND IsDeleted = 0", new { families = productFamily })).ToList();
    //    });
    //  }
    //  finally
    //  {
    //    PerhapsCloseConnection();
    //  }
    //}


    //#region AssetCount
    ///// <summary>
    /////  must have either 
    /////      1 no parameters, which means count of 'All Assets'
    /////      2 grouping OR parameter
    /////  The only valid AssetCountGrouping is ProductFamily
    /////  
    /////  If there is a grouping, 
    /////      then a list of existing productFamilies with count on each is returned 
    /////  If there is not a grouping and no parameters
    /////      then the count of ALL Assets (>=0) meeting any other criteria is returned
    /////  If there is not a grouping but a list of parameters
    /////      then the count of ALL Assets (>=0) with the ProductFamily and meeting any other criteria is returned
    ///// </summary>
    ///// <param name="grouping"></param>
    ///// <param name="productFamily"></param>
    ///// <returns></returns>
    //public async Task<List<CategoryCount>> GetAssetCount(AssetCountGrouping? grouping, string[] productFamily)
    //{
    //  var assetCount = new List<CategoryCount>();

    //  if (grouping.HasValue)
    //    if ((productFamily != null && productFamily.Count() > 0)
    //        || (grouping.Value != AssetCountGrouping.ProductFamily))
    //      return assetCount;

    //  try
    //  {
    //    await PerhapsOpenConnection();

    //    if (grouping.HasValue)
    //    {
    //      return await dbAsyncPolicy.ExecuteAsync(async () =>
    //      {
    //        return (await Connection.QueryAsync<CategoryCount>
    //        (@"SELECT 
    //                 AssetType AS CountOf, COUNT(1) AS Count
    //               FROM Asset 
    //               WHERE IsDeleted = 0 
    //               GROUP BY AssetType
    //               ORDER BY AssetType")).ToList();
    //      });
    //    }
    //    else
    //    if (productFamily != null && productFamily.Count() > 0)
    //    {
    //      return await dbAsyncPolicy.ExecuteAsync(async () =>
    //      {
    //        string queryString = string.Format(
    //            "SELECT " +
    //            "    \"All Assets\" AS CountOf, COUNT(1) AS Count " +
    //            "  FROM Asset " +
    //            "  WHERE AssetType IN @families " +
    //            "    AND IsDeleted = 0");
    //        return (await Connection.QueryAsync<CategoryCount>(@queryString, new { families = productFamily })).ToList();

    //      });
    //    }
    //    else
    //    {
    //      return await dbAsyncPolicy.ExecuteAsync(async () =>
    //      {
    //        string queryString = string.Format(
    //            "SELECT " +
    //            "    \"All Assets\" AS CountOf, COUNT(1) AS Count " +
    //            "  FROM Asset " +
    //            "  WHERE IsDeleted = 0");
    //        return (await Connection.QueryAsync<CategoryCount>(@queryString)).ToList();
    //      });
    //    }
    //  }
    //  finally
    //  {
    //    PerhapsCloseConnection();
    //  }
    //}

    //// leave this in for now as there is a case in S&F WebAPI (not VSP) which requires range filters to report totals on each requested group
    ////private List<CategoryCount> FormatList(string[] productFamily, IEnumerable<CategoryCount> foundCategories)
    ////{
    ////  List<CategoryCount> categoryCounts = new List<CategoryCount>();
    ////  foreach (string f in productFamily)
    ////  {
    ////    var gotType = foundCategories == null ? null : foundCategories.FirstOrDefault(x => string.Compare(x.CountOf, f, true) == 0);
    ////    categoryCounts.Add(new CategoryCount() { CountOf = f, Count = (gotType == null ? 0 : gotType.Count) });
    ////  }
    ////  return categoryCounts;
    ////}
    //#endregion
  }
}
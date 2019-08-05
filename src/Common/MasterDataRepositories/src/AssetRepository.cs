using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories
{
  public class AssetRepository : RepositoryBase, IRepository<IAssetEvent>, IAssetRepository
  {
    public AssetRepository(IConfigurationStore connectionString, ILoggerFactory logger) : base(connectionString,
      logger)
    {
      Log = logger.CreateLogger<AssetRepository>();
    }

    #region store


    public async Task<int> StoreEvent(IAssetEvent evt)
    {
      var asset = new Asset();
      var eventType = "Unknown";
      if (evt == null)
      {
        Log.LogWarning("Unsupported event type");
        return 0;
      }

      Log.LogDebug($"Event type is {evt.GetType()}");
      if (evt is CreateAssetEvent createAssetEvent)
      {
        asset.Name = createAssetEvent.AssetName;
        asset.AssetType = string.IsNullOrEmpty(createAssetEvent.AssetType) ? null : createAssetEvent.AssetType;
        asset.AssetUID = createAssetEvent.AssetUID.ToString();
        asset.EquipmentVIN = createAssetEvent.EquipmentVIN;
        asset.LegacyAssetID = createAssetEvent.LegacyAssetId;
        asset.SerialNumber = createAssetEvent.SerialNumber;
        asset.MakeCode = createAssetEvent.MakeCode;
        asset.Model = createAssetEvent.Model;
        asset.ModelYear = createAssetEvent.ModelYear;
        asset.IconKey = createAssetEvent.IconKey;
        asset.OwningCustomerUID = createAssetEvent.OwningCustomerUID.HasValue && createAssetEvent.OwningCustomerUID.Value != Guid.Empty
          ? createAssetEvent.OwningCustomerUID.ToString()
          : null;
        asset.IsDeleted = false;
        asset.LastActionedUtc = createAssetEvent.ActionUTC;
        eventType = "CreateAssetEvent";
      }
      else if (evt is UpdateAssetEvent updateAssetEvent)
      {
        asset.AssetUID = updateAssetEvent.AssetUID.ToString();
        asset.Name = updateAssetEvent.AssetName;
        asset.LegacyAssetID = updateAssetEvent.LegacyAssetId ?? -1;
        asset.Model = updateAssetEvent.Model;
        asset.ModelYear = updateAssetEvent.ModelYear;
        asset.AssetType = string.IsNullOrEmpty(updateAssetEvent.AssetType) ? null : updateAssetEvent.AssetType;
        asset.IconKey = updateAssetEvent.IconKey;
        asset.OwningCustomerUID = updateAssetEvent.OwningCustomerUID.HasValue && updateAssetEvent.OwningCustomerUID.Value != Guid.Empty
          ? updateAssetEvent.OwningCustomerUID.ToString()
          : null;
        asset.EquipmentVIN = updateAssetEvent.EquipmentVIN;
        asset.IsDeleted = false;
        asset.LastActionedUtc = updateAssetEvent.ActionUTC;
        eventType = "UpdateAssetEvent";
      }
      else if (evt is DeleteAssetEvent deleteAssetEvent)
      {
        asset.AssetUID = deleteAssetEvent.AssetUID.ToString();
        asset.IsDeleted = true;
        asset.LastActionedUtc = deleteAssetEvent.ActionUTC;
        eventType = "DeleteAssetEvent";
      }

      return await UpsertAssetDetail(asset, eventType);
    }

    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    ///     on deletion, a flag will be set.
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertAssetDetail(Asset asset, string eventType)
    {
      Log.LogDebug("AssetRepository: Upserting eventType{0} assetUid={1}", eventType, asset.AssetUID);
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<Asset>(@"SELECT 
                              AssetUID, Name, LegacyAssetID, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted, 
                              LastActionedUTC AS LastActionedUtc
                            FROM Asset
                            WHERE AssetUID = @AssetUID", new { asset.AssetUID })).FirstOrDefault();

      if (existing == null || existing.IsDeleted == false)
      {
        if (eventType == "CreateAssetEvent")
          upsertedCount = await CreateAsset(asset, existing);

        if (eventType == "UpdateAssetEvent")
          upsertedCount = await UpdateAsset(asset, existing);

        if (eventType == "DeleteAssetEvent")
          upsertedCount = await DeleteAsset(asset, existing);
      }

      Log.LogDebug("AssetRepository: upserted {0} rows", upsertedCount);
      Log.LogInformation("Event stored SUCCESS: {0}, {1}", eventType, JsonConvert.SerializeObject(asset));
      return upsertedCount;
    }

    private async Task<int> CreateAsset(Asset asset, Asset existing)
    {
      if (existing == null)
      {
        asset.AssetType = asset.AssetType ?? "Unassigned";
        const string upsert =
          @"INSERT Asset
                    (AssetUID, Name, LegacyAssetID, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted, LastActionedUTC )
                  VALUES
                   (@AssetUID, @Name, @LegacyAssetID, @SerialNumber, @MakeCode, @Model, @ModelYear, @AssetType, @IconKey, @OwningCustomerUID, @EquipmentVIN, @IsDeleted, @LastActionedUtc)";
        return await ExecuteWithAsyncPolicy(upsert, asset);
      }

      if (existing.LastActionedUtc == null
          // was generated by one of the internal updates e.g. LastUpdatedTime
          || asset.LastActionedUtc >= existing.LastActionedUtc) // potential intentional reprocessing
      {
        asset.Name = asset.Name ?? existing.Name;
        asset.LegacyAssetID = asset.LegacyAssetID == -1 ? existing.LegacyAssetID : asset.LegacyAssetID;
        asset.Model = asset.Model ?? existing.Model;
        asset.ModelYear = asset.ModelYear ?? existing.ModelYear;
        asset.AssetType = asset.AssetType ?? existing.AssetType;
        asset.IconKey = asset.IconKey ?? existing.IconKey;
        asset.OwningCustomerUID = asset.OwningCustomerUID ?? existing.OwningCustomerUID;
        asset.EquipmentVIN = asset.EquipmentVIN ?? existing.EquipmentVIN;

        const string update =
          @"UPDATE Asset                
                    SET Name = @Name,
                        LegacyAssetID = @LegacyAssetID,
                        SerialNumber = @SerialNumber,
                        MakeCode = @MakeCode,
                        Model = @Model,
                        ModelYear = @ModelYear,
                        AssetType = @AssetType,
                        IconKey = @IconKey,      
                        OwningCustomerUID = @OwningCustomerUID,
                        EquipmentVIN = @EquipmentVIN,      
                        LastActionedUTC = @LastActionedUtc
                  WHERE AssetUID = @AssetUID";
        return await ExecuteWithAsyncPolicy(update, asset);
      }

      if (asset.LastActionedUtc < existing.LastActionedUtc) // Create received after Update
      {
        const string update =
          @"UPDATE Asset                
                  SET MakeCode = @MakeCode,
                    SerialNumber = @SerialNumber
                  WHERE AssetUID = @AssetUID";
        return await ExecuteWithAsyncPolicy(update, asset);
      }

      return await Task.FromResult(0);
    }

    private async Task<int> DeleteAsset(Asset asset, Asset existing)
    {
      if (existing != null)
      {
        if (asset.LastActionedUtc >= existing.LastActionedUtc)
        {
          const string update =
            @"UPDATE Asset                
                          SET IsDeleted = 1,
                            LastActionedUTC = @LastActionedUtc
                          WHERE AssetUID = @AssetUID";
          return await ExecuteWithAsyncPolicy(update, asset);
        }

        Log.LogDebug(
          "AssetRepository: old delete event ignored currentActionedUTC{0} newActionedUTC{1}",
          existing.LastActionedUtc, asset.LastActionedUtc);
      }
      else
      {
        Log.LogDebug("AssetRepository: Inserted a DeleteAssetEvent as none existed. newActionedUTC{0}",
          asset.LastActionedUtc);

        var upsert = string.Format(
          "INSERT Asset " +
          "    (AssetUID, IsDeleted, LastActionedUTC, AssetType) " +
          "  VALUES " +
          "   (@AssetUID, @IsDeleted, @LastActionedUtc, \"Unassigned\")");
        return await ExecuteWithAsyncPolicy(upsert, asset);
      }

      return await Task.FromResult(0);
    }

    private async Task<int> UpdateAsset(Asset asset, Asset existing)
    {
      if (existing != null)
      {
        if (asset.LastActionedUtc >= existing.LastActionedUtc)
        {
          asset.Name = asset.Name ?? existing.Name;
          asset.LegacyAssetID = asset.LegacyAssetID == -1 ? existing.LegacyAssetID : asset.LegacyAssetID;
          asset.Model = asset.Model ?? existing.Model;
          asset.ModelYear = asset.ModelYear ?? existing.ModelYear;
          asset.AssetType = asset.AssetType ?? existing.AssetType;
          asset.IconKey = asset.IconKey ?? existing.IconKey;
          asset.OwningCustomerUID = asset.OwningCustomerUID ?? existing.OwningCustomerUID;
          asset.EquipmentVIN = asset.EquipmentVIN ?? existing.EquipmentVIN;

          const string update =
            @"UPDATE Asset                
                            SET Name = @Name,
                              LegacyAssetId = @LegacyAssetID,
                              Model = @Model,                      
                              ModelYear = @ModelYear, 
                              AssetType = @AssetType,
                              IconKey = @IconKey,
                              OwningCustomerUID = @OwningCustomerUID,
                              EquipmentVIN = @EquipmentVIN,
                              LastActionedUTC = @LastActionedUtc
                            WHERE AssetUID = @AssetUID";
          return await ExecuteWithAsyncPolicy(update, asset);
        }

        Log.LogWarning(
          "AssetRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
          existing.LastActionedUtc, asset.LastActionedUtc);
      }
      else
      {
        Log.LogDebug("AssetRepository: Inserted an UpdateAssetEvent as none existed.  newActionedUTC{0}",
          asset.LastActionedUtc);

        asset.AssetType = asset.AssetType ?? "Unassigned";
        const string upsert =
          @"INSERT Asset
                    (AssetUID, Name, LegacyAssetId, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted, LastActionedUTC )
                  VALUES
                    (@AssetUID, @Name, @LegacyAssetID, @Model, @ModelYear, @AssetType, @IconKey, @OwningCustomerUID, @EquipmentVIN, @IsDeleted, @LastActionedUtc)";
        return await ExecuteWithAsyncPolicy(upsert, asset);
      }

      return await Task.FromResult(0);
    }

    #endregion store

    #region getters

    public async Task<Asset> GetAsset(string assetUid)
    {
      return (await QueryWithAsyncPolicy<Asset>(@"SELECT 
                        AssetUID AS AssetUid, Name, LegacyAssetId, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted,
                        LastActionedUTC AS LastActionedUtc
                      FROM Asset
                      WHERE AssetUID = @AssetUID 
                        AND IsDeleted = 0", new { AssetUID = assetUid })).FirstOrDefault();
    }

    public async Task<Asset> GetAsset(long legacyAssetId)
    {
      return (await QueryWithAsyncPolicy<Asset>(@"SELECT 
                        AssetUID AS AssetUid, Name, LegacyAssetId, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted,
                        LastActionedUTC AS LastActionedUtc
                      FROM Asset
                      WHERE LegacyAssetId = @LegacyAssetID 
                        AND IsDeleted = 0"
        , new { LegacyAssetID = legacyAssetId }
      )).FirstOrDefault();
    }

    public async Task<IEnumerable<Asset>> GetAssets()
    {
      return (await QueryWithAsyncPolicy<Asset>
      (@"SELECT 
                        AssetUID AS AssetUid, Name, LegacyAssetId, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted,
                        LastActionedUTC AS LastActionedUtc
                      FROM Asset
                      WHERE IsDeleted = 0"
      )).ToList();
    }


    public async Task<IEnumerable<Asset>> GetAssets(IEnumerable<Guid> assetUids)
    {
      var assetsArray = assetUids.ToArray();
      return (await QueryWithAsyncPolicy<Asset>
      (@"SELECT 
                        AssetUID AS AssetUid, Name, LegacyAssetId, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted,
                        LastActionedUTC AS LastActionedUtc
                      FROM Asset
                      WHERE IsDeleted = 0 AND AssetUID IN @assets"
      , new { assets = assetsArray })).ToList();
    }



    public async Task<MatchingAssets> GetMatching3D2DAssets(MatchingAssets asset)
    {
      MatchingAssets result = null;
      if (Guid.TryParse(asset.AssetUID3D, out _))
      {
        result =
          (await QueryWithAsyncPolicy<MatchingAssets>
            //3d to 2d
            (@"Select
                a2.AssetUID AssetUID2D
                ,a.AssetUID AssetUID3D
                ,a.Name
                ,a2.SerialNumber SerialNumber2D
                ,a2.MakeCode MakeCode2D
                ,a.Model
                ,c.Name CustomerName
                ,a.SerialNumber SerialNumber3D
                ,a.MakeCode MakeCode3D
                from
	                Asset a
                    inner join AssetSubscription asu on asu.fk_AssetUID=a.AssetUID and asu.fk_AssetUID = @asset
	                  inner join Subscription sp on asu.fk_SubscriptionUID = sp.SubscriptionUID and sp.fk_ServiceTypeID = 13 and sp.EndDate  >= Utc_Timestamp()
                    inner join Customer c on c.CustomerUID = sp.fk_CustomerUID
                    left join Asset a2 on a2.SerialNumber = left(a.SerialNumber,locate('-',concat(replace(a.SerialNumber,' ','-'),'-'))-1)
                where
	                a.SerialNumber <> a2.SerialNumber and a.AssetUID <> a2.AssetUID"
              , new { asset = asset.AssetUID3D })).FirstOrDefault();
      }

      if (Guid.TryParse(asset.AssetUID2D, out _))
      {
        result =
          (await QueryWithAsyncPolicy<MatchingAssets>
            //2d to 3d
            (@" Select
                a.AssetUID AssetUID2D
                ,a2.AssetUID AssetUID3D
                ,a.Name
                ,a.SerialNumber SerialNumber2D
                ,a.MakeCode MakeCode2D
                ,a.Model
                ,c.Name CustomerName
                ,a2.SerialNumber SerialNumber3D
                ,a2.MakeCode MakeCode3D
            from
	            Asset a
                inner join AssetSubscription asu on asu.fk_AssetUID=a.AssetUID and asu.fk_AssetUID=@asset
	              inner join Subscription sp on asu.fk_SubscriptionUID = sp.SubscriptionUID and sp.fk_ServiceTypeID = 1
                inner join Customer c on c.CustomerUID = sp.fk_CustomerUID
                left join Asset a2 on a.SerialNumber = left(a2.SerialNumber,locate('-',concat(replace(a2.SerialNumber,' ','-'),'-'))-1)
            where
	            a.SerialNumber <> a2.SerialNumber and a.AssetUID <> a2.AssetUID"
              , new { asset = asset.AssetUID2D })).FirstOrDefault();
      }

      if (result == null) return asset;
      return result;
    }

    public async Task<IEnumerable<Asset>> GetAssets(IEnumerable<long> assetIds)
    {
      var assetsArray = assetIds.ToArray();
      return (await QueryWithAsyncPolicy<Asset>
      (@"SELECT 
                        AssetUID AS AssetUid, Name, LegacyAssetId, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted,
                        LastActionedUTC AS LastActionedUtc
                      FROM Asset
                      WHERE IsDeleted = 0 AND LegacyAssetId IN @assets"
      , new { assets = assetsArray })).ToList();
    }

    /// <summary>
    ///     Used for unit tests so we can test deleted assets
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Asset>> GetAllAssetsInternal()
    {
      return (await QueryWithAsyncPolicy<Asset>
      (@"SELECT 
                        AssetUID AS AssetUid, Name, LegacyAssetId, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted,
                        LastActionedUTC AS LastActionedUtc
                      FROM Asset"
      )).ToList();
    }

    public async Task<IEnumerable<Asset>> GetAssets(string[] productFamily)
    {
      return (await QueryWithAsyncPolicy<Asset>
      (@"SELECT 
                        AssetUID AS AssetUid, Name, LegacyAssetId, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted,
                        LastActionedUTC AS LastActionedUtc
                      FROM Asset 
                      WHERE AssetType IN @families
                        AND IsDeleted = 0", new { families = productFamily })).ToList();
    }

    #endregion getters
  }
}

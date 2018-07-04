using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories
{
  public class AssetRepository : RepositoryBase, IRepository<IAssetEvent>, IAssetRepository
  {
        public AssetRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(_connectionString,
            logger)
        {
            log = logger.CreateLogger<AssetRepository>();
        }

        #region store

        public async Task<int> StoreEvent(IAssetEvent evt)
        {
            var upsertedCount = 0;
            var asset = new Asset();
            var eventType = "Unknown";
          if (evt == null)
          {
            log.LogWarning($"Unsupported event type");
            return 0;
          }

      log.LogDebug($"Event type is {evt.GetType().ToString()}");
      if (evt is CreateAssetEvent)
            {
                var assetEvent = (CreateAssetEvent) evt;
                asset.Name = assetEvent.AssetName;
                asset.AssetType = string.IsNullOrEmpty(assetEvent.AssetType) ? null : assetEvent.AssetType;
                asset.AssetUID = assetEvent.AssetUID.ToString();
                asset.EquipmentVIN = assetEvent.EquipmentVIN;
                asset.LegacyAssetID = assetEvent.LegacyAssetId;
                asset.SerialNumber = assetEvent.SerialNumber;
                asset.MakeCode = assetEvent.MakeCode;
                asset.Model = assetEvent.Model;
                asset.ModelYear = assetEvent.ModelYear;
                asset.IconKey = assetEvent.IconKey;
                asset.OwningCustomerUID = assetEvent.OwningCustomerUID.HasValue
                    ? assetEvent.OwningCustomerUID.ToString()
                    : null;
                asset.IsDeleted = false;
                asset.LastActionedUtc = assetEvent.ActionUTC;
                eventType = "CreateAssetEvent";
            }
            else if (evt is UpdateAssetEvent)
            {
                var assetEvent = (UpdateAssetEvent) evt;
                asset.AssetUID = assetEvent.AssetUID.ToString();
                asset.Name = assetEvent.AssetName;
                asset.LegacyAssetID = assetEvent.LegacyAssetId.HasValue ? assetEvent.LegacyAssetId.Value : -1;
                asset.Model = assetEvent.Model;
                asset.ModelYear = assetEvent.ModelYear;
                asset.AssetType = string.IsNullOrEmpty(assetEvent.AssetType) ? null : assetEvent.AssetType;
                asset.IconKey = assetEvent.IconKey;
                asset.OwningCustomerUID = assetEvent.OwningCustomerUID.HasValue
                    ? assetEvent.OwningCustomerUID.ToString()
                    : null;
                asset.EquipmentVIN = assetEvent.EquipmentVIN;
                asset.IsDeleted = false;
                asset.LastActionedUtc = assetEvent.ActionUTC;
                eventType = "UpdateAssetEvent";
            }
            else if (evt is DeleteAssetEvent)
            {
                var assetEvent = (DeleteAssetEvent) evt;
                asset.AssetUID = assetEvent.AssetUID.ToString();
                asset.IsDeleted = true;
                asset.LastActionedUtc = assetEvent.ActionUTC;
                eventType = "DeleteAssetEvent";
            }

            upsertedCount = await UpsertAssetDetail(asset, eventType);
            return upsertedCount;
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
            log.LogDebug("AssetRepository: Upserting eventType{0} assetUid={1}", eventType, asset.AssetUID);
            var upsertedCount = 0;

            var existing = (await QueryWithAsyncPolicy<Asset>(@"SELECT 
                              AssetUID, Name, LegacyAssetID, SerialNumber, MakeCode, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted, 
                              LastActionedUTC AS LastActionedUtc
                            FROM Asset
                            WHERE AssetUID = @AssetUID", new { AssetUID = asset.AssetUID})).FirstOrDefault();

            if (existing == null || existing.IsDeleted == false)
            {
                if (eventType == "CreateAssetEvent")
                    upsertedCount = await CreateAsset(asset, existing);

                if (eventType == "UpdateAssetEvent")
                    upsertedCount = await UpdateAsset(asset, existing);

                if (eventType == "DeleteAssetEvent")
                    upsertedCount = await DeleteAsset(asset, existing);
            }
            log.LogDebug("AssetRepository: upserted {0} rows", upsertedCount);
            log.LogInformation("Event stored SUCCESS: {0}, {1}", eventType, JsonConvert.SerializeObject(asset));
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
                   (@AssetUid, @Name, @LegacyAssetID, @SerialNumber, @MakeCode, @Model, @ModelYear, @AssetType, @IconKey, @OwningCustomerUID, @EquipmentVIN, @IsDeleted, @LastActionedUtc)";
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
                WHERE AssetUID = @AssetUid";
                return await ExecuteWithAsyncPolicy(update, asset);
            }
            if (asset.LastActionedUtc < existing.LastActionedUtc) // Create received after Update
            {
                const string update =
                    @"UPDATE Asset                
                  SET MakeCode = @MakeCode,
                    SerialNumber = @SerialNumber
                  WHERE AssetUID = @AssetUid";
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
                    WHERE AssetUID = @AssetUid";
                    return await ExecuteWithAsyncPolicy(update, asset);
                }
                log.LogDebug(
                    "AssetRepository: old delete event ignored currentActionedUTC{0} newActionedUTC{1}",
                    existing.LastActionedUtc, asset.LastActionedUtc);
            }
            else
            {
                log.LogDebug("AssetRepository: Inserted a DeleteAssetEvent as none existed. newActionedUTC{0}",
                    asset.LastActionedUtc);

                var upsert = string.Format(
                    "INSERT Asset " +
                    "    (AssetUID, IsDeleted, LastActionedUTC, AssetType) " +
                    "  VALUES " +
                    "   (@AssetUid, @IsDeleted, @LastActionedUtc, \"Unassigned\")");
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
                      LegacyAssetId = @LegacyAssetId,
                      Model = @Model,                      
                      ModelYear = @ModelYear, 
                      AssetType = @AssetType,
                      IconKey = @IconKey,
                      OwningCustomerUID = @OwningCustomerUID,
                      EquipmentVIN = @EquipmentVIN,
                      LastActionedUTC = @LastActionedUtc
                    WHERE AssetUID = @AssetUid";
                    return await ExecuteWithAsyncPolicy(update, asset);
                }
                log.LogWarning(
                    "AssetRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
                    existing.LastActionedUtc, asset.LastActionedUtc);
            }
            else
            {
                log.LogDebug("AssetRepository: Inserted an UpdateAssetEvent as none existed.  newActionedUTC{0}",
                    asset.LastActionedUtc);

                asset.AssetType = asset.AssetType ?? "Unassigned";
                const string upsert =
                    @"INSERT Asset
                    (AssetUID, Name, LegacyAssetId, Model, ModelYear, AssetType, IconKey, OwningCustomerUID, EquipmentVIN, IsDeleted, LastActionedUTC )
                  VALUES
                    (@AssetUid, @Name, @LegacyAssetId, @Model, @ModelYear, @AssetType, @IconKey, @OwningCustomerUID, @EquipmentVIN, @IsDeleted, @LastActionedUtc)";
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
                        AND IsDeleted = 0", new {families = productFamily})).ToList();
        }

        #endregion getters
    }
}

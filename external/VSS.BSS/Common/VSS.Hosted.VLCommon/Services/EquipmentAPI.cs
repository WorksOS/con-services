using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core;
using System.Device.Location;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Hosted.VLCommon.Resources;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM;

namespace VSS.Hosted.VLCommon
{
    public class EquipmentAPI : IEquipmentAPI
    {
        private readonly IAssetService _assetService = null;
        private readonly bool EnableNextGenAssetSync;

        internal EquipmentAPI(IAssetService assetService, bool enableSync)
        {
            _assetService = assetService;
            EnableNextGenAssetSync = enableSync;
        }

        private readonly IWorkDefinitionService _workDefinitionService = null;
        private readonly bool EnableNextGenWorkDefinitionSync;

        internal EquipmentAPI(IWorkDefinitionService workDefinitionService, bool enableSync)
        {
            _workDefinitionService = workDefinitionService;
            EnableNextGenWorkDefinitionSync = enableSync;
        }

        private readonly IDeviceService _deviceService = null;
        private readonly bool EnableNextGenDeviceSync;

        internal EquipmentAPI(IDeviceService deviceService, bool enableSync)
        {
            _deviceService = deviceService;
            EnableNextGenDeviceSync = enableSync;
        }

        public EquipmentAPI()
        {
            _assetService = API.AssetService;
            _workDefinitionService = API.WorkDefinitionService;
            _deviceService = API.DeviceService;
            EnableNextGenAssetSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.AssetAPI.EnableSync"]);
            EnableNextGenWorkDefinitionSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.WorkDefinitionAPI.EnableSync"]);
            EnableNextGenDeviceSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.DeviceAPI.EnableSync"]);
        }

        private static readonly ILog log =
          LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

        public Asset Create(INH_OP opContext, string assetName, string makeCode, string serialNumber, long deviceID,
          DeviceTypeEnum deviceType, string familyDesc, string modelDesc, int year, Guid assetGuid, string assetVinSN = null,
          long storeId = 1)
        {
            long newAssetID = Asset.ComputeAssetID(makeCode, serialNumber);

            var newAsset = new Asset
            {
                AssetUID = assetGuid,
                AssetID = newAssetID,
                Name = assetName,
                SerialNumberVIN = serialNumber,
                fk_MakeCode = makeCode,
                fk_DeviceID = deviceID,
                ProductFamilyName = familyDesc,
                Model = modelDesc,
                InsertUTC = DateTime.UtcNow,
                UpdateUTC = DateTime.UtcNow,
                fk_StoreID = storeId
            };

            if (!string.IsNullOrWhiteSpace(assetVinSN))
                newAsset.EquipmentVIN = assetVinSN;

            if (year > 0)
                newAsset.ManufactureYear = year;
            new OemAssetInformationStrategy(opContext).UpdateAsset(newAsset);

            opContext.Asset.AddObject(newAsset);
            var result = opContext.SaveChanges();
            if (result <= 0)
            {
                throw new InvalidOperationException("Error creating asset");
            }
            if (EnableNextGenAssetSync)
            {
                //Make a call to Next Gen Asset Service
                var assetDetails = new
                {
                    AssetName = newAsset.Name,
                    SerialNumber = serialNumber,
                    MakeCode = makeCode,
                    Model = newAsset.Model,
                    AssetType = newAsset.ProductFamilyName,
                    EquipmentVIN = newAsset.EquipmentVIN,
                    ModelYear = newAsset.ManufactureYear,
                    AssetUID = assetGuid,
                    ActionUTC = DateTime.UtcNow,
                    IconKey = newAsset.IconID,
                };

                var success = _assetService.CreateAsset(assetDetails);
                if (!success)
                {
                    log.IfInfoFormat("Error occurred while creating Asset in VSP stack. Serial Number :{0}", newAsset.SerialNumberVIN);
                }
            }

            if (EnableNextGenDeviceSync && deviceID != 0)
            {
                Guid deviceUID = (from device in opContext.DeviceReadOnly
                                  where device.ID == deviceID
                                  select device.DeviceUID ?? Guid.NewGuid()).FirstOrDefault();

                AssociateDeviceAssetEvent associateEvent = new AssociateDeviceAssetEvent()
                {
                    DeviceUID = deviceUID,
                    AssetUID = assetGuid,
                    ActionUTC = DateTime.UtcNow
                };

                var associateSuccess = API.DeviceService.AssociateDeviceAsset(associateEvent);
                if (!associateSuccess)
                {
                    log.IfInfoFormat("Error occurred while associate device in VSP stack. DeviceId :{0}, AssetUId :{1}",
                      deviceID, assetGuid);
                }
            }
            CreateDefaultAssetUtilizationSettings(opContext, newAsset.AssetID, deviceType);
            API.PMDuePopulator.PopulatePMDue(opContext, newAsset.AssetID);
            CreateDefaultAssetMonitoringSettings(opContext, newAsset.AssetID);

            return newAsset;
        }

        public bool Update(INH_OP opContext, long assetID, List<Param> updatedFields)
        {
            Asset asset = (from a in opContext.Asset where a.AssetID == assetID select a).SingleOrDefault();
            return asset != null && UpdateAsset(opContext, updatedFields, asset);
        }

        private bool UpdateAsset(INH_OP opContext, List<Param> updatedFields, Asset asset)
        {
            UpdateModelAndProductFamily(opContext, ref updatedFields, asset);

            var deviceId = asset.fk_DeviceID;
            var updatedEntity = API.Update<Asset>(opContext, asset, updatedFields);
            if (updatedEntity != null && EnableNextGenAssetSync)
            {
                SyncUpdateEventWithNextGen(updatedFields, asset);
            }
            if (updatedEntity != null && EnableNextGenDeviceSync && deviceId != asset.fk_DeviceID)
            {
                if (deviceId != 0)
                {
                    DisassociateAssetDevice(opContext, asset.AssetID, deviceId);

                }
                else if (asset.fk_DeviceID != 0)
                {
                    AssociateAssetDevice(opContext, asset.AssetID, asset.fk_DeviceID);
                }
            }
            return (updatedEntity != null);
        }

        private void SyncUpdateEventWithNextGen(List<Param> updatedFields, Asset asset)
        {
            var nextGenUpdateFields = new List<string>
		      {
                "Name",
                "ProductFamilyName",
                "ManufactureYear",
                "Model",
                "EquipmentVIN",
                "UpdateUTC"
              };

            //Make a call to Next Gen Asset Service
            var updatedEntries = updatedFields.Where(x => nextGenUpdateFields.Contains(x.Name))
                .ToDictionary(y => y.Name, y => y.Value);

            updatedEntries.Add("AssetUID", asset.AssetUID);

            if (updatedEntries.ContainsKey("Name"))
            {
                var value = updatedEntries["Name"];
                updatedEntries.Remove("Name");
                updatedEntries.Add("AssetName", value);
            }

            if (updatedEntries.ContainsKey("ProductFamilyName"))
            {
                var value = updatedEntries["ProductFamilyName"];
                updatedEntries.Remove("ProductFamilyName");
                updatedEntries.Add("AssetType", value);
            }

            if (updatedEntries.ContainsKey("ManufactureYear"))
            {
                var value = updatedEntries["ManufactureYear"];
                updatedEntries.Remove("ManufactureYear");
                updatedEntries.Add("ModelYear", value);
            }

            if (updatedEntries.ContainsKey("UpdateUTC"))
            {
                var value = updatedEntries["UpdateUTC"];
                updatedEntries.Remove("UpdateUTC");
                updatedEntries.Add("ActionUTC", value);
            }


            var result = _assetService.UpdateAsset(updatedEntries);
            if (!result)
            {
                log.IfInfoFormat("Error occurred while updating Asset in VSP stack. Serial Number :{0}",
                  asset.SerialNumberVIN);
            }
        }

        public bool UpdateByAssetUid(INH_OP opContext, Guid assetUid, List<Param> updatedFields)
        {
            Asset asset = (from a in opContext.Asset where a.AssetUID == assetUid select a).SingleOrDefault();
            return asset != null && UpdateAsset(opContext, updatedFields, asset);
        }

        public bool UpdateLeaseOwner(long[] assetIDs, long? leaseOwnerCustomerID, long? userID)
        {
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                int count = 0;
                var assetDetail = (from asst in ctx.Asset
                                   where assetIDs.Contains(asst.AssetID)
                                   select asst);
                foreach (var asst in assetDetail)
                {
                    asst.UpdateUTC = DateTime.UtcNow;
                    asst.ifk_LeaseOwnerCustomerID = leaseOwnerCustomerID == 0
                      ? (asst.ifk_LeaseOwnerCustomerID != null) ? leaseOwnerCustomerID : null
                      : leaseOwnerCustomerID;
                }
                foreach (long asset in assetIDs)
                {
                    if (asset == 0)
                        continue;
                    else
                    {
                        var assetDeviceInfo = (from asst in ctx.AssetReadOnly
                                               join device in ctx.DeviceReadOnly on asst.fk_DeviceID equals device.ID
                                               where asst.AssetID == asset
                                               select new
                                               {
                                                   make = asst.fk_MakeCode,
                                                   serialNum = asst.SerialNumberVIN,
                                                   deviceTypeID = device.fk_DeviceTypeID,
                                                   eventID = leaseOwnerCustomerID == 0 ? (asst.ifk_LeaseOwnerCustomerID != null) ? 5 : -1 : 4
                                               }).FirstOrDefault();
                        if (assetDeviceInfo.eventID != -1)
                        {
                            AuditConfigChanges(ctx, assetDeviceInfo.make, assetDeviceInfo.serialNum, assetDeviceInfo.eventID, userID,
                              assetDeviceInfo.deviceTypeID);
                            count++;
                        }
                    }
                }
                int result = ctx.SaveChanges();
                if (result <= 0)
                {
                    log.IfErrorFormat("Setting of LeaseOwnership succeeded for {0} assets out of {1} ", count, assetIDs.Count());
                    throw new InvalidOperationException("Failed to save AssetSecurityStatus History");
                }
                return true;
            }
        }

        public bool UpdateWorkingDefinition(INH_OP opContext, long assetID, WorkDefinitionEnum workDefn, int sensorNumber,
          bool sensorStartIsOn)
        {
            if (assetID == 0)
                throw new InvalidOperationException("Please specify an Asset to Update");

            AssetWorkingDefinition assetWorkingDefinition = (from awd in opContext.AssetWorkingDefinition
                                                             where awd.fk_AssetID == assetID
                                                             select awd).FirstOrDefault();

            Guid assetGuid = (from asset in opContext.Asset
                              where asset.AssetID == assetID
                              select asset.AssetUID ?? new Guid()).FirstOrDefault();

            assetWorkingDefinition.fk_WorkDefinitionID = (int)workDefn;
            assetWorkingDefinition.SensorNumber = sensorNumber;
            assetWorkingDefinition.SensorStartIsOn = sensorStartIsOn;
            assetWorkingDefinition.UpdateUTC = DateTime.UtcNow;

            string workDefinitionType = (from assetworkingdefinition in opContext.WorkDefinition
                                         where assetworkingdefinition.ID == assetWorkingDefinition.fk_WorkDefinitionID
                                         select assetworkingdefinition.Description).FirstOrDefault();

            bool success = opContext.SaveChanges() > 0;

            if (success != true)
                throw new InvalidOperationException("Unable to update working definition for asset");

            if (success && EnableNextGenWorkDefinitionSync)
            {
                if (workDefinitionType == "Sensor Events" || workDefinitionType == "Movement and Sensor Events")
                {
                    var workDefinitionDetails = new
                    {
                        AssetUID = assetGuid,
                        ActionUTC = DateTime.UtcNow,
                        WorkDefinitionType = workDefinitionType,
                        SensorNumber = sensorNumber,
                        StartIsOn = sensorStartIsOn
                    };
                    var result = _workDefinitionService.UpdateWorkDefinition(workDefinitionDetails);
                    if (!result)
                    {
                        log.IfInfoFormat("Error occurred while updating default working definition for asset in VSP stack. Asset UID :{0}", assetGuid);
                    }
                }
                else
                {
                    var workDefinitionDetails = new
                    {
                        AssetUID = assetGuid,
                        ActionUTC = DateTime.UtcNow,
                        WorkDefinitionType = workDefinitionType
                    };
                    var result = _workDefinitionService.UpdateWorkDefinition(workDefinitionDetails);
                    if (!result)
                    {
                        log.IfInfoFormat("Error occurred while updating working definition for asset in VSP stack. Asset UID :{0}", assetGuid);
                    }
                }
            }

            return success;
        }

        public bool UpdateFuelBurnRate(INH_OP opCtx, long assetID, double idleBurnRate, double workingBurnRate)
        {
            if (assetID == 0)
            {
                throw new InvalidOperationException("Please specify an Asset to Update");
            }

            Device device = (from d in opCtx.DeviceReadOnly
                             join a in opCtx.AssetReadOnly on d.ID equals a.fk_DeviceID
                             where a.AssetID == assetID
                             select d).FirstOrDefault();
            if (device == null) // Do not update the  burn rates if no device is associated with the Asset
            {
                throw new InvalidOperationException(
                  "Unable to update expected runtime hours for asset as device type is indeterminate.");
            }
            int deviceTypeID = device.fk_DeviceTypeID;
            bool success = false;

            AssetBurnRates updatedAssetBurnRates = (from abr in opCtx.AssetBurnRates
                                                    where abr.fk_AssetID == assetID
                                                    select abr).FirstOrDefault();

            if (deviceTypeID != (int)DeviceTypeEnum.PL321) // PL321 does not use estimated burn rates
            {
                updatedAssetBurnRates.EstimatedIdleBurnRateGallonsPerHour = (!double.IsNaN(idleBurnRate)
                  ? (double?)idleBurnRate
                  : null);
                updatedAssetBurnRates.EstimatedWorkingBurnRateGallonsPerHour = (!double.IsNaN(workingBurnRate)
                  ? (double?)workingBurnRate
                  : null);
                updatedAssetBurnRates.UpdateUTC = DateTime.UtcNow;
                success = opCtx.SaveChanges() > 0;
            }

            if (success != true)
            {
                throw new InvalidOperationException("Unable to update Fuel Burn Rates for asset");
            }

            return success;
        }

        private void RefillNetworkDealerCustomerCode(INH_OP ctx, AssetAlias alias)
        {
            var result = (from cr in ctx.CustomerRelationshipReadOnly
                          join cust in ctx.CustomerReadOnly on cr.fk_ParentCustomerID equals cust.ID
                          where cr.fk_ClientCustomerID == alias.fk_CustomerID
                          select new
                          {
                              customerID = cust.ID,
                              customerTypeID = cust.fk_CustomerTypeID,
                              custNetworkDealerCode = cust.NetworkDealerCode,
                              custNetworkCustCode = cust.NetworkCustomerCode
                          }).ToList();
            alias.NetworkDealerCode =
              result.Where(t => t.customerTypeID == (int)CustomerTypeEnum.Dealer)
                .Select(t => t.custNetworkDealerCode)
                .FirstOrDefault() ?? alias.NetworkDealerCode;
            alias.NetworkCustomerCode =
              result.Where(t => t.customerTypeID == (int)CustomerTypeEnum.Customer)
                .Select(t => t.custNetworkCustCode)
                .FirstOrDefault() ?? alias.NetworkCustomerCode;
        }

        public void PopulateAssetAlias(INH_OP ctx, long assetID, string updatedAssetName, long? userID)
        {
            try
            {
                var alias = (from ar in ctx.AssetReadOnly
                             join d in ctx.DeviceReadOnly on ar.fk_DeviceID equals d.ID
                             join c in ctx.CustomerReadOnly on d.OwnerBSSID equals c.BSSID
                             where ar.AssetID == assetID
                             select new
                             {
                                 Name = updatedAssetName,
                                 OwnerBSSID = c.BSSID,
                                 fk_CustomerID = c.ID,
                                 fk_UserID = (long)userID,
                                 NetworkDealerCode = c.NetworkDealerCode,
                                 NetworkCustomerCode = c.NetworkCustomerCode,
                                 DealerAccountCode = c.DealerAccountCode,
                                 customerType = c.fk_CustomerTypeID,
                                 IBKey = d.IBKey
                             }).FirstOrDefault();
                AssetAlias assetAlias = new AssetAlias();
                assetAlias.Name = alias.Name;
                assetAlias.OwnerBSSID = alias.OwnerBSSID;
                assetAlias.fk_AssetID = assetID;
                assetAlias.fk_CustomerID = alias.fk_CustomerID;
                assetAlias.fk_UserID = alias.fk_UserID;
                assetAlias.NetworkCustomerCode = alias.NetworkCustomerCode;
                assetAlias.NetworkDealerCode = alias.NetworkDealerCode;
                assetAlias.DealerAccountCode = alias.DealerAccountCode;
                assetAlias.IBKey = alias.IBKey;
                assetAlias.InsertUTC = DateTime.UtcNow;
                if (alias.customerType == (int)CustomerTypeEnum.Account)
                {
                    RefillNetworkDealerCustomerCode(ctx, assetAlias);
                }
                if (assetAlias != null)
                {
                    ctx.AssetAlias.AddObject(assetAlias);
                    ctx.SaveChanges();
                }
            }
            catch (System.Data.OptimisticConcurrencyException)
            {
                throw new Exception(string.Format("Another user trying to update the same asset. Asset ID {0}", assetID),
                  new IntentionallyThrownException());
            }
        }

        /// <summary>
        /// Method for claiming the asset
        /// </summary>
        public bool ClaimAsset(INH_OP opContext, Guid assetUid, long storeID)
        {
            bool updated = false;
            //Do claim only when the asset is in release mode i.e fk_storeId is "0"
            Asset opAsset = ((from a in opContext.Asset where a.AssetUID == assetUid select a).SingleOrDefault());

            if (null != opAsset)
            {
                opAsset.fk_StoreID = storeID;
                opAsset.UpdateUTC = DateTime.UtcNow;
                updated = opContext.SaveChanges() > 0;
            }

            return updated;
        }

        public bool ReleaseAsset(INH_OP opContext, Guid assetUid)
        {
            bool updated = false;
            Asset opAsset = ((from a in opContext.Asset where a.AssetUID == assetUid select a).SingleOrDefault());

            if (null != opAsset)
            {
                // To release, set storeId as '0' indicating an unclaimed asset
                opAsset.fk_StoreID = 0;

                // State for device associated with an unclaimed asset (if any) ought to be "Provisioned"
                if (opAsset.fk_DeviceID > 0)
                    API.Device.UpdateDeviceState(opAsset.fk_DeviceID, DeviceStateEnum.Provisioned, opContext);

                opAsset.UpdateUTC = DateTime.UtcNow;
                updated = opContext.SaveChanges() > 0;
            }

            return updated;
        }


        public string BuildAssetName(INH_OP opContext, string nickname, string makeCode, string equipSerialNumberVin)
        {
            return !string.IsNullOrEmpty(nickname)
              ? MakeUniqueAssetName(opContext, nickname)
              : MakeUniqueAssetName(opContext, makeCode, equipSerialNumberVin);
        }

        public long GetOemPMSalesModelID(INH_OP opContext, long assetID)
        {
            long salesModelID = DEFAULT_SALES_MODEL_ID;

            var assetInfo = (from a in opContext.AssetReadOnly
                             where a.AssetID == assetID
                             select new { SerialNumber = a.SerialNumberVIN, MakeCode = a.fk_MakeCode, Model = a.Model }).FirstOrDefault();

            if (assetInfo != null)
            {
                string assetPrefix = string.Empty;
                int assetSuffix = 0;
                if (!string.IsNullOrEmpty(assetInfo.SerialNumber) && assetInfo.SerialNumber.Length >= 4)
                {
                    assetPrefix = assetInfo.SerialNumber.Substring(0, 3);
                    int.TryParse(assetInfo.SerialNumber.Substring(3), out assetSuffix);
                }

                salesModelID = (from sm in opContext.PMSalesModelReadOnly
                                where sm.ExternalID != null &&
                                      ((sm.SerialNumberPrefix == assetPrefix
                                        && assetSuffix >= sm.StartRange
                                        && assetSuffix <= sm.EndRange
                                        && assetInfo.MakeCode == sm.fk_MakeCode)
                                       || (sm.Model != null && sm.fk_MakeCode == assetInfo.MakeCode && sm.Model == assetInfo.Model)
                                        )
                                orderby sm.UpdateUTC descending
                                select sm.ID).FirstOrDefault();

                if (salesModelID == 0)
                    salesModelID = DEFAULT_SALES_MODEL_ID;
            }
            return salesModelID;
        }

        /* Series 522 */

        public string GetStartModeDescription(int startMode, string locale)
        {
            MachineStartStatus mode = (MachineStartStatus)startMode;
            switch (mode)
            {
                case MachineStartStatus.NoPending:
                    return VLResourceManager.GetString("NoPending", locale);
                case MachineStartStatus.NormalOperation:
                    return VLResourceManager.GetString("NormalOperation", locale);
                case MachineStartStatus.NormalOperationPending:
                    return VLResourceManager.GetString("NormalOperationPending", locale);
                case MachineStartStatus.Derated:
                    return VLResourceManager.GetString("Derated", locale);
                case MachineStartStatus.DeratedPending:
                    return VLResourceManager.GetString("DeratedPending", locale);
                case MachineStartStatus.Disabled:
                    return VLResourceManager.GetString("Disabled", locale);
                case MachineStartStatus.DisabledPending:
                    return VLResourceManager.GetString("DisabledPending", locale);
                default:
                    return mode.ToString();
            }
        }

        /* PL 421 */

        public string GetMachineStartModeDescription(int startMode, string locale)
        {
            MachineSecurityModeSetting mode = (MachineSecurityModeSetting)startMode;
            switch (mode)
            {
                case MachineSecurityModeSetting.NoPending:
                    return VLResourceManager.GetString("NoPending", locale);
                case MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureDisabled:
                    return VLResourceManager.GetString("NormalOperation", locale);
                case MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureEnabled:
                    return VLResourceManager.GetString("NormalOperation", locale);
                case MachineSecurityModeSetting.Derated:
                    return VLResourceManager.GetString("Derated", locale);
                case MachineSecurityModeSetting.Disabled:
                    return VLResourceManager.GetString("Disabled", locale);
                default:
                    return mode.ToString();
            }
        }

        private static void UpdateModelAndProductFamily(INH_OP opContext, ref List<Param> updatedFields, Asset asset)
        {
            // It is possible that we do not want to update model and product family fields.  For example, when a Caterpillar asset is modified,
            // and a BSS update record is sent, we do not want to update the asset's product family or model with the information from the BSS update record.
            Param productFamily =
              (from pf in
                   updatedFields.Where(x => x.Name.Equals("ProductFamilyName", StringComparison.CurrentCultureIgnoreCase))
               select pf).FirstOrDefault();
            Param model =
              (from pf in updatedFields.Where(x => x.Name.Equals("Model", StringComparison.CurrentCultureIgnoreCase))
               select pf).FirstOrDefault();
            if ((productFamily != null) || (model != null))
            {
                var strategy = new OemAssetInformationStrategy(opContext).GetStrategy(asset.fk_MakeCode);
                strategy.UpdateAsset(asset);
                if (productFamily != null && !(strategy is NullAssetInformationStrategy) && !strategy.UseStoreInformation)
                {
                    updatedFields.Remove(productFamily);
                    productFamily.Value = asset.ProductFamilyName;
                    updatedFields.Add(productFamily);
                }
                if (model != null && !(strategy is NullAssetInformationStrategy) && !strategy.UseStoreInformation)
                {
                    updatedFields.Remove(model);
                    model.Value = asset.Model;
                    updatedFields.Add(model);
                }
            }
        }

        private string MakeUniqueAssetName(INH_OP opContext, string make, string equipSerialNumberVin)
        {
            string name = string.Format("{0} {1}", make, equipSerialNumberVin);

            return MakeUniqueAssetName(opContext, name);
        }

        private string MakeUniqueAssetName(INH_OP opContext, string name)
        {
            string existingName = (from a in opContext.AssetReadOnly
                                   where a.Name == name
                                   select a.Name).FirstOrDefault<string>();

            int uniqueCount = 2;
            String newName = name;
            while (!String.IsNullOrEmpty(existingName))
            {
                newName = string.Format("{0} ({1})", name, uniqueCount);
                existingName = (from a in opContext.AssetReadOnly
                                where a.Name == newName
                                select a.Name).FirstOrDefault<string>();
                uniqueCount++;
            }
            return newName;
        }

        public bool AssociateAssetDevice(INH_OP opContext, long assetId, long deviceId)
        {
            bool isAssociated = false;
            var asset = (from a in opContext.Asset
                         where a.AssetID == assetId
                         select a
              ).FirstOrDefault();
            asset.fk_DeviceID = deviceId;
            asset.UpdateUTC = DateTime.UtcNow;
            var result = opContext.SaveChanges();

            if (result > 0)
            {
                isAssociated = true;
                if (EnableNextGenDeviceSync && deviceId != 0)
                {
                    var DeviceGuid = (from device in opContext.Device
                                      where device.ID == deviceId
                                      select device.DeviceUID).FirstOrDefault();

                    AssociateDeviceAssetEvent associateEvent = new AssociateDeviceAssetEvent();
                    if (DeviceGuid.HasValue)
                    {
                        associateEvent.DeviceUID = DeviceGuid ?? Guid.Empty;
                    }
                    if (asset.AssetUID.HasValue)
                    {
                        associateEvent.AssetUID = asset.AssetUID ?? Guid.Empty;
                    }
                    associateEvent.ActionUTC = DateTime.UtcNow;
                    var success = API.DeviceService.AssociateDeviceAsset(associateEvent);
                    if (!success)
                    {
                        log.IfInfoFormat("Error occurred while associate device in VSP stack. DeviceId :{0}, AssetId :{1}",
                          deviceId, assetId);
                    }
                }
            }
            return isAssociated;
        }

        public AssetDeviceHistory GetAssetDeviceHistory(INH_OP opCtx, long assetId)
        {
            var assetDeviceHistory = (from adh in opCtx.AssetDeviceHistoryReadOnly
                                      where adh.fk_AssetID == assetId
                                      orderby adh.StartUTC descending
                                      select adh).FirstOrDefault();
            return assetDeviceHistory;
        }

        public AssetDeviceHistory CreateAssetDeviceHistory(INH_OP opContext, long assetId, long deviceId, string ownerBssId,
          DateTime startUtc)
        {
            var existingHistory = GetAssetDeviceHistory(opContext, assetId);

            if (existingHistory != null && !existingHistory.EndUTC.HasValue)
                throw new InvalidOperationException("Asset Device History has a null Enddate");

            AssetDeviceHistory assetDeviceHistory = new AssetDeviceHistory
            {
                fk_AssetID = assetId,
                fk_DeviceID = deviceId,
                OwnerBSSID = ownerBssId,
                StartUTC = existingHistory != null ? existingHistory.EndUTC.Value : startUtc,
                EndUTC = DateTime.UtcNow
            };

            opContext.AssetDeviceHistory.AddObject(assetDeviceHistory);

            if (opContext.SaveChanges() > 0)
                return assetDeviceHistory;

            return null;
        }

        public bool DisassociateAssetDevice(INH_OP opContext, long assetId, long deviceId)
        {
            bool isDisassociated = false;
            var asset = (from a in opContext.Asset
                         where a.AssetID == assetId
                         select a
              ).FirstOrDefault();
            asset.fk_DeviceID = 0;
            asset.UpdateUTC = DateTime.UtcNow;
            var result = opContext.SaveChanges();

            if (result > 0)
            {
                isDisassociated = true;
                if (EnableNextGenDeviceSync)
                {
                    var DeviceGuid = (from device in opContext.Device
                                      where device.ID == deviceId
                                      select device.DeviceUID).FirstOrDefault();

                    DissociateDeviceAssetEvent dissociateEvent = new DissociateDeviceAssetEvent();
                    if (DeviceGuid.HasValue)
                    {
                        dissociateEvent.DeviceUID = DeviceGuid ?? Guid.Empty;
                    }
                    if (asset.AssetUID.HasValue)
                    {
                        dissociateEvent.AssetUID = asset.AssetUID ?? Guid.Empty;
                    }
                    dissociateEvent.ActionUTC = DateTime.UtcNow;
                    var success = API.DeviceService.DissociateDeviceAsset(dissociateEvent);
                    if (!success)
                    {
                        log.IfInfoFormat("Error occurred while dissociate device in VSP stack. DeviceId :{0}, AssetId :{1}",
                          deviceId, assetId);
                    }
                }
            }
            return isDisassociated;
        }

        private DateTime GetSunOfWeek(DateTime date)
        {
            int dayOffset = DayOfWeek.Sunday - date.DayOfWeek;
            return date.AddDays(dayOffset);
        }

        private void CreateDefaultAssetUtilizationSettings(INH_OP opContext, long assetID, DeviceTypeEnum deviceType)
        {
            DateTime defaultUpdateUTC = DateTime.UtcNow;

            AssetExpectedRuntimeHoursProjected defaultAssetExpectedRuntimeHoursProjected = new AssetExpectedRuntimeHoursProjected
              ()
            {
                HoursSun = 0,
                HoursMon = 8,
                HoursTue = 8,
                HoursWed = 8,
                HoursThu = 8,
                HoursFri = 8,
                HoursSat = 0,
                UpdateUTC = defaultUpdateUTC,
                fk_AssetID = assetID
            };
            opContext.AssetExpectedRuntimeHoursProjected.AddObject(defaultAssetExpectedRuntimeHoursProjected);

            AssetBurnRates defaultAssetBurnRates = new AssetBurnRates()
            {
                EstimatedIdleBurnRateGallonsPerHour = null,
                EstimatedWorkingBurnRateGallonsPerHour = null,
                UpdateUTC = defaultUpdateUTC,
                fk_AssetID = assetID
            };
            opContext.AssetBurnRates.AddObject(defaultAssetBurnRates);

            AssetWorkingDefinition defaultAssetWorkingDefinition = GetDefaultAssetWorkingDefinition(assetID, defaultUpdateUTC,
              deviceType);

            opContext.AssetWorkingDefinition.AddObject(defaultAssetWorkingDefinition);

            int result = opContext.SaveChanges();
            if (result < 3)
                throw new InvalidOperationException("Error creating default Asset Utilization Settings.");


            Guid assetGuid = (from asset in opContext.Asset
                              where asset.AssetID == assetID
                              select asset.AssetUID ?? new Guid()).FirstOrDefault();

            string workDefinitionType = (from assetworkingdefinition in opContext.WorkDefinition
                                         where assetworkingdefinition.ID == defaultAssetWorkingDefinition.fk_WorkDefinitionID
                                         select assetworkingdefinition.Description).FirstOrDefault();

            List<int> workDefinitionSupportedDeviceTypes = (from afsaf in opContext.AppFeatureSetAppFeatureReadOnly
                                                            join dt in opContext.DeviceTypeReadOnly on afsaf.fk_AppFeatureSetID equals dt.fk_AppFeatureSetID
                                                            where afsaf.fk_AppFeatureID == (int)AppFeatureEnum.WorkDefinitionConfig
                                                            select dt.ID).ToList<int>();


            bool isWorkDefinitionSupported = workDefinitionSupportedDeviceTypes.Contains((int)deviceType);

            if (EnableNextGenWorkDefinitionSync && isWorkDefinitionSupported)
            {
                if (workDefinitionType == "Sensor Events" || workDefinitionType == "Movement and Sensor Events")
                {
                    var workDefinitionDetails = new
                    {
                        AssetUID = assetGuid,
                        ActionUTC = DateTime.UtcNow,
                        WorkDefinitionType = workDefinitionType,
                        SensorNumber = defaultAssetWorkingDefinition.SensorNumber,
                        StartIsOn = defaultAssetWorkingDefinition.SensorStartIsOn
                    };
                    var success = _workDefinitionService.CreateWorkDefinition(workDefinitionDetails);
                    if (!success)
                    {
                        log.IfInfoFormat("Error occurred while creating default working definition for asset in VSP stack. Asset UID :{0}", assetGuid);
                    }
                }
                else
                {
                    var workDefinitionDetails = new
                    {
                        AssetUID = assetGuid,
                        ActionUTC = DateTime.UtcNow,
                        WorkDefinitionType = workDefinitionType
                    };
                    var success = _workDefinitionService.CreateWorkDefinition(workDefinitionDetails);
                    if (!success)
                    {
                        log.IfInfoFormat("Error occurred while creating default working definition for asset in VSP stack. Asset UID :{0}", assetGuid);
                    }
                }

            }


        }

        private void CreateDefaultAssetMonitoringSettings(INH_OP opContext, long assetID)
        {
            AssetMonitoring assetMonitoring = AssetMonitoring.Default(assetID);
            opContext.AssetMonitoring.AddObject(assetMonitoring);
            int result = opContext.SaveChanges();
            if (result < 1)
                throw new InvalidOperationException("Error creating default Asset Monitoring Settings.");
        }

        //We're breaking this out and making it VERY obvious that we need to initialize each devicetype explicitly.
        private static AssetWorkingDefinition GetDefaultAssetWorkingDefinition(long assetID, DateTime defaultUpdateUTC,
          DeviceTypeEnum deviceType)
        {
            int SensorNumber = 0;
            bool SensorStartIsOn = true;
            int fk_WorkDefinitionID = (int)WorkDefinitionEnum.MeterDelta;

            if (deviceType == DeviceTypeEnum.DCM300)
            {
                // Need to manually configure default for DCM300 as movement until there is a better strategy.
                fk_WorkDefinitionID = (int)WorkDefinitionEnum.MovementEvents;
            }

            AssetWorkingDefinition defaultAssetWorkingDefinition = new AssetWorkingDefinition()
            {
                fk_AssetID = assetID,
                fk_WorkDefinitionID = fk_WorkDefinitionID,
                SensorNumber = SensorNumber,
                SensorStartIsOn = SensorStartIsOn,
                UpdateUTC = defaultUpdateUTC
            };

            return defaultAssetWorkingDefinition;
        }

        public static readonly long DEFAULT_SALES_MODEL_ID = 0; //Used for assets with no model

        private void AuditConfigChanges(INH_OP ctx, string makeCode, string serialNumber, int eventType, long? UserID,
          int deviceTypeID)
        {
            AssetSecurityIncident assetSecurityIncident = new AssetSecurityIncident();
            assetSecurityIncident.SerialNumberVIN = serialNumber;
            assetSecurityIncident.fk_MakeCode = makeCode;
            assetSecurityIncident.fk_DeviceTypeID = deviceTypeID;
            assetSecurityIncident.fk_UserID = UserID;
            assetSecurityIncident.EventType = eventType.ToString();
            assetSecurityIncident.TimeStampUTC = DateTime.UtcNow;
            ctx.AssetSecurityIncident.AddObject(assetSecurityIncident);
        }

        //public IQueryable<DimAsset> GetAssetsNearby(INH_RPT rptCtx, long activeUserID, double distanceKilometers, Point beginningPoint)
        //{
        //  return (from w in rptCtx.vw_WorkingSetPopulationReadOnly
        //          join a in rptCtx.DimAssetReadOnly on w.fk_DimAssetID equals a.AssetID
        //          join acs in rptCtx.AssetCurrentStatusReadOnly on w.fk_DimAssetID equals acs.fk_DimAssetID
        //          where w.ifk_ActiveUserID == activeUserID
        //                && w.HasActiveService
        //                && acs.Latitude.HasValue
        //                && acs.Longitude.HasValue
        //                && acs.Latitude >= -90
        //                && acs.Latitude <= 90
        //                && NH_RPT.fn_GetDistanceBetweenPoints(beginningPoint.Latitude, beginningPoint.Longitude,
        //                                                   acs.Latitude.Value, acs.Longitude.Value) <= (distanceKilometers*1000)
        //          select a);
        //}

        public class AssetWorkingSet
        {
            public long AssetID;
            public string ProductFamilyName;
            public double? Latitude;
            public double? Longitude;
        }

        public int SelectAssetsForKeywordSearchIntoWorkingSet(INH_OP opCtx, long activeUserID, string keyword,
          int maxWorkingSetCount)
        {
            List<long> assetIDs = (from aws in opCtx.vw_AssetWorkingSetReadOnly
                                   join a in opCtx.AssetReadOnly on aws.fk_AssetID equals a.AssetID
                                   join m in opCtx.MakeReadOnly on a.fk_MakeCode equals m.Code
                                   where (a.SerialNumberVIN.Contains(keyword) ||
                                          a.Name.Contains(keyword) ||
                                          a.Model.Contains(keyword) ||
                                          a.ProductFamilyName.Contains(keyword) ||
                                          m.Name.Contains(keyword))
                                         && aws.fk_ActiveUserID == activeUserID && aws.HasActiveService
                                   select aws.fk_AssetID).Take(maxWorkingSetCount).ToList();

            ActiveUserSelectedAssetsAccess.Save(activeUserID, assetIDs);
            return assetIDs.Count;
        }

        public int GetPageCount(long activeUserID, long customerID, int numberToTake, List<AppFeatureEnum> featureList)
        {
            int workingsetCount;
            using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var assetList = (from w in opCtx.vw_AssetWorkingSetReadOnly
                                 where w.fk_ActiveUserID == activeUserID &&
                                       w.Selected &&
                                       w.HasActiveService
                                 select w.fk_AssetID).ToList();

                if (featureList != null)
                    workingsetCount = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetList, featureList, customerID).Count;
                else
                    workingsetCount = assetList.Count();
            }

            return (int)Math.Ceiling(workingsetCount / (double)numberToTake);
        }

        public int GetProjectCount(long customerID)
        {
            int projects = 0;
            using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                projects = (from p in opCtx.ProjectReadOnly
                            where p.fk_CustomerID == customerID &&
                                  p.Active
                            select p).Count();
            }

            return projects;
        }

        /// <summary>
        /// Retrieves the DeviceId for the given makeCode and serialNumber
        /// Currently this method is only being used by InspectionLogProcessor, 
        /// please check your query performance if you want to reuse this method.
        /// </summary>
        /// <param name="makeCode"></param>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public string GetDeviceId(string makeCode, string serialNumber)
        {
            using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                return (from device in opCtx.DeviceReadOnly
                        join asset in opCtx.AssetReadOnly on device.ID equals asset.fk_DeviceID
                        where asset.SerialNumberVIN == serialNumber && asset.fk_MakeCode == makeCode
                        select device.GpsDeviceID).FirstOrDefault();
            }
        }
    }
}
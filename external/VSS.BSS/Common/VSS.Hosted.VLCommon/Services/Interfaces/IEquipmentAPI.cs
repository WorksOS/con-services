using System;
using System.Collections.Generic;


namespace VSS.Hosted.VLCommon
{
  public interface IEquipmentAPI
  {
    Asset Create(INH_OP opContext, string assetName, string makeCode, string serialNumber, long deviceID,
                        DeviceTypeEnum deviceType, string familyDesc, string modelDesc, int year, Guid assetGuid, string assetVinSN = null, long storeId = 1);
    void PopulateAssetAlias(INH_OP ctx, long assetID, string updatedAssetName, long? userID);

    bool Update(INH_OP opContext, long assetID, List<Param> updatedFields);
    bool UpdateByAssetUid(INH_OP opContext, Guid assetUid, List<Param> updatedFields);
    bool UpdateLeaseOwner(long[] assetID, long? leaseOwnerCustomerID, long? userID);
    bool UpdateWorkingDefinition(INH_OP opContext, long assetID, WorkDefinitionEnum workDefn, int sensorNumber, bool sensorStartIsOn);
    bool UpdateFuelBurnRate(INH_OP opContext, long assetID, double idleBurnRate, double workingBurnRate);
    bool ClaimAsset(INH_OP ctx, Guid assetID, long storeId);
    bool ReleaseAsset(INH_OP ctx, Guid assetID);
    bool AssociateAssetDevice(INH_OP opContext, long assetId, long deviceId);
    AssetDeviceHistory GetAssetDeviceHistory(INH_OP opCtx, long assetId);
    AssetDeviceHistory CreateAssetDeviceHistory(INH_OP opContext, long assetId, long deviceId, string ownerBssId, DateTime startUtc);
    bool DisassociateAssetDevice(INH_OP opContext, long assetId, long deviceId);    

    string BuildAssetName(INH_OP opOpContext, string nickname, string makeCode, string equipSerialNumberVin);
    long GetOemPMSalesModelID(INH_OP opOpContext, long assetID);
    string GetStartModeDescription(int startMode, string locale);
    string GetMachineStartModeDescription(int startMode, string locale);
    int SelectAssetsForKeywordSearchIntoWorkingSet(INH_OP rptCtx, long activeUserID, string keyword, int maxWorkingSetCount);
    int GetPageCount(long activeUserID, long customerID, int numberToTake, List<AppFeatureEnum> featureList);
    int GetProjectCount(long customerID);
    string GetDeviceId(string makeCode, string serialNumber);
  }
}

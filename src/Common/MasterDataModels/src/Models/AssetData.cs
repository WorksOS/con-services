using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  ///   Describes VL asset
  /// </summary>
  public class AssetData  : IMasterDataModel
  {
    public Guid CustomerUID { get; set; }
    public Guid AssetUID { get; set; }
    public string AssetName { get; set; }
    public long LegacyAssetID { get; set; }
    
    public string SerialNumber { get; set; }
    public string MakeCode { get; set; }
    public string Model { get; set; }
    public string AssetTypeName { get; set; }
    public string EquipmentVIN { get; set; }
    public long IconKey { get; set; }
    public long ModelYear { get; set; }
    
    public AssetData()
    { }

    public AssetData(Guid customerUID, Guid assetUID, string assetName, long legacyAssetId,
        string serialNumber, string makeCode, string model, string assetTypeName,
        string equipmentVIN, long iconKey, long modelYear)
    {
      CustomerUID = customerUID;
      AssetUID = assetUID;
      LegacyAssetID = legacyAssetId;
      SerialNumber = serialNumber;
      MakeCode = makeCode;
      Model = model;
      AssetTypeName = assetTypeName;
      EquipmentVIN = equipmentVIN;
      IconKey = iconKey;
      ModelYear = modelYear;
    }

    public override bool Equals(object obj)
    {
      var otherAssetData = obj as AssetData;
      if (otherAssetData == null) return false;
      return otherAssetData.CustomerUID == this.CustomerUID
             && otherAssetData.AssetUID == this.AssetUID
             && otherAssetData.AssetName == this.AssetName
             && otherAssetData.LegacyAssetID == this.LegacyAssetID
             && otherAssetData.SerialNumber == this.SerialNumber
             && otherAssetData.MakeCode == this.MakeCode
             && otherAssetData.Model == this.Model
             && otherAssetData.AssetTypeName == this.AssetTypeName
             && otherAssetData.EquipmentVIN == this.EquipmentVIN
             && otherAssetData.IconKey == this.IconKey
             && otherAssetData.ModelYear == this.ModelYear
        ;
    }
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public List<string> GetIdentifiers() => new List<string>()
    {
      CustomerUID.ToString(),
      AssetUID.ToString()
    };
  }
}
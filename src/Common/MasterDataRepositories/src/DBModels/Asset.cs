using System;

namespace VSS.MasterData.Repositories.DBModels
{
    public class Asset
    {
        public string AssetUID { get; set; }
        public string Name { get; set; }
        public long LegacyAssetID { get; set; }
        public string EquipmentVIN { get; set; }
        public string SerialNumber { get; set; }
        public string MakeCode { get; set; }
        public string Model { get; set; }
        public int? ModelYear { get; set; }
        public string AssetType { get; set; }
        public int? IconKey { get; set; }
        public string OwningCustomerUID { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? LastActionedUtc { get; set; }

        public override bool Equals(object obj)
        {
            var otherAsset = obj as Asset;
            if (otherAsset == null) return false;
            return otherAsset.AssetUID == AssetUID
                   && otherAsset.Name == Name
                   && otherAsset.LegacyAssetID == LegacyAssetID
                   && otherAsset.EquipmentVIN == EquipmentVIN
                   && otherAsset.SerialNumber == SerialNumber
                   && otherAsset.MakeCode == MakeCode
                   && otherAsset.Model == Model
                   && otherAsset.ModelYear == ModelYear
                   && otherAsset.AssetType == AssetType
                   && otherAsset.IconKey == IconKey
                   && otherAsset.OwningCustomerUID == OwningCustomerUID
                   && otherAsset.IsDeleted == IsDeleted
                   && otherAsset.LastActionedUtc == LastActionedUtc;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
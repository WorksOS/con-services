namespace VSS.MasterData.WebAPI.DbModel
{
	public class CustomerAsset
	{
		#region Public Properties
		public object AssetUID { get; set; }
		public string AssetName { get; set; }
		public long LegacyAssetID { get; set; }
		public string SerialNumber { get; set; }
		public string MakeCode { get; set; }
		public string Model { get; set; }
		public string AssetTypeName { get; set; }
		public string EquipmentVIN { get; set; }
		public int IconKey { get; set; }
		public int ModelYear { get; set; }

		[System.Text.Json.Serialization.JsonIgnore]
		public bool StatusInd { get; set; }
		public object OwningCustomerUID { get; set; }

		#endregion Public Properties

		#region Public Methods
		public override bool Equals(object obj)
		{
			var otherAsset = obj as CustomerAsset;
			if (otherAsset == null) return false;
			return otherAsset.AssetUID == AssetUID && otherAsset.AssetName == AssetName &&
				   otherAsset.SerialNumber == SerialNumber
				   && otherAsset.MakeCode == MakeCode && otherAsset.Model == Model && otherAsset.IconKey == IconKey;
		}
		public override int GetHashCode() { return AssetUID.GetHashCode(); }

		#endregion Public Methods
	}
}

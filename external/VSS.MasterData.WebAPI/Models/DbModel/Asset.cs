using System.Text.Json;
 
namespace VSS.MasterData.WebAPI.DbModel
{
	public class Asset
	{

		#region Public Properties
		/// <summary>
		/// 
		/// </summary>
		public string AssetUID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string AssetName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public long LegacyAssetID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string SerialNumber { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string MakeCode { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Model { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string AssetTypeName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string EquipmentVIN { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int IconKey { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int ModelYear { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[System.Text.Json.Serialization.JsonIgnore]
		public bool StatusInd { get; set; }

		#endregion Public Properties

		#region Public Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			var otherAsset = obj as Asset;
			if (otherAsset == null) return false;
			return otherAsset.AssetUID == AssetUID && otherAsset.AssetName == AssetName &&
				   otherAsset.SerialNumber == SerialNumber
				   && otherAsset.MakeCode == MakeCode && otherAsset.Model == Model && otherAsset.IconKey == IconKey;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode() { return AssetUID.GetHashCode(); }

		#endregion Public Methods
	}

	
}
using System;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbAssetCustomer : IDbTable
	{
		#region Private Fields

		public static readonly string TableName = "md_customer_CustomerAsset";
		public static readonly string IgnoreColumns = "AssetCustomerID,Fk_CustomerUID,Fk_AssetUID";
		public static readonly string IdColumn = "AssetCustomerID";

		#endregion

		public int AssetCustomerID { get; set; }
		public Guid Fk_CustomerUID { get; set; }
		public Guid Fk_AssetUID { get; set; }
		public int fk_AssetRelationTypeID { get; set; }
		public DateTime LastCustomerUTC { get; set; }

		public string GetIdColumn()
		{
			return IdColumn;
		}

		public string GetIgnoreColumnsOnUpdate()
		{
			return IgnoreColumns;
		}

		public string GetTableName()
		{
			return TableName;
		}
	}
}

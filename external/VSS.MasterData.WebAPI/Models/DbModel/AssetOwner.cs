using System;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetOwner : IDbTable
	{
		public static readonly string TableName = "md_asset_AssetOwner";
		public static readonly string IgnoreColumns = "AssetOwnerID,InsertUTC";

		public Guid fk_AssetUID { get; set; }
		public Guid? fk_CustomerUID { get; set; }
		public string CustomerName { get; set; }
		public Guid? fk_DealerCustomerUID { get; set; }
		public string DealerName { get; set; }
		public Guid? fk_AccountCustomerUID { get; set; }
		public string AccountName { get; set; }
		public string NetworkCustomerCode { get; set; }
		public string DealerAccountCode { get; set; }
		public string NetworkDealerCode { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }

		public string GetIdColumn()
		{
			throw new NotImplementedException();
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


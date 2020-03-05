using System;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbCustomer : IDbTable
	{
		#region Private Fields

		public static readonly string TableName = "md_customer_Customer";
		public static readonly string IgnoreColumns = "CustomerID,CustomerUID";
		public static readonly string IdColumn = "CustomerID";

		#endregion
		public int CustomerID { get; set; }
		public string CustomerName { get; set; }
		public long fk_CustomerTypeID { get; set; }
		public Guid CustomerUID { get; set; }
		public string PrimaryContactEmail { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime LastCustomerUTC { get; set; }
		public string NetworkDealerCode { get; set; }
		public bool? IsActive { get; set; }
		public string BSSID { get; set; }
		public string DealerNetwork { get; set; }
		public string NetworkCustomerCode { get; set; }
		public string DealerAccountCode { get; set; }

		public string GetTableName()
		{
			return TableName;
		}
		public string GetIgnoreColumnsOnUpdate()
		{
			return IgnoreColumns;
		}

		public string GetIdColumn()
		{
			return IdColumn;
		}
	}
}

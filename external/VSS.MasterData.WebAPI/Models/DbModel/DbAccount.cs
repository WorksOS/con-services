using System;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbAccount : IDbTable
	{
		#region Private Fields

		private static readonly string TableName = "md_customer_CustomerAccount";
		private static readonly string IgnoreColumns = "CustomerAccountUID,CustomerAccountID";
		public static readonly string IdColumn = "CustomerAccountID";

		#endregion

		public int CustomerAccountID { get; set; }
		public string AccountName { get; set; }
		public string BSSID { get; set; }
		public string NetworkCustomerCode { get; set; }
		public string DealerAccountCode { get; set; }
		public Guid CustomerAccountUID { get; set; }
		public Guid? fk_ParentCustomerUID { get; set; }
		public Guid? fk_ChildCustomerUID { get; set; }
		public DateTime RowUpdatedUTC { get; set; }

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
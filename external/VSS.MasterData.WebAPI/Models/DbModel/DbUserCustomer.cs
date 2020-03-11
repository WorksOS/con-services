using System;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbUserCustomer : IDbTable
	{
		#region Private Fields

		private static readonly string TableName = "md_customer_CustomerUser";
		private static readonly string IgnoreColumns = "UserCustomerID,fk_UserUID,fk_CustomerUID";
		public static readonly string IdColumn = "UserCustomerID";

		#endregion

		public int UserCustomerID { get; set; }
		public Guid fk_UserUID { get; set; }
		public Guid fk_CustomerUID { get; set; }
		public int fk_CustomerID { get; set; }
		public DateTime LastUserUTC { get; set; }

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

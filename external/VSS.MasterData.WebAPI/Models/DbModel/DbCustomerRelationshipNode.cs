using System;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbCustomerRelationshipNode : IDbTable
	{
		#region Private Fields
		private static readonly string TableName = "md_customer_CustomerRelationshipNode";
		private static readonly string IgnoreColumns = "CustomerRelationshipNodeID";
		public static readonly string IdColumn = "CustomerRelationshipNodeID";
		#endregion

		public long CustomerRelationshipNodeID { get; set; }
		public Guid fk_RootCustomerUID { get; set; }
		public Guid fk_ParentCustomerUID { get; set; }
		public Guid FK_CustomerUID { get; set; }
		public int LeftNodePosition { get; set; }
		public int RightNodePosition { get; set; }
		public DateTime LastCustomerRelationshipNodeUTC { get; set; }

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

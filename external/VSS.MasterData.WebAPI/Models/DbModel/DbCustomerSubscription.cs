using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbCustomerSubscription : IDbTable
	{
		public Guid CustomerSubscriptionUID { get; set; }

		public Guid fk_CustomerUID { get; set; }

		public long fk_ServiceTypeID { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public DateTime InsertUTC { get; set; }

		public DateTime UpdateUTC { get; set; }

		public string GetIgnoreColumnsOnUpdate()
		{
			return "fk_CustomerUID,fk_ServiceTypeID,InsertUTC";
		}

		public string GetTableName()
		{
			return "md_subscription_CustomerSubscription";
		}
		

		public string GetIdColumn()
		{
			return null;
		}
	}
}
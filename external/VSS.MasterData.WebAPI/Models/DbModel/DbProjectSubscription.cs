using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbProjectSubscription : IDbTable
	{
		public Guid ProjectSubscriptionUID { get; set; }

		public Guid? fk_ProjectUID { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public DateTime InsertUTC { get; set; }

		public DateTime UpdateUTC { get; set; }

		public Guid fk_CustomerUID { get; set; }

		public long fk_ServiceTypeID { get; set; }

		public string GetIgnoreColumnsOnUpdate()
		{
			return "InsertUTC";
		}

		public string GetTableName()
		{
			return "md_subscription_ProjectSubscription";
		}
		

		public string GetIdColumn()
		{
			return null;
		}
	}
}
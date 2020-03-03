using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class DbAssetSubscription : IDbTable
	{
		public Guid AssetSubscriptionUID { get; set; }

		public Guid fk_AssetUID { get; set; }

		public Guid fk_DeviceUID { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public DateTime InsertUTC { get; set; }

		public DateTime UpdateUTC { get; set; }

		public Guid fk_CustomerUID { get; set; }

		public long fk_ServiceTypeID { get; set; }

		public int LastProcessStatus { get; set; }

		public int fk_SubscriptionSourceID { get; set; }
		
		public string GetIgnoreColumnsOnUpdate()
		{
			return "LastProcessStatus,InsertUTC";
		}

		public string GetTableName()
		{
			return "md_subscription_AssetSubscription";
		}


		public string GetIdColumn()
		{
			return null;
		}
	}
}
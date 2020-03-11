using System;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class OwnerVisibility
	{
		public Guid CustomerUID { get; set; }
		public string CustomerName { get; set; }

		public string CustomerType { get; set; }

		public Guid SubscriptionUID { get; set; }

		public string SubscriptionName { get; set; }

		public string SubscriptionStatus { get; set; }

		public DateTime SubscriptionStartDate { get; set; }

		public DateTime SubscriptionEndDate { get; set; }
	}
}
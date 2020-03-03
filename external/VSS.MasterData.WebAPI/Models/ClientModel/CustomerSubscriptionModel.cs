using System;

namespace VSS.MasterData.WebAPI.ClientModel
{ 
	public class CustomerSubscriptionModel
	{
		public string SubscriptionType { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }
	}
}
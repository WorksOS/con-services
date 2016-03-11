using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using VSS.Subscription.Data.Interfaces;

namespace VSS.Subscription.Data.Models
{
	public class CreateCustomerSubscriptionEvent : ISubscriptionEvent
	{
		[Required]
		public Guid SubscriptionUID { get; set; }

		[Required]
		public Guid CustomerUID { get; set; }

		[Required]
		public string SubscriptionType { get; set; }

		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		public DateTime EndDate { get; set; }

		[Required]
		public DateTime ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }
	}
}

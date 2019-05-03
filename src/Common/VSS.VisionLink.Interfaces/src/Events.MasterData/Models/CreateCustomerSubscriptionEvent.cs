using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
	public class CreateCustomerSubscriptionEvent : ISubscriptionEvent
	{
		public Guid SubscriptionUID { get; set; }

		public Guid CustomerUID { get; set; }

    public string SubscriptionType { get; set; } //Required Field

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public DateTime ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }
	}
}

using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
	public class CreateCustomerEvent : ICustomerEvent
	{
		public string CustomerName { get; set; } //Required Field
		public string CustomerType { get; set; } //Required Field
		public string BSSID { get; set; }
		public string DealerNetwork { get; set; } //Required Field
		public string NetworkDealerCode { get; set; }
		public string NetworkCustomerCode { get; set; }
		public string DealerAccountCode { get; set; }
		public string PrimaryContactEmail { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public Guid CustomerUID { get; set; }
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }
	}
}
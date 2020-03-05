using System;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class CustomerResponse
	{
		public Guid CustomerUID { get; set; }
		public string CustomerName { get; set; }
		public string CustomerType { get; set; }
		public string NetworkCustomerCode { get; set; }
		public string NetworkDealerCode { get; set; }
		public string BSSID { get; set; }
		public string DealerAccountCode { get; set; }
		public string PrimaryContactEmail { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}

using System;
using VSS.MasterData.WebAPI.Utilities.Attributes;

namespace VSS.MasterData.WebAPI.Customer.KafkaModel
{
	public class AccountEvent
	{
		[DbFieldName("AccountName")]
		public string AccountName { get; set; }
		[DbFieldName("BSSID")]
		public string BSSID { get; set; }
		[DbFieldName("NetworkCustomerCode")]
		public string NetworkCustomerCode { get; set; }
		[DbFieldName("DealerAccountCode")]
		public string DealerAccountCode { get; set; }
		public string Action { get; set; }
		public Guid? fk_ParentCustomerUID { get; set; }
		public Guid? fk_ChildCustomerUID { get; set; }
		public Guid AccountUID { get; set; }
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }
	}
}

using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.Customer.KafkaModel
{
	public class DeleteCustomerEvent
	{
		[Required]
		public Guid CustomerUID { get; set; }
		[Required]
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }
	}
}
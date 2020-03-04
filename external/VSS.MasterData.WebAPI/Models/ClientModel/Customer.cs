using System;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class Customer
	{
		public Guid CustomerUID { get; set; }
		public string CustomerName { get; set; }
		public string CustomerType { get; set; }
	}
}

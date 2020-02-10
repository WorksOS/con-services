using System;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class AssetCustomerResponse
	{
		public Guid CustomerUID { get; set; }
		public string CustomerName { get; set; }
		public string CustomerType { get; set; }
		public Guid? ParentCustomerUID { get; set; }
		public string ParentName { get; set; }
		public string ParentCustomerType { get; set; }
	}
}

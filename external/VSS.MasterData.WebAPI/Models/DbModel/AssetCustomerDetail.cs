using System;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetCustomerDetail
	{
		public Guid AssetUID { get; set; }
		public Guid CustomerUID { get; set; }
		public string CustomerName { get; set; }
		public int CustomerType { get; set; }
		public Guid? ParentCustomerUID { get; set; }
		public string ParentName { get; set; }
		public int ParentCustomerType { get; set; }
	}
}

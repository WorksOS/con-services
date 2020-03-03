using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class CustomerHierarchyInfo
	{
		public string UserUID { get; set; }
		public List<CustomerHierarchyNode> Customers { get; set; }
	}
}

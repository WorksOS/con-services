using Newtonsoft.Json;
using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class CustomerHierarchyNode
	{
		public CustomerHierarchyNode()
		{
			Children = new List<CustomerHierarchyNode>();
		}
		public string CustomerUID { get; set; }
		public string Name { get; set; }
		public string CustomerType { get; set; }
		[JsonIgnore]
		public int CustomerId { get; set; }
		[JsonIgnore]
		public int LeftNodePosition { get; set; }
		[JsonIgnore]
		public int RightNodePosition { get; set; }
		public string CustomerCode { get; set; }
		public string DisplayName { get; set; }
		public List<CustomerHierarchyNode> Children { get; private set; }
	}
}

using System;
using VSS.MasterData.WebAPI.Utilities.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class AssociatedCustomer
	{
		[JsonProperty("uid")]
		public Guid CustomerUID { get; set; }
		[JsonProperty("name")]
		public string CustomerName { get; set; }
		[JsonProperty("type")]
		[JsonConverter(typeof(StringEnumConverter))]
		public CustomerType CustomerType { get; set; }
	}
}
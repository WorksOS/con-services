using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace VSS.MasterData.WebAPI.ClientModel
{
	/// <summary>
	/// Customer List Success Response class
	/// </summary>
	public class CustomerListSuccessResponse
	{
		/// <summary>
		/// HttpStatusCode
		/// </summary>
		[JsonProperty("status")]
		public HttpStatusCode Status { get; set; }

		/// <summary>
		/// GetMetadata
		/// </summary>
		[JsonProperty("metadata")]
		public Metadata Metadata { get; set; }

		/// <summary>
		/// GetMetadata
		/// </summary>
		[JsonProperty("customer")]
		public List<AssociatedCustomer> Customers { get; set; }
	}
}

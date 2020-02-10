using Newtonsoft.Json;

namespace VSS.MasterData.WebAPI.ClientModel
{
	/// <summary>
	/// Metadata object
	/// </summary>
	public class Metadata
	{
		/// <summary>
		/// Message
		/// </summary>
		[JsonProperty("msg")]
		public string Message { get; set; }
	}
}
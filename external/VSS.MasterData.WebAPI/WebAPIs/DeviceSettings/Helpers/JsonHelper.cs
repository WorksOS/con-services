using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DeviceSettings.Helpers
{
	/// <summary>
	/// Json object helper methods
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static class JsonHelper
	{
		#region  Public Methods

		/// <summary>
		/// Serialize an object to Json string
		/// </summary>
		/// <typeparam name="T">object type</typeparam>
		/// <param name="msg">message to serialize</param>
		/// <returns>serilaized json string</returns>
		public static string SerializeObjectToJson<T>(T msg)
		{
			return JsonConvert.SerializeObject(msg);
		}

		/// <summary>
		/// Serialize an object to Json string conforming to a specific serializer settings
		/// </summary>
		/// <typeparam name="T">object type</typeparam>
		/// <param name="msg">message to serialize</param>
		/// <param name="settings">serializer settings</param>
		/// <returns></returns>
		public static string SerializeObjectToJson<T>(T msg, JsonSerializerSettings settings)
		{
			return JsonConvert.SerializeObject(msg, settings);
		}

		/// <summary>
		/// Deserialize json to type
		/// </summary>
		/// <typeparam name="T">object type</typeparam>
		/// <param name="msg">message to deserialize</param>
		/// <returns></returns>
		//This method name(DeserializeJsonToObject) is hardcoded in ChunkedEncodingFilterAttribute's reflection
		public static T DeserializeJsonToObject<T>(string msg)
		{
			return JsonConvert.DeserializeObject<T>(msg, new CustomJsonConverter());
		}

		#endregion  Public Methods
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.VisionLink.SearchAndFilter.Interfaces.v1_6.DataContracts;
using VSS.MasterData.WebAPI.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace VSS.MasterData.WebAPI.Asset.Helpers
{
	[ExcludeFromCodeCoverage]
	public class ControllerUtilities : IControllerUtilities
	{

		#region Public Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryNameValuePairs"></param>
		/// <returns></returns>
		public string GetParameterNamesAndValues(IEnumerable<KeyValuePair<string, string>> queryNameValuePairs)
		{
			var nameValuePairs = queryNameValuePairs as IList<KeyValuePair<string, string>> ?? queryNameValuePairs.ToList();
			if (!nameValuePairs.Any())
				return "no parameters";
			var output = new StringBuilder();
			var count = 0;
			foreach (var nvp in nameValuePairs)
				output.Append(count++ == 0 ? string.Empty : ", ").Append(nvp);
			return output.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryNames"></param>
		/// <param name="parameterNames"></param>
		/// <exception cref="Exception"></exception>
		public void VerifyParameters(IEnumerable<string> queryNames, IEnumerable<string> parameterNames)
		{
			parameterNames = parameterNames.ToList().ConvertAll(d => d.ToLower());
			foreach (var queryParameterName in queryNames)
			{
				if (!parameterNames.Contains(queryParameterName.ToLower()))
				{
					throw new Exception($"Invalid query parameter '{queryParameterName}'");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryNames"></param>
		/// <param name="singleValueParameterNames"></param>
		/// <exception cref="Exception"></exception>
		public void VerifySingleValueParameters(IEnumerable<string> queryNames,
		  IEnumerable<string> singleValueParameterNames)
		{
			queryNames = queryNames.ToList().ConvertAll(d => d.ToLower());
			singleValueParameterNames = singleValueParameterNames.ToList();
			foreach (var parameterName in singleValueParameterNames)
			{
				if (queryNames.Count(o => o == parameterName.ToLower()) > 1)
				{
					throw new Exception($"Invalid query parameter '{parameterName}'.  Cannot have multiple instances.");
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetUID"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public Guid[] ValidateAssetUIDParameters(string[] assetUID)
		{
			var assetUIDs = new List<Guid>();
			if (assetUID != null && assetUID.Count() != 0)
			{
				foreach (var auid in assetUID)
				{
					try
					{
						assetUIDs.Add(new Guid(auid));
					}
					catch (Exception ex)
					{
						throw new Exception($"Invalid assetUID '{auid}'", ex);
					}
				}
			}
			return assetUIDs.ToArray();
		}		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pageSize"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSizeInt"></param>
		/// <param name="pageNumberInt"></param>
		/// <exception cref="Exception"></exception>
		public void ValidatePageParameters(string pageSize, string pageNumber, out int pageSizeInt,
		  out int pageNumberInt)
		{
			if (!Int32.TryParse(pageSize, out pageSizeInt))
				throw new Exception($"Invalid pageSize '{pageSize}'");
			if (pageSizeInt < 1)
				throw new Exception($"Invalid pageSize '{pageSizeInt}'");
			if (!Int32.TryParse(pageNumber, out pageNumberInt))
				throw new Exception($"Invalid pageNumber '{pageNumber}'");
			if (pageNumberInt < 1)
				throw new Exception($"Invalid pageNumber '{pageNumberInt}'");
		}



		#endregion Public Methods
	}
}
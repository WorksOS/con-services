using System;
using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface IControllerUtilities
	{
		#region Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryNameValuePairs"></param>
		/// <returns></returns>
		string GetParameterNamesAndValues(IEnumerable<KeyValuePair<string, string>> queryNameValuePairs);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryNames"></param>
		/// <param name="parameterNames"></param>
		void VerifyParameters(IEnumerable<string> queryNames, IEnumerable<string> parameterNames);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queryNames"></param>
		/// <param name="singleValueParameterNames"></param>
		void VerifySingleValueParameters(IEnumerable<string> queryNames, IEnumerable<string> singleValueParameterNames);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetUID"></param>
		/// <returns></returns>
		Guid[] ValidateAssetUIDParameters(string[] assetUID);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pageSize"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSizeInt"></param>
		/// <param name="pageNumberInt"></param>
		void ValidatePageParameters(string pageSize, string pageNumber, out int pageSizeInt, out int pageNumberInt);

		#endregion Methods
	}
}
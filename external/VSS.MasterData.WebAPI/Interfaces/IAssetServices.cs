using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface IAssetServices
	{
		bool ValidateAuthorizedCustomerByAsset(Guid assetGuid, Guid userGuid);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		bool CreateAsset(CreateAssetEvent asset);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		bool UpdateAsset(UpdateAssetEvent asset);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		bool DeleteAsset(DeleteAssetPayload asset);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetGuid"></param>
		/// <param name="make"></param>
		/// <param name="sno"></param>
		/// <param name="persistPattern"></param>
		/// <returns></returns>
		Guid? GetAssetUid(Guid assetGuid, string make, string sno);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetGuids"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		List<DbModel.Asset> GetAssets(Guid[] assetGuids, Guid userGuid);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetUID"></param>
		/// <param name="deviceUID"></param>
		/// <returns></returns>
		object GetAssetDetail(Guid? assetUID, Guid? deviceUID = null);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		List<object> GetHarvesterAssets();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="searchString"></param>
		/// <param name="pageNum"></param>
		/// <param name="pageLimit"></param>
		/// <returns></returns>
		object GetAssetsforSupportUser(string searchString, int pageNum, int pageLimit);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetGuid"></param>
		/// <param name="make"></param>
		/// <param name="sno"></param>
		/// <param name="custGuid"></param>
		/// <returns></returns>
		//Guid? GetAssetUidForCustomers(Guid assetGuid, string make, string sno, IList<Guid> custGuid);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appName"></param>
		/// <returns></returns>
		List<Guid> GetCustomersForApplication(string appName);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="customerGuids"></param>
		/// <param name="pageNum"></param>
		/// <param name="pageLimit"></param>
		/// <returns></returns>
		CustomerAssetsListData GetAssetsForCustomer(List<Guid> customerGuids, int pageNum, int pageLimit);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userGuid"></param>
		/// <param name="legacyAssetID"></param>
		/// <param name="AssetUID"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="makeCode"></param>
		/// <param name="serialNumber"></param>
		/// <returns></returns>
		List<LegacyAssetData> GetAssetByAssetLegacyID(Guid userGuid, long legacyAssetID = 0, Guid? AssetUID = null, long? pageNumber = 1, int? pageSize = 100, string makeCode = null, string serialNumber = null);

		ClientModel.Asset GetAsset(Guid AssetUID);

		bool IsValidMakeCode(string makeCode);
		// ----------
		//AssetNameIconStatus GetAssetNameIconandStatus(Guid assetGuid);
		///// <summary>
		///// 
		///// </summary>
		///// <param name="assetGuid"></param>
		///// <returns></returns>
		//bool GetAssetStatus(Guid assetGuid);

		///// <summary>
		///// 
		///// </summary>
		///// <param name="assetUID"></param>
		///// <returns></returns>
		//string GetAssetOwningCustomer(Guid assetUID);
	}
}

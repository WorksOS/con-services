using System;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface IAssetOwnerServices
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetOwnerEvent"></param>
		/// <returns></returns>
		bool CreateAssetOwnerEvent(AssetOwnerEvent assetOwnerEvent);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetOwnerEvent"></param>
		/// <returns></returns>
		bool UpdateAssetOwnerEvent(AssetOwnerEvent assetOwnerEvent);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetOwnerEvent"></param>
		/// <returns></returns>
		bool DeleteAssetOwnerEvent(AssetOwnerEvent assetOwnerEvent);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetGuid"></param>
		/// <returns></returns>
		bool CheckExistingAssetOwner(Guid assetGuid);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetGuid"></param>
		/// <returns></returns>
		AssetOwnerInfo GetExistingAssetOwner(Guid assetGuid);

		///// <summary>
		///// 
		///// </summary>
		///// <param name="existingAssetOwner"></param>
		///// <param name="assetowner"></param>
		/// <returns></returns>
		//bool CheckExistingAssetOwnerForUpdate(AssetOwnerInfo existingAssetOwner, AssetOwnerPayload assetowner);

	}
}


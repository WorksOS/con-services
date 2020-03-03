using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.ClientModel;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface ISupportAssetServices
	{
		List<AssetCustomer> GetAssetCustomerByAssetGuid(Guid assetUid);
		AssetSubscriptionModel GetSubscriptionForAsset(Guid assetGuid);
		List<AssetDetail> GetAssetDetailFromAssetGuids(List<Guid> assetUIDs);
	}
}

using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.DbModel;

namespace VSS.MasterData.WebAPI.Interfaces
{
	public interface IAssetECMInfoServices
	{
		List<AssetECM> GetAssetECMInfo(Guid assetGuid);
	}
}

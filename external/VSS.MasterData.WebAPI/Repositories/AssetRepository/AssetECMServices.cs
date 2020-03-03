using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.WebAPI.DbModel;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.Utilities.Extensions;

namespace VSS.MasterData.WebAPI.AssetRepository
{
	public class AssetECMServices : IAssetECMInfoServices
	{
		#region Constructors & Declarations

		private readonly ITransactions _transaction;
		private readonly IConfiguration _configuration;

		public AssetECMServices(ITransactions transactions, IConfiguration configuration)
		{
			_transaction = transactions;
			_configuration = configuration;
		}

		#endregion

		#region Public Methods

		public List<AssetECM> GetAssetECMInfo(Guid assetGuid)
		{

			var readAssetECMQuery = $"SELECT ECMSerialNumber AS SerialNumber, FirmwarePartNumber AS PartNumber, ECMDescription AS Description," +
				$"IsSyncClockEnabled AS SyncClockEnabled,SyncClockLevel AS SyncClockLevel " +
				$"FROM msg_md_assetecm_AssetEcmInfo ecm " +
				$"INNER JOIN md_asset_AssetDevice ad on ecm.AssetUID = ad.fk_AssetUID and ecm.DeviceUID = ad.fk_DeviceUID WHERE AssetUID = {assetGuid.ToStringWithoutHyphens().WrapWithUnhex()} " +
				$"and ECMSerialNumber <> 'Unavailable' ORDER BY ECMSerialNumber; ";

			return _transaction.Get<AssetECM>(readAssetECMQuery).ToList();
		}
		#endregion
	}
}
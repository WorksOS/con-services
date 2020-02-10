using DbModel.DeviceConfig;
using Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceConfigRepository.MySql.DeviceConfig
{

	//TODO: Need to remove if the AssetSecurityHist is being handled by Messaging Acknowledgement processor
	public class AssetSecurityHistoryRepository : IAssetSecurityHistoryRepository
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;
        private const string logMethodFormat = "AssetSecurityHistoryRepository.{0}";

        public AssetSecurityHistoryRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<AssetSecurityHistoryDto> InsertSecurityMode(AssetSecurityHistoryDto assetSecurityHistoryDto)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "AssetSecurityHistoryRepository.InsertSecurityMode");
                this._transactions.Upsert(assetSecurityHistoryDto);
                return assetSecurityHistoryDto;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", "AssetSecurityHistoryRepository.InsertSecurityMode", ex);
                throw ex;
            }
        }

        public async Task<AssetSecurityHistoryDto> UpdateSecurityMode(AssetSecurityHistoryDto assetSecurityHistoryDto)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "AssetSecurityHistoryRepository.UpdateSecurityMode");
                this._transactions.Upsert(assetSecurityHistoryDto);
                return assetSecurityHistoryDto;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", "AssetSecurityHistoryRepository.UpdateSecurityMode", ex);
                throw ex;
            }
        }

        public async Task<AssetSecurityHistoryDto> InsertSecurityStatus(AssetSecurityHistoryDto assetSecurityHistoryDto)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "AssetSecurityHistoryRepository.InsertSecurityStatus");
                this._transactions.Upsert(assetSecurityHistoryDto);
                return assetSecurityHistoryDto;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", "AssetSecurityHistoryRepository.InsertSecurityStatus", ex);
                throw ex;
            }
        }

        public async Task<AssetSecurityHistoryDto> UpdateSecurityStatus(AssetSecurityHistoryDto assetSecurityHistoryDto)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "AssetSecurityHistoryRepository.UpdateSecurityStatus");
                this._transactions.Upsert(assetSecurityHistoryDto);
                return assetSecurityHistoryDto;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", "AssetSecurityHistoryRepository.UpdateSecurityStatus", ex);
                throw ex;
            }
        }

        public async Task<AssetSecurityHistoryDto> Fetch(AssetSecurityHistoryDto assetSecurityHistoryDto)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "AssetSecurityHistoryRepository.Fetch");
                var result = await this._transactions.GetAsync<AssetSecurityHistoryDto>(Queries.SELECT_ASSET_SECURITY_HISTORY, assetSecurityHistoryDto);
                return result.ToList().FirstOrDefault();
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", "AssetSecurityHistoryRepository.Fetch", ex);
                throw ex;
            }
        }
    }
}

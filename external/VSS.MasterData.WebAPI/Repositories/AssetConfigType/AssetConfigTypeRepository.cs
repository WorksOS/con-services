using DbModel.AssetSettings;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace AssetConfigTypeRepository
{
	public class AssetConfigTypeRepository : IAssetConfigTypeRepository
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;

        public AssetConfigTypeRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<IEnumerable<AssetConfigTypeDto>> FetchByConfigTypeNames(AssetConfigTypeDto configType)
        {
            try
            {
                this._loggingService.Debug("Started executing query", MethodInfo.GetCurrentMethod().Name);
                var result = await this._transactions.GetAsync<AssetConfigTypeDto>(
                        Queries.SELECT_ASSET_CONFIG_TYPE,
                        configType
                        );
                this._loggingService.Debug("Ended executing query", MethodInfo.GetCurrentMethod().Name);
                return result.ToList();
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }
    }
}

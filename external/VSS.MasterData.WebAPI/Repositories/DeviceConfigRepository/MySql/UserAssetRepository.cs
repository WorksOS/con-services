using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using Interfaces;
using DbModel;
using DbModel.DeviceConfig;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceConfigRepository.MySql
{
    public class UserAssetRepository : IUserAssetRepository
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;
        private const string _logMethodFormat = "UserAssetRepository.{0}";

        public UserAssetRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(typeof(UserAssetRepository));
        }
        public async Task<IEnumerable<string>> FetchValidAssetUIds(IList<string> assetUids, UserAssetDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", string.Format(_logMethodFormat, "FetchValidAssetUIds"));
                var result = await this._transactions.GetAsync<string>(
                    string.Format(Queries.FetchValidAssetUIDs,
                    string.Join(",", assetUids.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty)),
                    request);
                this._loggingService.Info(string.Format("Valid Asset UIDs for request : {0} - {1}", string.Join(",",assetUids), JsonConvert.SerializeObject(request)), string.Format(_logMethodFormat, "FetchValidAssetUIds"));
                this._loggingService.Debug("Ended executing query", string.Format(_logMethodFormat, "FetchValidAssetUIds"));
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<string>> FetchValiAssetUIDs(IList<string> assetUids, UserAssetDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", string.Format(_logMethodFormat, "FetchValidAssetUIds"));
                var result = await this._transactions.GetAsync<string>(
                    string.Format(Queries.FetchValidAssetUIDs,
                    string.Join(",", assetUids.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty)),
                    request);
                this._loggingService.Info(string.Format("Valid Asset UIDs for request : {0} - {1}", string.Join(",", assetUids), JsonConvert.SerializeObject(request)), string.Format(_logMethodFormat, "FetchValidAssetUIds"));
                this._loggingService.Debug("Ended executing query", string.Format(_logMethodFormat, "FetchValidAssetUIds"));
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

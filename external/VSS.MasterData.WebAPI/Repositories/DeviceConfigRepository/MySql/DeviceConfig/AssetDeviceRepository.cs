using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;
using Interfaces;
using DbModel;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using VSS.MasterData.WebAPI.Transactions;
using VSS.MasterData.WebAPI.DbModel;

namespace DeviceConfigRepository.MySql.DeviceConfig
{
    public class AssetDeviceRepository : IAssetDeviceRepository
    {
        private ILoggingService _loggingService;
        private ITransactions _transactions;

        public AssetDeviceRepository(ITransactions transactions, ILoggingService loggingService){
			_transactions = transactions;
            _loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        #region IAssetDeviceRepository implementation

        public Task<IEnumerable<AssetDeviceDto>> Fetch(string assetUIDs)
        {
            TraceMessage("Started Fetching AssetDevice Details");
            string query = string.Format(Queries.FetchAssetDeviceWithAssetUID, assetUIDs);
            TraceMessage(query);
            return _transactions.GetAsync<AssetDeviceDto>(query);
        }

        public async Task<AssetDeviceDto> FetchWithDeviceUID(string deviceUID)
        {
            TraceMessage("Started Fetching AssetDevice Details");
            string query = string.Format(Queries.FetchAssetDeviceWithDeviceUID, "UNHEX('"+deviceUID+"')");
            TraceMessage(query);
            var result = await _transactions.GetAsync<AssetDeviceDto>(query);
            return result.FirstOrDefault();
        }

        private void TraceMessage(string data, [CallerMemberName] string memberName=""){
            _loggingService.Info(data, memberName);
        }

        public Task<IEnumerable<AssetDeviceDto>> FetchByAssetUIDAndDeviceType(List<string> assetUIDs, string deviceType)
        {
            TraceMessage("Started Fetching AssetDevice Details");
            var assets = string.Join(",", assetUIDs.Select(x => "UNHEX('" + x + "')")).Replace("-", string.Empty);
            string query = string.Format(Queries.FetchAssetDeviceWithAssetUIDAndDeviceType, assets, deviceType);
            TraceMessage(query);
            return _transactions.GetAsync<AssetDeviceDto>(query);
        }

        #endregion

    }
}

using DbModel.DeviceConfig;
using Interfaces;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceConfigRepository.MySql.DeviceConfig
{
	public class DeviceRepository : IDeviceRepository
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;
        public const string INVALID_STRING_VALUE = "$#$#$";
        private readonly string _deviceConnectionString;

        public DeviceRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
            _deviceConnectionString = ConfigurationManager.ConnectionStrings["MySql.Device"].ConnectionString;
        }

        public async Task<DeviceData> Fetch(string serialNumber)
        {
            try
            {
                this._loggingService.Debug("Started executing query", "DeviceRepository.Fetch");
                var response = await this._transactions.GetAsync<DeviceData>(Queries.SELECT_DEVICE_BY_MODULECODE, new { SerialNumber = serialNumber });
                return response.FirstOrDefault();
            }
            catch (Exception ex)
            {
                this._loggingService.Error("Exception occurred while executing query", "DeviceRepository.Fetch", ex);
                throw ex;
            }
        } 

    }
}

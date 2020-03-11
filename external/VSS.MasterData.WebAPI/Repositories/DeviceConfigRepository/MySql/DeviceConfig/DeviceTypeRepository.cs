using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;
using Interfaces;
using DbModel;
using DbModel.DeviceConfig;
using VSS.MasterData.WebAPI.Transactions;

namespace DeviceConfigRepository.MySql.DeviceConfig
{
    public class DeviceTypeRepository : IDeviceTypeRepository
    {
        private readonly ILoggingService _loggingService;
        private readonly ITransactions _transactions;

        public DeviceTypeRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<IEnumerable<DeviceTypeDto>> FetchDeviceTypes(DeviceTypeDto deviceTypeDto)
        {
            try
            {
                return await this._transactions.GetAsync<DeviceTypeDto>(Queries.FetchDeviceTypeByTypeName, deviceTypeDto);
            }
            catch(Exception ex)
            {
                this._loggingService.Error("An Unhandled Exception has occurred", "DeviceTypeRepository.FetchDeviceTypes", ex);
                throw;
            }
        }

        public async Task<IEnumerable<DeviceTypeDto>> FetchAllDeviceTypes(DeviceTypeDto deviceTypeDto)
        {
            try
            {
                return await this._transactions.GetAsync<DeviceTypeDto>(Queries.FetchAllDeviceTypes, deviceTypeDto);
            }
            catch (Exception ex)
            {
                this._loggingService.Error("An Unhandled Exception has occurred", "DeviceTypeRepository.FetchAllDeviceTypes", ex);
                throw;
            }
        }
    }
}

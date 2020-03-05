using Newtonsoft.Json;
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
    public class DeviceParamGroupRepository : IDeviceParamGroupRepository
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;
        private const string logMethodFormat = "DeviceParamGroupRepository.{0}";

        public DeviceParamGroupRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<IEnumerable<DeviceParamGroupDto>> FetchDeviceTypeParamGroups(DeviceParamGroupDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                this._loggingService.Info("Device Type Parameter Groups for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                var result = await this._transactions.GetAsync<DeviceParamGroupDto>(Queries.FetchDeviceParameterGroups, request);

                this._loggingService.Info("Device Type Parameter Groups : " + JsonConvert.SerializeObject(result) , string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                this._loggingService.Debug("Ended executing query", string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                return result;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<DeviceParamGroupDto>> FetchDeviceParameterGroupById(DeviceParamGroupDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                this._loggingService.Info("Device Type Parameter Groups for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                var result = await this._transactions.GetAsync<DeviceParamGroupDto>(Queries.FetchParameterGroupById, request);

                this._loggingService.Info("Device Type Parameter Groups : " + JsonConvert.SerializeObject(result), string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                this._loggingService.Debug("Ended executing query", string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<DeviceParamGroupDto>> FetchAllDeviceParameterGroups(DeviceParamGroupDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", string.Format(logMethodFormat, "FetchAllDeviceParameterGroups"));

                this._loggingService.Info("Device Type Parameter Groups for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "FetchAllDeviceParameterGroups"));

                var result = await this._transactions.GetAsync<DeviceParamGroupDto>(Queries.FetchAllParameterGroups, request);

                this._loggingService.Info("Device Type Parameter Groups : " + JsonConvert.SerializeObject(result), string.Format(logMethodFormat, "FetchAllDeviceParameterGroups"));

                this._loggingService.Debug("Ended executing query", string.Format(logMethodFormat, "FetchAllDeviceParameterGroups"));

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
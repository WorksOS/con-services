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
    public class DeviceParamRepository : IDeviceParamRepository
    {
        private readonly ITransactions _transactions;
        private readonly ILoggingService _loggingService;
        private const string logMethodFormat = "DeviceParamRepository.{0}";

        public DeviceParamRepository(ITransactions transactions, ILoggingService loggingService)
        {
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<IEnumerable<DeviceParamDto>> FetchDeviceTypeParameters(DeviceParamDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", string.Format(logMethodFormat, "FetchDeviceTypeParameters"));

                this._loggingService.Info("Device Type Parameters for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "FetchDeviceTypeParameters"));
                IEnumerable<DeviceParamDto> result;
                
                if(request.ParameterGroupId == 4)
                    result = await this._transactions.GetAsync<DeviceParamDto>(Queries.FetchDeviceParametersWithAttributes, request);
                else 
                    result = await this._transactions.GetAsync<DeviceParamDto>(Queries.FetchDeviceParameters, request);

                this._loggingService.Info("Device Type Parameters : " + JsonConvert.SerializeObject(result), string.Format(logMethodFormat, "FetchDeviceTypeParameters"));

                this._loggingService.Debug("Ended executing query", string.Format(logMethodFormat, "FetchDeviceTypeParameters"));

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<DeviceParamDto>> FetchDeviceTypeParametersByDeviceType(DeviceParamDto request)
        {
            try
            {
                this._loggingService.Debug("Started executing query", string.Format(logMethodFormat, "FetchDeviceTypeParameters"));

                this._loggingService.Info("Device Type Parameters for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "FetchDeviceTypeParameters"));
                IEnumerable<DeviceParamDto> result;
                    result = await this._transactions.GetAsync<DeviceParamDto>(Queries.FetchDeviceParametersByDeviceTypeId, request);

                this._loggingService.Info("Device Type Parameters : " + JsonConvert.SerializeObject(result), string.Format(logMethodFormat, "FetchDeviceTypeParameters"));

                this._loggingService.Debug("Ended executing query", string.Format(logMethodFormat, "FetchDeviceTypeParameters"));

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
